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
    private BloodTexture bloodTexture;
    private Vector2 lastPlayerPos;
    private OverlayRender charOverlayRender;
    private Light2D overlayLight;

    public BloodTexture @BloodTexture { get { return bloodTexture; } }
    public OverlayRender @OverlayRender { get { return charOverlayRender; } }
    public Player @Player { get { return player; } }

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
    private PackedScene bloodPoolScene;

    //Other
    public List<Vector2[]> debugLines = new List<Vector2[]>();
    private string defaultKilledByText = "";
    public bool DrawPlayerBloodTrail = true;
    public RandomNumberGenerator rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        debugPoint = GD.Load<Texture>("res://textures/2x2_white.png");
        bloodPoolScene = GD.Load<PackedScene>("res://scenes/BloodPool.tscn");
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
        bloodTexture = GetNode<BloodTexture>("BloodTexture");
        charOverlayRender = GetNode<OverlayRender>("CharOverlayViewport/OverlayRender");
        overlayLight = GetNode<Light2D>("Player/OverlayLight");

        rng.Randomize();
        respawnBtn.Active = false;
        respawnBtn.Connect(nameof(MainMenuButton.Clicked), this, nameof(GoToMainMenu));

        player.Connect(nameof(Player.HealthChanged), this, nameof(UpdateHealthUI));
        UpdateHealthUI(player.currentHealth, player.maxHealth);
        player.Connect(nameof(Player.PlayerDied), this, nameof(PlayerDied));
        lastPlayerPos = player.Position;
        charOverlayRender.AddCharacter(player);

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

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (!levelGenerator.Done || player.isDead)
            return;

        // overlayLight.RangeLayerMin = Mathf.RoundToInt(player.GlobalPosition.y);

        // Check if player stood in strong blood
        bloodTexture.BloodCheckPos = player.Position;
        if (bloodTexture.CheckAlpha - 0.5f > player.BloodTrailAmount)
        {
            player.BloodTrailAmount = bloodTexture.CheckAlpha - 0.2f;
        }

        Vector2 newPlayerPos = player.Position + -(player.dir * 2.0f);

        // Trail blood
        if (player.BloodTrailAmount > 0.0f && DrawPlayerBloodTrail)
        {
            float dist = lastPlayerPos.DistanceTo(newPlayerPos);
            Vector2 randOffset = new Vector2(rng.RandfRange(-2, 2), rng.RandfRange(-2, 2));
            if (dist > 1.0f)
                if (player.BloodTrailAmount >= 0.5f)
                    bloodTexture.AddSweepedPlus(lastPlayerPos + randOffset, newPlayerPos, Mathf.Max(Mathf.RoundToInt(dist), 8), player.Position, 0.25f);
                else
                    bloodTexture.AddSweepedPoints(lastPlayerPos + randOffset, newPlayerPos, Mathf.Max(Mathf.RoundToInt(dist), 8), player.Position, 0.25f);
            else
            {
                if (player.BloodTrailAmount >= 0.5f)
                    bloodTexture.AddPlus(newPlayerPos + randOffset, player.Position, 0.25f);
                else
                    bloodTexture.AddPoint(newPlayerPos + randOffset);
            }
        }

        lastPlayerPos = newPlayerPos;
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
                    // TestSpawnNodeAtMouse<Pedestal>("res://scenes/Pedestal.tscn").GenerateItem();
                    // TestSpawnEnemyAtMouse<FireFly>("res://scenes/enemies/FireFly.tscn");

                    // TestSpawnNodeAtMouse<BloodPool>("res://scenes/BloodPool.tscn").Start(bloodTexture);

                    // PlayerGib head = TestSpawnNodeAtMouse<PlayerGib>("res://scenes/PlayerGibTorso.tscn");
                    // head.Init(bloodTexture, Vector2.Right * 50.0f, 0f);
                    // head.BounceTween(rng.RandfRange(-16f, -56));

                    AICharacter ff = TestSpawnNodeAtMouse<AICharacter>("res://scenes/enemies/Imp.tscn");
                    OverlayRender.AddCharacter(ff);

                }
            }
        }
        else if (evt is InputEventMouseMotion)
        {
            if (Input.IsMouseButtonPressed((int)ButtonList.Middle))
            {

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
        foreach (Node node in GetNode<CanvasLayer>("CanvasLayer").GetChildren())
        {
            if (node is CanvasItem canvasItem && !node.IsInGroup("death_gui"))
            {
                tween.InterpolateProperty(canvasItem, "modulate", Colors.White, Colors.Transparent, 0.25f, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
            }
        }

        // Dim scene
        tween.InterpolateProperty(GetNode<WorldEnvironment>("WorldEnvironment").Environment, "tonemap_exposure", 0.59f, 0.25f, 0.5f, Tween.TransitionType.Cubic, Tween.EaseType.InOut);

        // Show death GUI
        foreach (Node node in GetTree().GetNodesInGroup("death_gui"))
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

    public void SpawnBloodPool(Vector2 position)
    {
        BloodPool newPool = bloodPoolScene.Instance<BloodPool>();
        newPool.Position = position;
        AddChild(newPool);
        newPool.Start(bloodTexture);
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