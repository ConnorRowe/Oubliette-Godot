using Godot;
using System;

public class TreasureRoom : Room
{
    private Pedestal pedestal;
    [Export]
    private NodePath _pedestalPath;

    public override void _Ready()
    {
        base._Ready();

        pedestal = GetNode<Pedestal>(_pedestalPath);
    }

    public override void RoomEntered()
    {
        base.RoomEntered();

        pedestal.GenerateItem();
    }
}
