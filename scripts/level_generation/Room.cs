using Godot;
using System.Collections.Generic;

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
    public int enemyCounter;
    public Dictionary<Direction, AnimatedSprite> doors = new Dictionary<Direction, AnimatedSprite>();
    public List<AICharacter> enemies = new List<AICharacter>();

    [Export]
    private NodePath _wallTileMapPath;
    [Export]
    private NodePath _floorTileMapPath;
    [Export]
    public uint Width;
    [Export]
    public uint Height;

    public Dictionary<Direction, bool> connections = new Dictionary<Direction, bool>() { { Direction.Up, false }, { Direction.Right, false }, { Direction.Down, false }, { Direction.Left, false } };


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
        UnlockDoors(firstRoom);
    }

    public Rect2 AsRect()
    {
        return new Rect2(this.Position, new Vector2(Width * 16.0f, Height * 16.0f));
    }

    public void UpdateDoors()
    {
        foreach (KeyValuePair<Direction, bool> connection in connections)
        {
            if (!connection.Value)
            {
                doors[connection.Key].QueueFree();
                doors.Remove(connection.Key);
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

    public void EnemyDied()
    {
        enemyCounter--;

        if (enemyCounter <= 0)
        {
            UnlockDoors();

            EmitSignal(nameof(Cleared), this);
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
}
