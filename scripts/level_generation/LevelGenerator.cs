using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace Oubliette.LevelGen
{
    public class LevelGenerator : Node, IProvidesNav
    {
        private Camera2D camera;
        private bool canPan = false;
        private RandomNumberGenerator rng = new RandomNumberGenerator();
        private Player player;
        private World world;
        private Point currentRoomKey = Point.Empty;
        public Point CurrentRoomKey { get { return currentRoomKey; } }
        public Room CurrentRoom { get { return GeneratedRooms[currentRoomKey]; } }
        public bool Done = false;

        private List<PackedScene> possibleRooms = new List<PackedScene>();
        private List<PackedScene> treasureRooms = new List<PackedScene>();
        private List<PackedScene> allBosses = new List<PackedScene>();
        private PackedScene bossRoom;
        private PackedScene startingRoom;
        public Dictionary<Point, Room> GeneratedRooms { get; set; } = new Dictionary<Point, Room>();
        private int roomsToGen = 12;
        private readonly Point[] pointDirections = new Point[4] { new Point(0, 1), new Point(1, 0), new Point(0, -1), new Point(-1, 0) };
        private int gridWidth = 8;
        private int gridHeight = 8;
        private Sprite roomBorder;
        private TileMap wallTiles;
        private TileMap floorTiles;
        public Navigation2D NavMesh { get; set; }

        [Export]
        private string rooms_directory = "res://scenes/level_generation/rooms/";
        [Export]
        private string treasure_rooms_directory = "res://scenes/level_generation/treasure_rooms/";
        [Export]
        private string boss_enemies_directory = "res://scenes/enemies/bosses/";
        [Export]
        private Godot.Collections.Array<PackedScene> possibleEnemies = new Godot.Collections.Array<PackedScene>();
        [Export(hintString: "The tilemap to add wall tiles to.")]
        private NodePath _wallTileMapPath;
        [Export(hintString: "The tilemap to add floor tiles to.")]
        private NodePath _floorTileMapPath;
        [Export]
        private NodePath _navigationPath;

        [Signal]
        public delegate void RoomChanged(LevelGenerator levelGen);
        [Signal]
        public delegate void RoomCleared(int x, int y);

        public override void _Ready()
        {
            rng.Randomize();

            world = GetParent<World>();
            camera = world.GetNode<Camera2D>("Camera2D");
            player = world.GetNode<Player>("Player");
            roomBorder = GetNode<Sprite>("RoomBorder");
            wallTiles = GetNode<TileMap>(_wallTileMapPath);
            floorTiles = GetNode<TileMap>(_floorTileMapPath);
            NavMesh = GetNode<Navigation2D>(_navigationPath);

            bossRoom = GD.Load<PackedScene>("res://scenes/level_generation/BossRoom.tscn");
            startingRoom = GD.Load<PackedScene>("res://scenes/level_generation/StartingRoom.tscn");

            LoadFromDirectory<PackedScene>(rooms_directory, possibleRooms);
            LoadFromDirectory<PackedScene>(treasure_rooms_directory, treasureRooms);
            LoadFromDirectory<PackedScene>(boss_enemies_directory, allBosses);

            // Generate level
            CallDeferred(nameof(GenerateLevel));
        }

        public override void _Input(InputEvent evt)
        {
            base._Input(evt);

            if (evt is InputEventMouseMotion emm)
            {
                if (canPan)
                    camera.Position -= emm.Relative * 2.0f * camera.Zoom;
            }
            if (evt is InputEventMouseButton emb)
            {
                if (emb.ButtonIndex == (uint)ButtonList.Middle)
                {
                    canPan = emb.Pressed;
                }
                if (emb.ButtonIndex == (uint)ButtonList.WheelUp)
                {
                    camera.Zoom /= 2.0f;
                }
                if (emb.ButtonIndex == (uint)ButtonList.WheelDown)
                {
                    camera.Zoom *= 2.0f;
                }
            }
            if (evt is InputEventKey ek)
            {
                if (ek.Pressed)
                {
                    if (ek.Scancode == (uint)KeyList.Space)
                    {
                        // GenerateLevel();
                    }

                    if (false && ek.Scancode == (uint)KeyList.Enter)
                    {
                        if (player.Camera.Current)
                        {
                            this.camera.Current = true;
                        }
                        else
                        {
                            player.Camera.Current = true;
                        }
                    }
                }
            }
        }

        public static void LoadFromDirectory<T>(string fileDirectory, List<T> objectList) where T : Godot.Object
        {
            HashSet<T> objects = new HashSet<T>();
            LoadFromDirectory<T>(fileDirectory, objects);
            foreach (T o in objects)
            {
                objectList.Add(o);
            }
        }

        public static void LoadFromDirectory<T>(string fileDirectory, HashSet<T> objectSet) where T : Godot.Object
        {
            Directory directory = new Directory();
            directory.Open(fileDirectory);
            directory.ListDirBegin(skipNavigational: true);

            string file = directory.GetNext();
            do
            {
                // remove .import extension
                if (file.Substring(file.Length - 7) == ".import")
                    file = file.Substring(0, file.Length - 7);

                objectSet.Add(GD.Load<T>(fileDirectory + file));

                file = directory.GetNext();
            } while (!file.Empty());

        }

        private int[,] GenerateLevelGrid()
        {
            int[,] grid = new int[gridWidth, gridHeight];
            for (int x = 0; x < gridWidth; ++x)
            {
                for (int y = 0; y < gridHeight; ++y)
                {
                    grid[x, y] = -1;
                }
            }
            List<Point> rooms = new List<Point>();

            // Treasure room    = 1
            // Boss room        = 2
            // Starting room    = 3
            // at least one treasure room and one boss room must be generated
            List<int> requiredSpecialRooms = new List<int>() { 1, 2, 3 };

            for (int i = 0; i < roomsToGen; ++i)
            {
                if (rooms.Count == 0) // First room
                {
                    rooms.Add(new Point(rng.RandiRange(0, gridWidth - 1), rng.RandiRange(0, gridHeight - 1)));
                    grid[rooms[0].X, rooms[0].Y] = 0;
                }
                else // Rest of the rooms
                {
                    Point next = Point.Empty;
                    bool success = false;
                    while (!success)
                    {
                        Point start = rooms[rng.RandiRange(0, rooms.Count - 1)];
                        foreach (Point dir in ShuffledPointDirections())
                        {
                            if (start.X + dir.X >= 0 && start.X + dir.X < gridWidth
                            && start.Y + dir.Y >= 0 && start.Y + dir.Y < gridHeight)
                            {
                                if (grid[start.X + dir.X, start.Y + dir.Y] < 0)
                                {
                                    next = new Point(start.X + dir.X, start.Y + dir.Y);
                                    success = true;
                                    break;
                                }
                            }
                        }
                    }

                    rooms.Add(next);

                    int nextRoomID = 0;

                    // Generate special room if needed or at random
                    if (requiredSpecialRooms.Count > 0)
                    {
                        if (roomsToGen - i <= requiredSpecialRooms.Count)
                        {
                            int specialRoomIndex = rng.RandiRange(0, requiredSpecialRooms.Count - 1);
                            nextRoomID = requiredSpecialRooms[specialRoomIndex];
                            requiredSpecialRooms.RemoveAt(specialRoomIndex);
                        }
                        else
                        {
                            if (rng.Randf() < 0.25f)
                            {
                                int specialRoomIndex = rng.RandiRange(0, requiredSpecialRooms.Count - 1);
                                nextRoomID = requiredSpecialRooms[specialRoomIndex];
                                requiredSpecialRooms.RemoveAt(specialRoomIndex);
                            }
                        }
                    }

                    grid[next.X, next.Y] = nextRoomID;
                }
            }

            int roomCount = 0;

            for (int y = 0; y < gridHeight; ++y)
            {
                string row = "";
                for (int x = 0; x < gridWidth; ++x)
                {
                    row += " " + grid[x, y];

                    if (grid[x, y] >= 0)
                    {
                        ++roomCount;
                    }
                }

                GD.Print(row);
            }

            GD.Print("Generated " + roomCount + " rooms");

            return grid;
        }

        private Point[] ShuffledPointDirections()
        {
            return pointDirections.OrderBy(x => rng.Randi()).ToArray<Point>();
        }

        private void GenerateLevel()
        {
            foreach (KeyValuePair<Point, Room> room in GeneratedRooms)
            {
                room.Value.QueueFree();
            }
            GeneratedRooms.Clear();

            int[,] grid = GenerateLevelGrid();

            for (int y = 0; y < gridHeight; ++y)
            {
                for (int x = 0; x < gridWidth; ++x)
                {
                    if (grid[x, y] >= 0)
                    {
                        Room nextRoom = RandomRoomFromIndex(grid[x, y]);
                        nextRoom.Position = new Vector2(x, y) * nextRoom.Width * 16.0f;
                        nextRoom.Name = "Room_" + GeneratedRooms.Count;
                        nextRoom.RoomType = grid[x, y];
                        nextRoom.Visible = false;
                        nextRoom.BloodTexture.ResetImage();

                        GeneratedRooms.Add(new Point(x, y), nextRoom);
                        GetParent().CallDeferred("add_child", nextRoom);
                    }
                }
            }

            foreach (KeyValuePair<Point, Room> room in GeneratedRooms)
            {
                // Figure out connections
                foreach (Direction d in DirectionExt.Directions())
                {
                    Point pos = room.Key;
                    pos.Offset(d.AsPoint());
                    if (GeneratedRooms.ContainsKey(pos))
                    {
                        room.Value.Connections[d] = GeneratedRooms[pos].RoomType;
                    }
                }

                // Connect door collision
                room.Value.Connect(nameof(Room.DoorHit), this, nameof(DoorHit));

                // Spawn enemies
                foreach (Node2D point in room.Value.GetNode("EnemySpawnPoints").GetChildren())
                {
                    AI.AICharacter newEnemy = RandomEnemyInstance();
                    room.Value.AddChild(newEnemy);
                    newEnemy.NavProvider = this;
                    newEnemy.InitPos = room.Value.Position + point.Position;

                    room.Value.Enemies.Add(newEnemy);
                    room.Value.EnemyCounter++;

                    // Connect enemy die signal to room function
                    newEnemy.Connect(nameof(AI.AICharacter.Died), room.Value, nameof(Room.EnemyDied));
                    // Also connect to function here to remove their overlay
                    newEnemy.Connect(nameof(AI.AICharacter.Died), this, nameof(EnemyDied));
                }

                // If boss room spawn a random boss
                if (room.Value.RoomType == 2)
                {
                    AI.AICharacter boss = allBosses[rng.RandiRange(0, allBosses.Count - 1)].Instance<AI.AICharacter>();
                    room.Value.AddChild(boss);
                    boss.NavProvider = this;
                    boss.InitPos = room.Value.Position + new Vector2(177, 143);

                    room.Value.Enemies.Add(boss);
                    room.Value.EnemyCounter++;

                    boss.Connect(nameof(AI.AICharacter.Died), room.Value, nameof(Room.EnemyDied));
                    boss.Connect(nameof(AI.AICharacter.Died), this, nameof(EnemyDied));
                }

                // Manage ChanceSpawnChild nodes
                Node2D objectSpawnPoints = room.Value.GetNode<Node2D>("ObjectSpawnPoints");
                List<ChanceSpawnChild> chanceSpawns = objectSpawnPoints.GetChildren().OfType<ChanceSpawnChild>().ToList<ChanceSpawnChild>();

                for (int i = 0; i < chanceSpawns.Count; ++i)
                {
                    if (rng.Randf() <= chanceSpawns[i].SpawnChance)
                    {
                        Node2D child = (Node2D)chanceSpawns[i].GetChild(0);
                        child.Position = chanceSpawns[i].Position;
                        chanceSpawns[i].RemoveChild(child);
                        objectSpawnPoints.AddChild(child);
                    }

                    chanceSpawns[i].QueueFree();
                }

                // Connect cleared signal
                room.Value.Connect(nameof(Room.Cleared), this, nameof(RoomWasCleared));

                // Unify TileMaps by copying each tile from each room into the level generator's tilemaps
                foreach (Vector2 tile in room.Value.WallTiles.GetUsedCells())
                {
                    wallTiles.SetCellv((room.Value.Position / 16) + tile, room.Value.WallTiles.GetCell(Mathf.FloorToInt(tile.x), Mathf.FloorToInt(tile.y)));
                }
                foreach (Vector2 tile in room.Value.FloorTiles.GetUsedCells())
                {
                    floorTiles.SetCellv((room.Value.Position / 16) + tile, room.Value.FloorTiles.GetCell(Mathf.FloorToInt(tile.x), Mathf.FloorToInt(tile.y)));
                }

                room.Value.WallTiles.CallDeferred("queue_free");
                room.Value.FloorTiles.CallDeferred("queue_free");

                if (room.Value.RoomType == 3)
                {
                    // Put player in starting room
                    currentRoomKey = room.Key;
                    player.Position = CurrentRoom.Position + new Vector2(177, 143);
                    CurrentRoom.Visible = true;
                    CurrentRoom.FirstRoom = true;
                    roomBorder.Position = CurrentRoom.Position + new Vector2(176, 144);
                    CurrentRoom.RoomEntered();
                    CurrentRoom.BloodTexture.IsActive = true;
                }
            }

            wallTiles.UpdateBitmaskRegion();
            wallTiles.UpdateDirtyQuadrants();
            floorTiles.UpdateBitmaskRegion();
            floorTiles.UpdateDirtyQuadrants();

            // Update minimap
            EmitSignal(nameof(RoomChanged), this);
            EmitSignal(nameof(RoomCleared), currentRoomKey.X, currentRoomKey.Y);

            Done = true;
        }

        public void RegenerateLevel()
        {
            foreach (Node node in GetParent().GetChildren())
            {
                if (node is Room || node is AI.AICharacter || node is BasePickup)
                {
                    node.QueueFree();
                }
            }

            wallTiles.Clear();
            floorTiles.Clear();

            world.OverlayRender.ClearCharacters();
            world.OverlayRender.AddCharacter(player);

            GetParent().GetNode<Minimap>("CanvasLayer/Minimap").ClearMinimap();

            CallDeferred(nameof(GenerateLevel));
        }

        private Room RandomRoomFromIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return RandomRoomInstance(possibleRooms);
                case 1:
                    return RandomRoomInstance(treasureRooms);
                case 2:
                    return bossRoom.Instance<BossRoom>();
                case 3:
                    return startingRoom.Instance<Room>();
            }

            return null;
        }

        private Room RandomRoomInstance(List<PackedScene> roomScenesList)
        {
            return roomScenesList[rng.RandiRange(0, roomScenesList.Count - 1)].Instance<Room>();
        }

        private AI.AICharacter RandomEnemyInstance()
        {
            return possibleEnemies[rng.RandiRange(0, possibleEnemies.Count - 1)].Instance<AI.AICharacter>();
        }

        private void MoveRoom(Direction dir)
        {
            GeneratedRooms[currentRoomKey].Visible = false;
            GeneratedRooms[currentRoomKey].BloodTexture.IsActive = false;

            Point nextRoom = currentRoomKey;
            nextRoom.Offset(dir.AsPoint());

            player.GlobalPosition = GeneratedRooms[nextRoom].Doors[dir.Opposite()].GlobalPosition + (dir.AsVector() * 20.0f);

            GeneratedRooms[nextRoom].Visible = true;
            currentRoomKey = nextRoom;

            foreach (AI.AICharacter enemy in GeneratedRooms[currentRoomKey].Enemies)
            {
                enemy.TargetPlayer(player);
            }

            world.OverlayRender.AddCharacters(GeneratedRooms[currentRoomKey].Enemies.Cast<Character>().ToHashSet());

            roomBorder.Position = GeneratedRooms[currentRoomKey].Position + new Vector2(176, 144);

            EmitSignal(nameof(RoomChanged), this);

            GeneratedRooms[currentRoomKey].RoomEntered();
        }

        private void DoorHit(Direction dir)
        {
            MoveRoom(dir);
        }

        public Navigation2D GetNavigation()
        {
            return NavMesh;
        }

        private void RoomWasCleared(Room room)
        {
            Point roomKey = GeneratedRooms.First(x => x.Value.Position == room.Position).Key;
            EmitSignal(nameof(RoomCleared), roomKey.X, roomKey.Y);

            if (!room.ClearedByDefault)
            {
                player.ClearedRoom();
            }
        }

        public void EnemyDied(AI.AICharacter aICharacter)
        {
            world.OverlayRender.RemoveCharacter(aICharacter);
        }
    }
}