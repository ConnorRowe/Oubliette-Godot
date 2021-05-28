using Godot;
using System;

public class BossRoom : Room
{
    PackedScene pedestalScene;

    public override void _Ready()
    {
        base._Ready();

        pedestalScene = GD.Load<PackedScene>("res://scenes/Pedestal.tscn");
    }

    public override void RoomCleared()
    {
        base.RoomCleared();

        Pedestal bossItem = pedestalScene.Instance<Pedestal>();
        AddChild(bossItem);
        bossItem.Position = new Vector2(177, 143);
    }
}
