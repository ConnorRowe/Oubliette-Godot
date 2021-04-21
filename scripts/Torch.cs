using Godot;
using System;

public class Torch : Node2D
{
    [Export]
    private Curve flickerCurve;

    private Light2D light;

    private float count = 0.0f;

    public override void _Ready()
    {
        light = GetNode<Light2D>("Sprite/Light2D");
        GD.Randomize();
        count = GD.Randf();
    }
    
    public override void _Process(float delta)
    {
        // float time = ((float)OS.GetTicksMsec()) * 1000.0f;
        // float frac = time - Mathf.Floor(time);
        
        count += delta / 2.0f;
        if(count > 1.0f)
            count -= 1.0f;

        light.Energy = flickerCurve.InterpolateBaked(count) * 0.75f;
    }
}
