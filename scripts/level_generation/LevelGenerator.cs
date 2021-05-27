using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

public class LevelGenerator : Node, IProvidesNav
{
    private Camera2D camera;
    private bool canPan = false;
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    private Player player;
    private Point currentRoom = Point.Empty;
    public Point CurrentRoom { get { return currentRoom; } }

    private List<PackedScene> possibleRooms = new List<PackedScene>();
    private List<PackedScene> treasureRooms = new List<PackedScene>();
    public Dictionary<Point, Room> generatedRooms = new Dictionary<Point, Room>();
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

        camera = GetParent().GetNode<Camera2D>("Camera2D");
        player = GetParent().GetNode<Player>("Player");
        roomBorder = GetNode<Sprite>("RoomBorder");
        wallTiles = GetNode<TileMap>(_wallTileMapPath);
        floorTiles = GetNode<TileMap>(_floorTileMapPath);
        NavMesh = GetNode<Navigation2D>(_navigationPath);

        LoadRoomsFromDirectory(rooms_directory, possibleRooms);
        LoadRoomsFromDirectory(treasure_rooms_directory, treasureRooms);

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

                if (ek.Scancode == (uint)KeyList.Enter)
                {
                    if (player.camera.Current)
                    {
                        this.camera.Current = true;
                    }
                    else
                    {
                        player.camera.Current = true;
                    }
                }
            }
        }
    }

    private void LoadRoomsFromDirectory(string fileDirectory, List<PackedScene> roomScenesList)
    {
        Directory directory = new Directory();
        directory.Open(fileDirectory);
        directory.ListDirBegin(skipNavigational: true);

        string file = directory.GetNext();
        do
        {
            roomScenesList.Add(GD.Load<PackedScene>(fileDirectory + file));
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

        // at least one treasure room must be generated
        List<int> requiredSpecialRooms = new List<int>() { 1 };

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
        foreach (KeyValuePair<Point, Room> room in generatedRooms)
        {
            room.Value.QueueFree();
        }
        generatedRooms.Clear();

        int[,] grid = GenerateLevelGrid();

        for (int y = 0; y < gridHeight; ++y)
        {
            for (int x = 0; x < gridWidth; ++x)
            {
                if (grid[x, y] >= 0)
                {
                    Room nextRoom = RandomRoomFromIndex(grid[x, y]);
                    nextRoom.Position = new Vector2(x, y) * nextRoom.Width * 16.0f;
                    nextRoom.Name = "Room_" + generatedRooms.Count;
                    nextRoom.roomType = grid[x, y];

                    generatedRooms.Add(new Point(x, y), nextRoom);
                    GetParent().CallDeferred("add_child", nextRoom);
                }
            }
        }

        foreach (KeyValuePair<Point, Room> room in generatedRooms)
        {
            // Figure out connections
            foreach (Direction d in DirectionExt.Directions())
            {
                Point pos = room.Key;
                pos.Offset(d.AsPoint());
                if (generatedRooms.ContainsKey(pos))
                {
                    room.Value.connections[d] = generatedRooms[pos].roomType;
                }
            }

            // room.Value.Visible = false;

            // Connect door collision
            room.Value.Connect(nameof(Room.DoorHit), this, nameof(DoorHit));

            // Don't spawn enemies in first room
            if (room.Key != generatedRooms.First().Key)
            {
                // Spawn enemies
                foreach (Node2D point in room.Value.GetNode("EnemySpawnPoints").GetChildren())
                {
                    AICharacter newEnemy = RandomEnemyInstance();
                    room.Value.AddChild(newEnemy);
                    newEnemy.navProvider = this;
                    newEnemy.initPos = room.Value.Position + point.Position;

                    room.Value.enemies.Add(newEnemy);
                    room.Value.enemyCounter++;

                    // Connect enemy die signal to room function
                    newEnemy.Connect(nameof(AICharacter.Died), room.Value, nameof(Room.EnemyDied));
                }
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
        }

        wallTiles.UpdateBitmaskRegion();
        wallTiles.UpdateDirtyQuadrants();
        floorTiles.UpdateBitmaskRegion();
        floorTiles.UpdateDirtyQuadrants();

        // Put player in a room
        currentRoom = generatedRooms.First().Key;
        player.Position = generatedRooms[currentRoom].Position + new Vector2(8 * 16, 4 * 16);
        generatedRooms[currentRoom].Visible = true;
        generatedRooms[currentRoom].firstRoom = true;
        roomBorder.Position = generatedRooms[currentRoom].Position + new Vector2(176, 144);

        // Update minimap
        EmitSignal(nameof(RoomChanged), this);
        EmitSignal(nameof(RoomCleared), currentRoom.X, currentRoom.Y);
    }

    private Room RandomRoomFromIndex(int index)
    {
        switch (index)
        {
            case 0:
                return RandomRoomInstance(possibleRooms);
            case 1:
                return RandomRoomInstance(treasureRooms);
        }

        return null;
    }

    private Room RandomRoomInstance(List<PackedScene> roomScenesList)
    {
        return roomScenesList[rng.RandiRange(0, roomScenesList.Count - 1)].Instance<Room>();
    }

    private AICharacter RandomEnemyInstance()
    {
        return possibleEnemies[rng.RandiRange(0, possibleEnemies.Count - 1)].Instance<AICharacter>();
    }

    private void MoveRoom(Direction dir)
    {
        generatedRooms[currentRoom].Visible = false;

        Point nextRoom = currentRoom;
        nextRoom.Offset(dir.AsPoint());

        player.GlobalPosition = generatedRooms[nextRoom].doors[dir.Opposite()].GlobalPosition + (dir.AsVector() * 20.0f);

        generatedRooms[nextRoom].Visible = true;
        currentRoom = nextRoom;

        foreach (AICharacter enemy in generatedRooms[currentRoom].enemies)
        {
            enemy.TargetPlayer(player);
        }

        roomBorder.Position = generatedRooms[currentRoom].Position + new Vector2(176, 144);

        EmitSignal(nameof(RoomChanged), this);

        generatedRooms[currentRoom].RoomEntered();
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
        Point roomKey = generatedRooms.First(x => x.Value.Position == room.Position).Key;
        EmitSignal(nameof(RoomCleared), roomKey.X, roomKey.Y);
    }
}
