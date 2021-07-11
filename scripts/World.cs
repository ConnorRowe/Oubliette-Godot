using Godot;
using System;
using System.Collections.Generic;
using Oubliette.LevelGen;

namespace Oubliette
{
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
        private Items items;
        public ArtefactNamePopup artefactNamePopup { get; set; }
        private RichTextLabel killedByLabel;
        private MainMenuButton respawnBtn;
        private Vector2 lastPlayerPos;
        private OverlayRender charOverlayRender;
        private Light2D overlayLight;
        private AudioStreamPlayer sfxPlayer;

        public BloodTexture @BloodTexture { get { return levelGenerator.CurrentRoom.BloodTexture; } }
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
        private PackedScene bloodSplatScene;

        //Other
        public List<Vector2[]> debugLines { get; set; } = new List<Vector2[]>();
        private string defaultKilledByText = "";
        public bool DrawPlayerBloodTrail { get; set; } = true;
        private bool displayedDeathGUI = false;
        public LevelGenerator @LevelGenerator { get { return levelGenerator; } }

        public static RandomNumberGenerator rng = new RandomNumberGenerator();
        static World()
        {
            rng.Randomize();
        }

        public override void _Ready()
        {
            debugPoint = GD.Load<Texture>("res://textures/2x2_white.png");
            bloodPoolScene = GD.Load<PackedScene>("res://scenes/BloodPool.tscn");
            bloodSplatScene = GD.Load<PackedScene>("res://scenes/BloodSplat.tscn");
            camera = GetNode<Player>("Player").GetNode<Camera2D>("Camera2D");
            levelGenerator = GetNodeOrNull<LevelGenerator>(_levelPath);
            debugOverlay = GetNode<DebugOverlay>(_debugOverlayPath);
            debugOverlay.world = this;
            player = GetNode<Player>("Player");
            healthContainer = GetNode<HealthContainer>("CanvasLayer/HealthContainer");
            globalLighting = GetNode<CanvasModulate>(_globalLightingPath);
            tween = GetNode<Tween>(_tweenPath);
            items = GetNode<Items>("/root/Items");
            artefactNamePopup = GetNode<ArtefactNamePopup>(_artefactNamePopupPath);
            killedByLabel = GetNode<RichTextLabel>("CanvasLayer/KilledBy");
            respawnBtn = GetNode<MainMenuButton>("CanvasLayer/RespawnButton");
            charOverlayRender = GetNode<OverlayRender>("CharOverlayViewport/OverlayRender");
            overlayLight = GetNode<Light2D>("Player/OverlayLight");
            sfxPlayer = GetNode<AudioStreamPlayer>("SFXPlayer");

            respawnBtn.Active = false;
            respawnBtn.Connect(nameof(MainMenuButton.Clicked), this, nameof(GoToMainMenu));

            player.Connect(nameof(Player.HealthChanged), this, nameof(UpdateHealthUI));
            UpdateHealthUI(player.CurrentHealth, player.MaxHealth);
            player.Connect(nameof(Player.PlayerDied), this, nameof(PlayerDied));
            lastPlayerPos = player.Position;
            charOverlayRender.AddCharacter(player);

            defaultKilledByText = killedByLabel.BbcodeText;

            globalLighting.Color = defaultGlobalLighting;
        }

        public void PlayGlobalSoundEffect(AudioStream audioStream)
        {
            sfxPlayer.Stream = audioStream;
            sfxPlayer.Play(0);
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

            if (!levelGenerator.Done || player.IsDead || BloodTexture == null)
                return;

            // Check if player stood in strong blood
            BloodTexture.BloodCheckPos = player.Position;
            if (BloodTexture.CheckAlpha - 0.5f > player.BloodTrailAmount)
            {
                player.BloodTrailAmount = BloodTexture.CheckAlpha - 0.2f;
                player.BloodTrailColour = BloodTexture.BloodCheckColour;
            }

            Vector2 newPlayerPos = player.Position + -(player.Dir * 2.0f);

            // Trail blood
            if (player.BloodTrailAmount > 0.0f && DrawPlayerBloodTrail)
            {
                // Lower blood opacity when amount is low
                Color weakBlood = player.BloodTrailColour;
                weakBlood.a = Math.Min(1.0f, player.BloodTrailAmount);

                float posDelta = lastPlayerPos.DistanceTo(newPlayerPos);
                Vector2 randOffset = new Vector2(rng.RandfRange(-2, 2), rng.RandfRange(-2, 2));

                if (posDelta > 1.0f)
                    if (player.BloodTrailAmount >= 0.5f)
                        BloodTexture.AddSweepedPlus(lastPlayerPos + randOffset, newPlayerPos, Math.Max(Mathf.RoundToInt(posDelta), 8), player.Position, weakBlood);
                    else
                        BloodTexture.AddSweepedPixels(lastPlayerPos + randOffset, newPlayerPos, Math.Max(Mathf.RoundToInt(posDelta), 8), player.Position, weakBlood);
                else
                {
                    if (player.BloodTrailAmount >= 0.5f)
                        BloodTexture.AddPlus(newPlayerPos + randOffset, player.Position, weakBlood);
                    else
                        BloodTexture.AddPixel(newPlayerPos + randOffset, weakBlood);
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
                        if (emb.Shift)
                        {
                        }
                        else
                        {
                        }
                    }
                    if (emb.ButtonIndex == (int)ButtonList.WheelDown)
                    {
                        // healthContainer.ChangeHealth(-1);
                        if (emb.Shift)
                        {
                        }
                        else
                        {
                        }

                    }
                    if (emb.ButtonIndex == (int)ButtonList.Middle)
                    {
                        // BottleSmashEffect bottleTest = TestSpawnNodeAtMouse<BottleSmashEffect>("res://scenes/BottleSmashEffect.tscn");
                        // bottleTest.Start(new Vector2(rng.RandfRange(2.0f, 3.5f) * (rng.Randf() <= 0.5f ? -1.0f : 1.0f), rng.RandfRange(-1.0f, 1.0f)), rng.RandfRange(350.0f, 500.0f));

                        // TestSpawnEnemyAtMouse<AI.AICharacter>("res://scenes/enemies/Imp.tscn").Position += (Vector2.Up * 32.0f);
                        // TestSpawnEnemyAtMouse<AI.AICharacter>("res://scenes/enemies/FireFly.tscn").Position += (Vector2.Left * 32.0f);
                        // TestSpawnEnemyAtMouse<AI.AICharacter>("res://scenes/enemies/Slime.tscn").Position += (Vector2.Right * 32.0f);
                        // TestSpawnEnemyAtMouse<AI.AICharacter>("res://scenes/enemies/Snail.tscn").Position += (Vector2.Down * 32.0f);


                        if (emb.Shift)
                        {
                            TestSpawnNodeAtMouse<Pedestal>("res://scenes/Pedestal.tscn").GenerateItem();
                        }
                        else
                        {
                            // TestSpawnEnemyAtMouse<AI.GoblinWBearTrap>("res://scenes/enemies/GoblinWBearTrap.tscn");

                            player.PickUpPrimarySpell(Spells.Spells.GoldenShower);
                        }
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

        private void UpdateHealthUI(float currentHealth, float maxHealth)
        {
            healthContainer.SetHealth(Mathf.RoundToInt(currentHealth), Mathf.RoundToInt(maxHealth));
        }

        private void PlayerDied(Player player)
        {
            if (displayedDeathGUI)
                return;

            displayedDeathGUI = true;

            // Stop enemy AI
            foreach (AI.AICharacter enemy in levelGenerator.CurrentRoom.Enemies)
            {
                enemy.HasTarget = false;
                enemy.AIManager.SetCurrentBehaviour("idle");
                enemy.AIManager.StopTryTransitionLoop();
            }

            // Zoom in on player
            tween.InterpolateProperty(player.Camera, "zoom", new Vector2(0.333f, 0.333f), new Vector2(0.1f, 0.1f), 0.25f, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
            tween.InterpolateProperty(player.Camera, "offset", Vector2.Zero, new Vector2(0, -16), 0.25f);

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
            // Reset item pools
            items.ResetItemPools();

            // Reset worldenvironment resource
            GetNode<WorldEnvironment>("WorldEnvironment").Environment.TonemapExposure = 0.59f;

            GetTree().ChangeSceneTo(GD.Load<PackedScene>("res://scenes/MainMenu.tscn"));

            QueueFree();
        }

        public BloodPool SpawnBloodPool(Vector2 position, Color bloodColour)
        {
            BloodPool bloodPool = bloodPoolScene.Instance<BloodPool>();
            AddChild(bloodPool);
            bloodPool.Position = position;
            bloodPool.BloodColour = bloodColour;
            bloodPool.Start(BloodTexture);

            return bloodPool;
        }

        public BloodSplat SpawnBloodSplat(Vector2 position, Color bloodColour)
        {
            BloodSplat bloodSplat = bloodSplatScene.Instance<BloodSplat>();
            AddChild(bloodSplat);
            bloodSplat.Position = position;
            bloodSplat.Init(BloodTexture, new Vector2(rng.RandfRange(-2.0f, 2.0f), rng.RandfRange(-1.0f, 1.0f)), bloodColour);

            return bloodSplat;
        }

        // For testing purposes only. vvv
        private T TestSpawnNodeAtMouse<T>(NodePath scenePath) where T : Node2D
        {
            T instance = GD.Load<PackedScene>(scenePath).Instance<T>();
            instance.Position = camera.GetGlobalMousePosition();
            AddChild(instance);

            return instance;
        }

        private T TestSpawnEnemyAtMouse<T>(string scenePath) where T : AI.AICharacter
        {
            T enemy = GD.Load<PackedScene>(scenePath).Instance<T>();
            AddChild(enemy);
            enemy.Position = camera.GetGlobalMousePosition();
            enemy.NavProvider = GetNode<LevelGenerator>("LevelGenerator");
            enemy.TargetPlayer(player);

            return enemy;
        }
    }
}