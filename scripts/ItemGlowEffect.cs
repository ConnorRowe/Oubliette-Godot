using Godot;
using System.Collections.Generic;

public class ItemGlowEffect : Node2D
{
    private HashSet<Sprite> sprites = new HashSet<Sprite>();
    private float count;

    [Export]
    private float rotation_adjust = 2.0f;
    [Export]
    private float speed_scale = 0.25f;
    [Export]
    private float baseHueAdjust = 0.897f;

    public override void _Ready()
    {
        foreach (Node node in GetChildren())
        {
            if (node is Sprite sprite)
            {
                sprites.Add(sprite);
            }
        }
    }

    public override void _Process(float delta)
    {
        count += (delta * speed_scale);
        if (count > 1.0f)
        {
            count -= 1.0f;
        }

        bool updatedShader = false;

        foreach (Sprite sprite in sprites)
        {
            sprite.RotationDegrees += (Mathf.Cos(count * Mathf.Pi * 2.0f) * rotation_adjust);

            if (!updatedShader)
            {
                (sprite.Material as ShaderMaterial).SetShaderParam("hue_shift", baseHueAdjust + ((Mathf.Sin(count * Mathf.Pi * 2.0f) + 1.0f) / 2.0f) * 0.04f);
                updatedShader = true;
            }
        }
    }
}
