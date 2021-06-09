using Godot;
using System.Collections.Generic;

namespace Oubliette.LevelGen
{
    public class Room : Node2D
    {
        [Signal]
        public delegate void DoorHit(Direction dir);
        [Signal]
        public delegate void Cleared(Room room);

        private TileMap wallTiles;
        private TileMap floorTiles;

        public TileMap WallTiles
        {
            get
            {
                if (wallTiles == null)
                {
                    wallTiles = GetNode<TileMap>(_wallTileMapPath);
                }
                return wallTiles;
            }
        }
        public TileMap FloorTiles
        {
            get
            {
                if (floorTiles == null)
                {
                    floorTiles = GetNode<TileMap>(_floorTileMapPath);
                }
                return floorTiles;
            }
        }

        public bool firstRoom = false;
        public int roomType = -1;
        public int enemyCounter;
        public Dictionary<Direction, AnimatedSprite> doors = new Dictionary<Direction, AnimatedSprite>();
        public HashSet<AI.AICharacter> enemies = new HashSet<AI.AICharacter>();

        [Export]
        private NodePath _wallTileMapPath;
        [Export]
        private NodePath _floorTileMapPath;
        [Export]
        public uint Width;
        [Export]
        public uint Height;
        [Export]
        public bool ClearedByDefault = false;

        public Dictionary<Direction, int> connections = new Dictionary<Direction, int>() { { Direction.Up, -1 }, { Direction.Right, -1 }, { Direction.Down, -1 }, { Direction.Left, -1 } };


        public override void _Ready()
        {
            base._Ready();

            doors.Add(Direction.Up, GetNode<AnimatedSprite>("Doors/Up"));
            doors.Add(Direction.Right, GetNode<AnimatedSprite>("Doors/Right"));
            doors.Add(Direction.Down, GetNode<AnimatedSprite>("Doors/Down"));
            doors.Add(Direction.Left, GetNode<AnimatedSprite>("Doors/Left"));

            // Removes doors that arent connected
            UpdateDoors();

            // Connect to door collision
            foreach (KeyValuePair<Direction, AnimatedSprite> door in doors)
            {
                door.Value.GetNode("Area2D").Connect("body_entered", this, nameof(DoorOverlap), new Godot.Collections.Array() { door.Key });
            }

            // Lock doors if not first room
            UnlockDoors(firstRoom || ClearedByDefault);
        }

        public Rect2 AsRect()
        {
            return new Rect2(this.Position, new Vector2(Width * 16.0f, Height * 16.0f));
        }

        public virtual void RoomEntered()
        {
            if (ClearedByDefault)
                EmitSignal(nameof(Cleared), this);
        }

        public void UpdateDoors()
        {
            foreach (KeyValuePair<Direction, int> connection in connections)
            {
                if (connection.Value < 0)
                {
                    doors[connection.Key].QueueFree();
                    doors.Remove(connection.Key);
                }
                else if (connection.Value > 0)
                {
                    switch (connection.Value)
                    {
                        case 1:
                            doors[connection.Key].Modulate = new Color(1, 0.937255f, 0);
                            break;
                    }
                }
            }
        }

        public void DoorOverlap(Node2D node, Direction dir)
        {
            if (node is Player)
            {
                EmitSignal(nameof(DoorHit), dir);
            }
        }

        public void EnemyDied(AI.AICharacter aICharacter)
        {
            enemyCounter--;

            if (enemyCounter <= 0)
            {
                UnlockDoors();

                RoomCleared();
            }
        }

        public void UnlockDoors(bool unlock = true)
        {
            foreach (var door in doors)
            {
                door.Value.Frame = unlock ? 1 : 0;
                door.Value.GetChild<Area2D>(0).Monitorable = unlock;
                door.Value.GetChild<Area2D>(0).Monitoring = unlock;
            }
        }

        public virtual void RoomCleared()
        {
            EmitSignal(nameof(Cleared), this);
        }
    }
}