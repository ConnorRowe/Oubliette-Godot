using Godot;
using System;

public class MajykaContainer : Node2D
{
    private int _maxMajyka = 100;
    private int _currentMajyka = 100;

    [Export(PropertyHint.Range, "0,100,1,or_greater")]
    public int MaxMajyka
    {
        get
        {
            return _maxMajyka;
        }
        set
        {
            if (_maxMajyka != value)
            {
                _maxMajyka = value;
                UpdateBar();
            }
        }
    }

    [Export(PropertyHint.Range, "0,100,1,or_greater")]
    public int CurrentMajyka
    {
        get
        {
            return _currentMajyka;
        }
        set
        {
            if (_currentMajyka != value)
            {
                _currentMajyka = value;
                UpdateBar();
            }
        }
    }

    [Export]
    private NodePath _fillPath;
    private Node2D fill;

    public override void _Ready()
    {
        base._Ready();

        fill = GetNode<Node2D>(_fillPath);
    }

    private void UpdateBar()
    {
        if (fill?.Material is ShaderMaterial shader)
        {
            shader.SetShaderParam("fill_percent", ((float)_currentMajyka) / ((float)_maxMajyka));
        }
    }

    // [Export(PropertyHint.ResourceType, "Texture")]
    // Texture background;
    // [Export(PropertyHint.ResourceType, "Texture")]
    // Texture foreground;
    // [Export(PropertyHint.ResourceType, "Texture")]
    // Texture fillMask;

    // private const int endCapacity = 6;
    // private readonly Color maskColour = new Color(1.0f, 0.0f, 1.0f);

    // public override void _Draw()
    // {
    //     base._Draw();

    //     if (background != null && foreground != null && fillMask != null)
    //     {
    //         DrawLayer(background);

    //         if (_currentMajyka > 0)
    //         {
    //             int x = Math.Min(_currentMajyka, endCapacity);
    //             DrawTextureRectRegion(fillMask, new Rect2(Position + new Vector2(2*Scale.x,0), new Vector2(x*Scale.x, 16*Scale.y)), new Rect2(2, 0, x, 16));
    //         }

    //         if (_currentMajyka > endCapacity)
    //         {
    //             DrawTextureRectRegion(fillMask, new Rect2(Position + new Vector2((endCapacity + 2) * Scale.x, 0), new Vector2(Math.Min(_currentMajyka - endCapacity, _maxMajyka - 12), 16) * Scale.x), new Rect2(7, 0, 2, 16));
    //             //DrawRect(new Rect2(Position + new Vector2(endCapacity + 2, 0), new Vector2(Math.Min(_currentMajyka - endCapacity, _maxMajyka - 12), 16)), maskColour);
    //         }

    //         if (_currentMajyka > MaxMajyka - 6)
    //         {
    //             int x = _currentMajyka - (MaxMajyka - 6);
    //             DrawTextureRectRegion(fillMask, new Rect2(Position + new Vector2((_maxMajyka - endCapacity + 2) * Scale.x, 0), new Vector2(x*Scale.x, 16*Scale.y)), new Rect2(8, 0, x, 16));
    //         }

    //         DrawLayer(foreground);
    //     }
    // }

    // private void DrawLayer(Texture tex)
    // {
    //     DrawTextureRectRegion(tex, new Rect2(Position, new Vector2(8*Scale.x, 16*Scale.y)), new Rect2(0, 0, 8, 16));

    //     DrawTextureRectRegion(tex, new Rect2(Position + new Vector2(8*Scale.x, 0), new Vector2(_maxMajyka - (endCapacity * 2), 16) * Scale), new Rect2(7, 0, 2, 16));

    //     DrawTextureRectRegion(tex, new Rect2(Position + new Vector2((_maxMajyka - (endCapacity) + 2) * Scale.x, 0), new Vector2(8, 16) * Scale), new Rect2(8, 0, 8, 16));

    // }
}