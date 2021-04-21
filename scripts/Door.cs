using Godot;
using System;

public class Door : Node2D, IInteractible
{
    private Sprite closedSprite;
    private Sprite openSprite;
    private StaticBody2D closedBody;
    private Area2D openArea;
    private LightOccluder2D closedOccluder;
    private LightOccluder2D openOccluder;


    public override void _Ready()
    {
        closedSprite = GetNode<Sprite>("ClosedSprite");
        openSprite = GetNode<Sprite>("OpenSprite");
        closedBody = GetNode<StaticBody2D>("ClosedSprite/ClosedBody");
        closedOccluder = GetNode<LightOccluder2D>("ClosedSprite/ClosedOccluder");
        openOccluder = GetNode<LightOccluder2D>("OpenSprite/OpenOccluder");
        openArea = GetNode<Area2D>("OpenSprite/OpenArea");
    }

    public bool Interact()
    {
        Toggle();

        return true;
    }

    public void Toggle()
    {
        closedSprite.Visible = !closedSprite.Visible;
        openSprite.Visible = !openSprite.Visible;

        if (closedSprite.Visible)
        {
            closedBody.CollisionLayer = 0b0011;
            closedBody.CollisionMask = 1;
            closedOccluder.LightMask = 1;
            openOccluder.LightMask = 0;
            openArea.CollisionMask = 0;
            openArea.CollisionLayer = 0;
        }
        else
        {
            closedBody.CollisionLayer = 0;
            closedBody.CollisionMask = 0;
            closedOccluder.LightMask = 0;
            openOccluder.LightMask = 1;
            openArea.CollisionMask = 1;
            openArea.CollisionLayer = 0b0011;

        }

        closedOccluder.Update();
    }
}
