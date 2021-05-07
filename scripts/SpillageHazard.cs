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
                Modulate = Modulate.LinearInterpolate(new Color(Modulate, 0.0f), delta * 2.0f);
            }

            if(Modulate.a < 0.05)
            {
                QueueFree();
            }
        }
    }
}