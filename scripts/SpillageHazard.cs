using Godot;
using System;

public class SpillageHazard : Area2D
{
    public float lifeTime = 2.0f;

    public override void _Process(float delta)
    {
        lifeTime -= delta;

        if (lifeTime <= 0.0f)
        {
            if (Modulate.a > 0)
            {
                Modulate = new Color(Modulate, Mathf.Lerp(Modulate.a, 0.0f, delta * 2.0f));
            }

            if (Modulate.a < 0.05)
            {
                QueueFree();
            }
        }
    }

    public void SetColours(Color spillageColour, Color bubbleColor)
    {
        GetNode<Sprite>("Sprite").SelfModulate = spillageColour;
        GetNode<Particles2D>("Bubbles").SelfModulate = bubbleColor;
    }
}