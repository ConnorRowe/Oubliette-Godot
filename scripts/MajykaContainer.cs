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
    private ProgressBar spellCooldownBar;

    public override void _Ready()
    {
        base._Ready();

        fill = GetNode<Node2D>(_fillPath);
        spellCooldownBar = GetNode<ProgressBar>("SpellCooldownBar");
    }

    private void UpdateBar()
    {
        if (fill?.Material is ShaderMaterial shader)
        {
            shader.SetShaderParam("fill_percent", ((float)_currentMajyka) / ((float)_maxMajyka));
        }
    }

    public void UpdateSpellCooldown(float fillPercent)
    {
        spellCooldownBar.Value = fillPercent;
    }
}