using Godot;
using System;
using System.Collections.Generic;

public class World : Node2D
{
    //Nodes
    private Camera2D camera;
    private LevelGenerator levelGenerator;
    private DebugOverlay debugOverlay;
    private Player player;
    private HealthContainer healthContainer;
    private CanvasModulate globalLighting;
    private Tween tween;
    private AudioStreamPlayer musicPlayer;
    private Items items;
    public ArtefactNamePopup artefactNamePopup;
    private RichTextLabel killedByLabel;
    private MainMenuButton respawnBtn;

    [Export]
    private NodePath _basicAIPath;
    [Export]
    private NodePath _levelPath;
    [Export]
    private NodePath _debugOverlayPath;
    [Export]
    private NodePath _globalLightingPath;
    [Export]
    private NodePath _tweenPath;
    [Export]
    private Color defaultGlobalLighting = new Color(0.15f, 0.15f, 0.15f, 1);
    [Export]
    private NodePath _artefactNamePopupPath;

    //Assets
    private Texture debugPoint;

    //Other
    public List<Vector2[]> debugLines = new List<Vector2[]>();
    private string defaultKilledByText = "";

    public override void _Ready()
    {
        debugPoint = GD.Load<Texture>("res://textures/2x2_white.png");
        camera = GetNode<Player>("Player").GetNode<Camera2D>("Camera2D");
        levelGenerator = GetNodeOrNull<LevelGenerator>(_levelPath);
        debugOverlay = GetNode<DebugOverlay>(_debugOverlayPath);
        debugOverlay.world = this;
        player = GetNode<Player>("Player");
        healthContainer = GetNode<HealthContainer>("CanvasLayer/HealthContainer");
        globalLighting = GetNode<CanvasModulate>(_globalLightingPath);
        tween = GetNode<Tween>(_tweenPath);
        musicPlayer = GetNode<AudioStreamPlayer>("MusicPlayer");
        items = GetNode<Items>("/root/Items");
        artefactNamePopup = GetNode<ArtefactNamePopup>(_artefactNamePopupPath);
        killedByLabel = GetNode<RichTextLabel>("CanvasLayer/KilledBy");
        respawnBtn = GetNode<MainMenuButton>("CanvasLayer/RespawnButton");

        respawnBtn.Active = false;
        respawnBtn.Connect(nameof(MainMenuButton.Clicked), this, nameof(GoToMainMenu));

        player.Connect(nameof(Player.HealthChanged), this, nameof(UpdateHealthUI));
        UpdateHealthUI(player.currentHealth, player.maxHealth);
        player.Connect(nameof(Player.PlayerDied), this, nameof(PlayerDied));

        defaultKilledByText = killedByLabel.BbcodeText;

        globalLighting.Color = defaultGlobalLighting;
        musicPlayer.Play();
    }

    public DebugOverlay GetDebugOverlay()
    {
        if (debugOverlay == null)
        {
            debugOverlay = GetNode<DebugOverlay>(_debugOverlayPath);
            return debugOverlay;
        }
        else
        {
            return debugOverlay;
        }
    }

    public override void _Input(InputEvent evt)
    {
        base._Input(evt);

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
                if (emb.ButtonIndex == (int)ButtonList.Middle)
                {
                    // Chest chest = GD.Load<PackedScene>("res://scenes/Chest.tscn").Instance<Chest>();
                    // AddChild(chest);
                    // chest.Position = camera.GetGlobalMousePosition();

                    // Pedestal pedestal = GD.Load<PackedScene>("res://scenes/Pedestal.tscn").Instance<Pedestal>();
                    // AddChild(pedestal);
                    // pedestal.Position = camera.GetGlobalMousePosition();
                    // pedestal.GenerateItem();

                    // PotionPickup testPotion = items.GetRandomPotionPickup();
                    // AddChild(testPotion);
                    // testPotion.Position = camera.GetGlobalMousePosition();

                    TestSpawnEnemyAtMouse<FireFly>("res://scenes/enemies/FireFly.tscn");
                }
            }
        }
    }

    public override void _Draw()
    {
        base._Draw();

        // Draw debug lines
        foreach (Vector2[] line in debugLines)
        {
            for (int i = 0; i < line.Length - 1; ++i)
            {
                DrawLine(line[i], line[i + 1], Colors.Magenta, 10.0f);
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
        // return navigation.GetSimplePath(pointA, pointB, optimize: true);
        return null;
    }

    private void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        healthContainer.SetHealth(currentHealth, maxHealth);
    }

    private void PlayerDied(Player player)
    {
        // Stop enemy AI
        foreach (AICharacter enemy in levelGenerator.CurrentRoom.enemies)
        {
            enemy.hasTarget = false;
            enemy.aIManager.SetCurrentBehaviour("idle");
            enemy.aIManager.StopTryTransitionLoop();
        }

        // Zoom in on player
        tween.InterpolateProperty(player.camera, "zoom", new Vector2(0.333f, 0.333f), new Vector2(0.1f, 0.1f), 0.25f, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
        tween.InterpolateProperty(player.camera, "offset", Vector2.Zero, new Vector2(0, -16), 0.25f);
        
        // Hide GUI
        foreach(Node node in GetNode<CanvasLayer>("CanvasLayer").GetChildren())
        {
            if(node is CanvasItem canvasItem && !node.IsInGroup("death_gui"))
            {
                tween.InterpolateProperty(canvasItem, "modulate", Colors.White, Colors.Transparent, 0.25f, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
            }
        }

        // Dim scene
        tween.InterpolateProperty(GetNode<WorldEnvironment>("WorldEnvironment").Environment, "tonemap_exposure", 0.59f, 0.25f, 0.5f, Tween.TransitionType.Cubic, Tween.EaseType.InOut);

        // Show death GUI
        foreach(Node node in GetTree().GetNodesInGroup("death_gui"))
        {
            (node as CanvasItem).Visible = true;
            tween.InterpolateProperty(node, "modulate", Colors.Transparent, Colors.White, 0.25f);
        }
        
        // Activate respawn button
        respawnBtn.Active = true;

        // Show who killed player
        GetNode<RichTextLabel>("CanvasLayer/KilledBy").BbcodeText = String.Format(defaultKilledByText, player.KilledBy);

        tween.Start();
    }

    private void GoToMainMenu()
    {
        GetTree().ChangeSceneTo(GD.Load<PackedScene>("res://scenes/MainMenu.tscn"));

        // Reset item pools
        items.ResetItemPools();

        // Reset worldenvironment resource
        GetNode<WorldEnvironment>("WorldEnvironment").Environment.TonemapExposure = 0.59f;
    }

    // For testing purposes only. vvv
    private T TestSpawnNodeAtMouse<T>(NodePath scenePath) where T : Node2D
    {
        T instance = GD.Load<PackedScene>(scenePath).Instance<T>();
        instance.Position = camera.GetGlobalMousePosition();
        AddChild(instance);

        return instance;
    }

    private T TestSpawnEnemyAtMouse<T>(string scenePath) where T : AICharacter
    {
        T enemy = GD.Load<PackedScene>(scenePath).Instance<T>();
        AddChild(enemy);
        enemy.Position = camera.GetGlobalMousePosition();
        enemy.TargetPlayer(player);
        enemy.navProvider = GetNode<LevelGenerator>("LevelGenerator");

        return enemy;
    }
}