using Godot;
using System;

public class World : Node2D
{
    //Nodes
    private Navigation2D navigation;
    private Line2D line;
    private Camera2D camera;
    private Node2D level;
    private DebugOverlay debugOverlay;
    private Player player;
    private HealthContainer healthContainer;

    [Export]
    private NodePath _linePath;
    [Export]
    private NodePath _basicAIPath;
    [Export]
    private NodePath _levelPath;
    [Export]
    private NodePath _debugOverlayPath;

    //Assets
    private Texture debugPoint;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()   
    {
        line = GetNodeOrNull<Line2D>(_linePath);
        debugPoint = GD.Load<Texture>("res://textures/2x2_white.png");
        camera = GetNode<Player>("Player").GetNode<Camera2D>("Camera2D");
        level = GetNode<Node2D>(_levelPath);
        navigation = level.GetNode<Navigation2D>("Navigation2D");
        debugOverlay = GetNode<DebugOverlay>(_debugOverlayPath);
        player = GetNode<Player>("Player");
        healthContainer = GetNode<HealthContainer>("CanvasLayer/HealthContainer");

        player.Connect(nameof(Player.HealthChanged), this, nameof(UpdateHealthUI));
        UpdateHealthUI(player.currentHealth, player.maxHealth);

        debugOverlay.TrackProperty("CurrentMajyka", player);
    }

    public DebugOverlay GetDebugOverlay()
    {
        if(debugOverlay == null)
        {
            debugOverlay = GetNode<DebugOverlay>(_debugOverlayPath);
            return debugOverlay;
        }
        else
        {
            return debugOverlay;
        }
    }

    public override void _UnhandledInput(InputEvent evt)
    {
        base._UnhandledInput(evt);

        if (evt is InputEventMouseButton emb)
        {
            if (emb.ButtonIndex == 1 && emb.Pressed)
            {
                // Vector2[] newPath = navigation.GetSimplePath(aiTest.GlobalPosition, camera.GetGlobalMousePosition());

                // line.Points = newPath;
                // aiTest.SetPath(newPath);

                // AddDebugPoint(camera.GetGlobalMousePosition(), Colors.DarkBlue);
            }
            else if (emb.IsPressed())
            {
                if (emb.ButtonIndex == (int)ButtonList.WheelUp)
                {
                    // healthContainer.ChangeHealth(1);
                }
                if (emb.ButtonIndex == (int)ButtonList.WheelDown)
                {
                    // healthContainer.ChangeHealth(-1);
                }
            }
        }
    }

    public void AddDebugPoint(Vector2 pos, Color colour)
    {
        var pnt = new Sprite()
        {
            Texture = debugPoint,
            Position = pos,
            Modulate = colour,
            ZIndex = 10,
        };

        GetParent().AddChild(pnt);
    }

    public Vector2[] GetNavPath(Vector2 pointA, Vector2 pointB)
    {
        return navigation.GetSimplePath(pointA, pointB, optimize: true);
    }

    private void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        healthContainer.SetHealth(currentHealth, maxHealth);
    }
}
