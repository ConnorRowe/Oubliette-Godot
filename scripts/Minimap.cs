using Godot;
using System.Collections.Generic;
using System.Drawing;

public class Minimap : Node2D
{
    struct RoomMapData
    {
        public bool cleared;

        public RoomMapData(bool cleared)
        {
            this.cleared = cleared;
        }
    }

    private Vector2 halfSize;

    private Dictionary<Point, RoomMapData> discoveredRooms = new Dictionary<Point, RoomMapData>();
    private Point currentRoom = new Point();

    [Export]
    private Vector2 size;
    [Export]
    private Godot.Color backgroundColour = new Godot.Color(0x45283cff);
    [Export]
    private Texture minimapBorder;
    [Export]
    private Texture currentRoomOverlay;
    [Export]
    private Texture roomIcons;
    [Export]
    private int roomIconsHCount;
    [Export]
    private int roomIconsVCount;

    private Vector2 roomIconSize = Vector2.Zero;

    public override void _Ready()
    {
        halfSize = size / 2;
        roomIconSize = new Vector2(roomIcons.GetSize().x / roomIconsHCount, roomIcons.GetSize().y / roomIconsVCount);

        LevelGenerator levelGen = GetParent().GetParent().GetNode<LevelGenerator>("LevelGenerator");

        levelGen.Connect(nameof(LevelGenerator.RoomChanged), this, nameof(RoomChanged));
        levelGen.Connect(nameof(LevelGenerator.RoomCleared), this, nameof(RoomCleared));
    }

    public override void _Draw()
    {
        base._Draw();

        // Background
        DrawRect(new Rect2(Position, size), backgroundColour);

        Vector2 roomSize = new Vector2(22.0f, 18.0f) / 2;
        Vector2 roomHalfSize = roomSize / 2;

        // Rooms
        foreach (var room in discoveredRooms)
        {
            Point roomFrame = new Point(1, 0);
            if (room.Value.cleared)
                roomFrame = new Point(0, 0);

            Rect2 roomRect = new Rect2(Position.x + halfSize.x - roomHalfSize.x + ((room.Key.X - currentRoom.X) * roomSize.x), Position.y + halfSize.y - roomHalfSize.y + ((room.Key.Y - currentRoom.Y) * roomSize.y), roomSize.x, roomSize.y);

            DrawTextureRectRegion(roomIcons, roomRect, new Rect2((roomIconSize.x * roomFrame.X), roomIconSize.y * roomFrame.Y, roomIconSize.x, roomIconSize.y));

            if (room.Key == currentRoom)
            {
                DrawTextureRect(currentRoomOverlay, roomRect, false);
            }

        }

        // Border
        DrawTexture(minimapBorder, Position, Modulate);

        // Clip rect so rooms outside minimap aren't rendered
        RID rID = GetCanvasItem();
        VisualServer.CanvasItemSetCustomRect(rID, true, new Rect2(Position, size));
        VisualServer.CanvasItemSetClip(rID, true);
    }

    private void RoomChanged(LevelGenerator levelGen)
    {
        currentRoom = levelGen.CurrentRoom;

        foreach (Direction dir in DirectionExt.Directions())
        {
            Point newRoom = currentRoom;
            newRoom.Offset(dir.AsPoint());

            if (levelGen.generatedRooms.ContainsKey(newRoom) && !discoveredRooms.ContainsKey(newRoom))
            {
                discoveredRooms.Add(newRoom, new RoomMapData(false));
            }
        }

        Update();
    }

    private void RoomCleared(int x, int y)
    {
        discoveredRooms[new Point(x, y)] = new RoomMapData(true);

        Update();
    }
}