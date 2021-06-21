using Godot;
using System.Collections.Generic;
using Oubliette.Stats;
using Oubliette.Spells;

namespace Oubliette
{
    public class Player : Character, ICastsSpells
    {
        private float jumpPower = 175f;
        private float scrollScale = 1.0f;
        private BaseSpell primarySpell;
        private BaseSpell secondarySpell;
        private SceneTreeTimer spellEffectTimer;
        private SceneTreeTimer takeDmgTimer;
        private bool canTakeDamage = true;
        private float primarySpellCooldown = 0.0f;
        public float maxPrimarySpellCooldown = 0.5f;
        private bool isSpellActive = false;
        private float maxMajyka = 100.0f;
        private float currentMajyka = 100.0f;
        public float staffRot = 0.0f;
        public List<Artefact.ArtefactTextureSet> artefactTextureSets = new List<Artefact.ArtefactTextureSet>();
        public string KilledBy = "";
        private Vector2 armSocket = Vector2.Zero;
        public Vector2 facingDir = Vector2.Zero;
        private Godot.Collections.Dictionary<Direction, Vector2> staffOrigins = new Godot.Collections.Dictionary<Direction, Vector2>() { { Direction.Up, new Vector2(4.0f, -10.0f) }, { Direction.Right, new Vector2(0.0f, -8.0f) }, { Direction.Down, new Vector2(-4.0f, -10.0f) }, { Direction.Left, new Vector2(0.0f, -10.0f) } };
        private Godot.Collections.Dictionary<Direction, Vector2> armOrigins = new Godot.Collections.Dictionary<Direction, Vector2>() { { Direction.Up, new Vector2(2.5f, -11.0f) }, { Direction.Right, new Vector2(-0.5f, -11.0f) }, { Direction.Down, new Vector2(-2.5f, -11.0f) }, { Direction.Left, new Vector2(0.5f, -11.0f) } };
        private Physics2DShapeQueryParameters hitAreaShapeQuery;
        private HashSet<Node> intersectedAreas = new HashSet<Node>();
        private Potion currentPotion;
        private float pickupCooldown = 0.0f;
        public HashSet<(Color colour, float weight)> SpellColourMods { get; set; } = new HashSet<(Color colour, float weight)>();
        private Color primarySpellColourCache;
        public float BloodTrailAmount { get; set; } = 0.0f;
        public Color BloodTrailColour { get; set; } = Colors.Transparent;
        public HashSet<BuffTracker> PerRoomBuffs { get; set; } = new HashSet<BuffTracker>();
        public static Color PlayerBloodColour = new Color(0.760784f, 0, 0.101961f);

        // Nodes
        public Camera2D Camera { get; set; }
        private Particles2D spellParticle;
        private MajykaContainer majykaBar;
        private Sprite staff;
        public World @World { get; set; }
        private Line2D arm;
        private Line2D armOutline;
        private Light2D staffLight;
        private Node2D spellSpawnPoint;
        private ItemDisplaySlot potionSlot;
        private ItemDisplaySlot primarySpellSlot;
        private PlayerGib headGib;
        private AudioStreamPlayer hitSoundPlayer;
        private AudioStreamPlayer spellSoundPlayer;
        private AudioStreamPlayer potionSoundPlayer;
        private GridContainer buffTrackerContainer;
        private Particles2D gsTest;

        // Input
        private bool inputMoveUp = false;
        private bool inputMoveDown = false;
        private bool inputMoveLeft = false;
        private bool inputMoveRight = false;

        // Assets
        private Texture debugPoint;
        private PackedScene projectileScene;
        private PackedScene potionScene;
        private PackedScene spellPickupScene;
        private Stack<PackedScene> deathGibs = new Stack<PackedScene>();
        private List<AudioStreamSample> hitSounds = new List<AudioStreamSample>();
        private AudioStreamRandomPitch spellCastSound;
        private AudioStreamRandomPitch gulpSound;
        private List<AudioStreamSample> burpSounds = new List<AudioStreamSample>();
        private PackedScene bottleSmashEffectScene;
        private PackedScene buffTrackerScene;

        // Export
        [Export]
        private NodePath _cameraPath;
        [Export]
        private Shape2D hitBoxTraceShape;

        // Signals
        [Signal]
        public delegate void PlayerDied(Player player);
        [Signal]
        public delegate void PlayerDamaged(int damage);

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();

            gsTest = GetNode<Particles2D>("CharSprite/staff/StaffLight/gstest");

            debugPoint = GD.Load<Texture>("res://textures/2x2_white.png");
            projectileScene = GD.Load<PackedScene>("res://scenes/Projectile.tscn");
            potionScene = GD.Load<PackedScene>("res://scenes/Potion.tscn");
            spellPickupScene = GD.Load<PackedScene>("res://scenes/SpellPickup.tscn");
            buffTrackerScene = GD.Load<PackedScene>("res://scenes/BuffTracker.tscn");

            // load player death gibs
            deathGibs.Push(GD.Load<PackedScene>("res://scenes/PlayerGibHead.tscn"));
            deathGibs.Push(GD.Load<PackedScene>("res://scenes/PlayerGibArm.tscn"));
            deathGibs.Push(GD.Load<PackedScene>("res://scenes/PlayerGibArm.tscn"));
            deathGibs.Push(GD.Load<PackedScene>("res://scenes/PlayerGibTorso.tscn"));
            deathGibs.Push(GD.Load<PackedScene>("res://scenes/PlayerGibLeg.tscn"));
            deathGibs.Push(GD.Load<PackedScene>("res://scenes/PlayerGibLeg.tscn"));

            // load player hit sounds
            LevelGen.LevelGenerator.LoadFromDirectory<AudioStreamSample>("res://sound/sfx/player_hit/", hitSounds);

            spellCastSound = new AudioStreamRandomPitch();
            spellCastSound.AudioStream = GD.Load<AudioStreamSample>("res://sound/sfx/player_spell_cast_mixdown.wav");
            spellCastSound.RandomPitch = 1.1f;

            gulpSound = new AudioStreamRandomPitch();
            gulpSound.AudioStream = GD.Load<AudioStreamSample>("res://sound/sfx/gulp_mixdown.wav");

            // load burp sounds
            LevelGen.LevelGenerator.LoadFromDirectory<AudioStreamSample>("res://sound/sfx/burp/", burpSounds);

            bottleSmashEffectScene = GD.Load<PackedScene>("res://scenes/BottleSmashEffect.tscn");

            majykaBar = GetParent().GetNode<MajykaContainer>("CanvasLayer/MajykaContainer");
            spellParticle = GetNode<Particles2D>("CharSprite/SpellParticle");
            spellParticle.Material = new ShaderMaterial();
            staff = GetNode<Sprite>("CharSprite/staff");
            Camera = GetNode<Camera2D>(_cameraPath);
            World = GetParent<World>();
            arm = GetNode<Line2D>("CharSprite/Arm");
            armOutline = GetNode<Line2D>("CharSprite/ArmOutline");
            staffLight = GetNode<Light2D>("CharSprite/staff/StaffLight");
            spellSpawnPoint = GetNode<Node2D>("CharSprite/staff/SpellSpawnPoint");
            potionSlot = World.GetNode<ItemDisplaySlot>("CanvasLayer/PotionSlot");
            primarySpellSlot = World.GetNode<ItemDisplaySlot>("CanvasLayer/PrimarySpellSlot");
            hitSoundPlayer = GetNode<AudioStreamPlayer>("HitSoundPlayer");
            spellSoundPlayer = GetNode<AudioStreamPlayer>("SpellSoundPlayer");
            potionSoundPlayer = GetNode<AudioStreamPlayer>("PotionSoundPlayer");
            buffTrackerContainer = World.GetNode<GridContainer>("CanvasLayer/BuffTrackerContainer");

            Items items = GetNode<Items>("/root/Items");
            var magicMissile = items.FindSpellPoolEntry(Spells.Spells.MagicMissile, Items.LootPool.GENERAL);
            PickUpPrimarySpell(magicMissile.spell);
            items.RemoveSpellFromPools(magicMissile);
            secondarySpell = Spells.Spells.IceSkin;

            CachePrimarySpellColour();

            DebugOverlay debug = World.GetDebugOverlay();
            debug.TrackFunc(nameof(GetSpellDamage), this, "DMG", 1);
            debug.TrackFunc(nameof(GetSpellRange), this, "RNG", 1);
            debug.TrackFunc(nameof(GetSpellKnockback), this, "KBK", 1);
            debug.TrackFunc(nameof(GetSpellSpeed), this, "SPD", 1);

            hitAreaShapeQuery = new Physics2DShapeQueryParameters();
            hitAreaShapeQuery.SetShape(hitBoxTraceShape);
            hitAreaShapeQuery.Transform = new Transform2D(0.0f, GlobalPosition + new Vector2(0, -9.0f));
            hitAreaShapeQuery.CollideWithAreas = true;
            hitAreaShapeQuery.CollideWithBodies = true;
            hitAreaShapeQuery.CollisionLayer = 512;
            hitAreaShapeQuery.Exclude = new Godot.Collections.Array() { this };

            CheckSlideCollisions = true;

            UpdatePotionSlot();
        }

        public override void _UnhandledInput(InputEvent evt)
        {
            base._UnhandledInput(evt);

            if (IsDead)
                return;

            if (evt is InputEventKey keyEvt)
            {
                if (keyEvt.Pressed)
                {
                    if (evt.IsActionPressed("g_interact"))
                    {
                        TryInteract();
                    }
                    if (evt.IsActionPressed("g_move_up"))
                    {
                        inputMoveUp = true;
                    }
                    if (evt.IsActionPressed("g_move_down"))
                    {
                        inputMoveDown = true;
                    }
                    if (evt.IsActionPressed("g_move_left"))
                    {
                        inputMoveLeft = true;
                    }
                    if (evt.IsActionPressed("g_move_right"))
                    {
                        inputMoveRight = true;
                    }
                    if (evt.IsActionPressed("g_use_potion"))
                    {
                        ConsumeCurrentPotion();
                    }
                    if (keyEvt.Scancode == (int)KeyList.K)
                    {
                        // suicide
                        TakeDamage(9999, sourceName: "suicide");
                    }
                }
                else
                {
                    if (evt.IsActionReleased("g_move_up"))
                    {
                        inputMoveUp = false;
                    }
                    if (evt.IsActionReleased("g_move_down"))
                    {
                        inputMoveDown = false;
                    }
                    if (evt.IsActionReleased("g_move_left"))
                    {
                        inputMoveLeft = false;
                    }
                    if (evt.IsActionReleased("g_move_right"))
                    {
                        inputMoveRight = false;
                    }
                }

                // Jump
                if (evt.IsActionPressed("g_jump"))
                {
                    if (elevation == 0)
                    {
                        jumpVelocity = jumpPower;
                    }
                }
            }

            if (evt is InputEventMouseButton emb)
            {
                if (emb.IsPressed())
                {
                    if (emb.ButtonIndex == (int)ButtonList.WheelUp)
                    {
                        scrollScale += 0.1f;
                    }
                    if (emb.ButtonIndex == (int)ButtonList.WheelDown)
                    {
                        scrollScale -= 0.1f;
                    }
                }
            }

            if (evt is InputEventMouseMotion emm)
            {
                Vector2 gMPos = Camera.GetGlobalMousePosition();
                facingDir = new Vector2(gMPos.x - GlobalPosition.x, gMPos.y - GlobalPosition.y).Normalized();
                staffRot = Mathf.Atan2(facingDir.y, facingDir.x);
            }
        }

        public override Vector2 GetInputAxis(float delta)
        {
            Vector2 newDir = new Vector2();

            if (inputMoveUp)
            {
                newDir.y -= 1;
            }
            if (inputMoveDown)
            {
                newDir.y += 1;
            }
            if (inputMoveLeft)
            {
                newDir.x -= 1;
            }
            if (inputMoveRight)
            {
                newDir.x += 1;
            }

            return newDir;
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            if (Input.IsActionPressed("g_cast_primary_spell"))
            {
                if (currentMajyka >= GetSpellCost(primarySpell.MajykaCost) && primarySpellCooldown == 0.0f)
                {
                    // Cast primary spell
                    primarySpell.Cast(this);

                    spellSoundPlayer.Stream = spellCastSound;
                    spellSoundPlayer.Play(0);

                    currentMajyka -= GetSpellCost(primarySpell.MajykaCost);
                    UpdateMajykaBar();

                    primarySpellCooldown = GetMaxPrimarySpellCooldown();

                }
            }
            if (Input.IsActionPressed("g_cast_secondary_spell"))
            {
                CastSecondarySpell();
            }

            // Follow head gib
            if (IsDead && headGib != null)
            {
                Camera.Offset = (headGib.GlobalPosition - Position) + new Vector2(0, -16);
            }

            if (IsDead)
                return;

            if (BloodTrailAmount > 0.0f)
            {
                BloodTrailAmount -= delta * 2.0f;

                if (BloodTrailAmount < 0.0f)
                {
                    BloodTrailAmount = 0.0f;
                }
            }

            if (primarySpellCooldown > 0.0f)
            {
                primarySpellCooldown -= delta;
            }

            if (primarySpellCooldown < 0.0f)
                primarySpellCooldown = 0.0f;

            if (pickupCooldown > 0.0f)
                pickupCooldown -= delta;

            if (pickupCooldown < 0.0f)
                pickupCooldown = 0.0f;

            Direction fDir = GetFacingDirection();

            staff.Rotation = staffRot;
            staff.Position = staffOrigins[fDir] + (facingDir * 4.0f);

            arm.Points = new Vector2[] { armOrigins[fDir] + (charSprite.Frame % 3 == 0 ? new Vector2(0, -1) : Vector2.Zero), staff.Position };
            armOutline.Points = arm.Points;

            staff.ShowBehindParent = fDir == Direction.Up;

            if (currentMajyka < maxMajyka)
                RegenMajyka(delta);

            float primarySpellCDPercent = primarySpellCooldown / GetMaxPrimarySpellCooldown();
            if (primarySpellCDPercent < 1.0f)
                majykaBar.UpdateSpellCooldown(primarySpellCDPercent);
            else
                majykaBar.UpdateSpellCooldown(0.0f);


            // Update staff glow intensity
            (staff.Material as ShaderMaterial).SetShaderParam("intensity", Mathf.Lerp(6.0f, 0.0f, primarySpellCooldown / maxPrimarySpellCooldown));

            Update();

            UpdateGSTest();
        }

        public override void _Draw()
        {
            DrawEquippedArtefacts(GetFacingDirection());
        }

        public override void Die()
        {
            if (IsDead)
                return;

            KilledBy = lastDamagedBy;

            staff.RemoveChild(staffLight);
            AddChild(staffLight);
            staffLight.Position = Vector2.Zero;
            staffLight.Color = Colors.White;
            staffLight.Energy = 1.0f;

            var rng = World.rng;

            LevelGen.BloodTexture bloodTexture = World.BloodTexture;
            foreach (PackedScene gibScene in deathGibs)
            {
                PlayerGib gib = gibScene.Instance<PlayerGib>();
                if (gib.isHead)
                {
                    headGib = gib;
                }
                World.AddChild(gib);
                Vector2 gibOffset = new Vector2(rng.Randf(), rng.Randf()).Normalized() * 2.0f;
                gib.Position = Position + gibOffset;
                float gibDir = rng.Randf() > 0.5f ? -1.0f : 1.0f;
                bool shouldBounce = rng.Randf() > 0.25f || gib.isHead;
                gib.Init(bloodTexture, new Vector2(rng.RandfRange(30.0f, 50.0f) * gibDir, 10.0f), shouldBounce ? (rng.RandfRange(350.0f, 500.0f) * gibDir) : 0.0f);
                if (shouldBounce)
                {
                    gib.BounceTween(rng.RandfRange(-16f, -56));
                }


            }

            charSprite.Visible = false;
            shadowSprite.Visible = false;

            base.Die();

            EmitSignal(nameof(PlayerDied), this);
        }

        public override float GetMaxSpeed()
        {
            return IsDead ? 0.0f : base.GetMaxSpeed();
        }

        private void DrawEquippedArtefacts(Direction direction)
        {
            foreach (Artefact.ArtefactTextureSet textureSet in artefactTextureSets)
            {
                Texture artefactTex = textureSet.TextureFromDirection(direction);

                if (artefactTex != null)
                {
                    Rect2 rect = new Rect2(textureSet.Offset + (charSprite.Frame % 3 == 0 ? new Vector2(0, -1) : Vector2.Zero) + new Vector2(0, -elevation), artefactTex.GetSize());
                    if (direction == Direction.Up || direction == Direction.Left)
                        rect.Size = new Vector2(rect.Size.x * -1.0f, rect.Size.y);

                    DrawTextureRect(artefactTex, rect, false);
                }
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            var spaceState = GetWorld2d().DirectSpaceState;
            var result = spaceState.IntersectRay(staffLight.GlobalPosition, staffLight.GlobalPosition + (facingDir * 4.0f), new Godot.Collections.Array() { this }, collisionLayer: 0b0001, collideWithAreas: true);

            if (result.Count > 0)
            {
                // Move down staff
                staffLight.Position -= new Vector2(120.0f * delta, 0);
            }
            else
            {
                staffLight.Position = staffLight.Position.LinearInterpolate(new Vector2(6, 0), delta * 2.0f);
            }

            // Custom area hit detection because godot's is unreliable

            hitAreaShapeQuery.Transform = new Transform2D(0.0f, GlobalPosition + new Vector2(0, -8.0f));
            Godot.Collections.Array hitResult = spaceState.IntersectShape(hitAreaShapeQuery);
            HashSet<Node> hitNodes = new HashSet<Node>();

            foreach (Godot.Collections.Dictionary hit in hitResult)
            {
                Node hitNode = ((Node)hit["collider"]);
                hitNodes.Add(hitNode);

                if (!intersectedAreas.Contains(hitNode))
                {
                    intersectedAreas.Add(hitNode);

                    if (hitNode is IIntersectsPlayerHitArea hitable)
                    {
                        hitable.PlayerHit(this);
                        GD.Print("Player hit " + hitNode.Name);
                    }
                    else if (hitNode.GetParent() is IIntersectsPlayerHitArea parentHitable)
                    {
                        parentHitable.PlayerHit(this);
                        GD.Print("Player hit " + hitNode.GetParent().Name);
                    }
                }
            }

            // remove intersections that no longer exist
            intersectedAreas.RemoveWhere((Node node) => { return !hitNodes.Contains(node); });
        }

        public override void OnSlideCollision(KinematicCollision2D kinematicCollision, Vector2 slideVelocity)
        {
            base.OnSlideCollision(kinematicCollision, slideVelocity);

            if (kinematicCollision.Collider is IIntersectsPlayerHitArea hittable)
            {
                hittable.PlayerHit(this);
            }
        }

        private bool TryInteract()
        {
            GD.Print("TryInteract():");

            Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;

            Vector2 rayStart = this.GlobalPosition + new Vector2(0, -9);
            Vector2 rayEnd = rayStart;

            switch (GetDirection(Dir))
            {
                case Direction.Up:
                    {
                        rayEnd += new Vector2(0, -20);
                        break;
                    }
                case Direction.Right:
                    {
                        rayEnd += new Vector2(20, 0);
                        break;
                    }
                case Direction.Down:
                    {
                        rayEnd += new Vector2(0, 20);
                        break;
                    }
                case Direction.Left:
                    {
                        rayEnd += new Vector2(-20, 0);
                        break;
                    }
            }

            var result = spaceState.IntersectRay(this.GlobalPosition + new Vector2(0, -9), rayEnd, new Godot.Collections.Array { this }, collisionLayer: 0b0010, collideWithAreas: true);

            Sprite debug = new Sprite();
            debug.Texture = debugPoint;
            GetParent().AddChild(debug);
            debug.Modulate = Colors.Red;
            debug.Position = rayEnd;

            if (result.Count > 0)
            {
                Node collider = (Node)result["collider"];
                GD.Print("Collided with " + collider.Name);

                debug.Modulate = Colors.Green;
                debug.Position = (Vector2)result["position"];


                if (collider is IInteractible)
                {
                    (collider as IInteractible).Interact();

                    GD.Print("Interacted with " + collider.Name);

                    return true;
                }
                else if (collider.Owner is IInteractible)
                {
                    (collider.Owner as IInteractible).Interact();

                    GD.Print("Interacted with " + collider.Owner.Name);

                    return true;
                }
            }

            return false;
        }

        public float GetSpellCost(float baseCost)
        {
            return (baseCost + currentStats[Stat.MagykaCostFlat]) * currentStats[Stat.MagykaCostMultiplier];
        }

        public void CastSecondarySpell()
        {
            if (currentMajyka >= GetSpellCost(secondarySpell.MajykaCost))
            {
                secondarySpell.Cast(this);

                spellSoundPlayer.Stream = spellCastSound;
                spellSoundPlayer.Play(0);

                currentMajyka -= GetSpellCost(secondarySpell.MajykaCost);
                UpdateMajykaBar();
            }
        }

        private void RegenMajyka(float delta)
        {
            currentMajyka += (25.0f * delta * GetStatValue(Stat.MagykaRegenMultiplayer));

            if (currentMajyka > maxMajyka)
                currentMajyka = maxMajyka;

            UpdateMajykaBar();
        }

        private void UpdateMajykaBar()
        {
            majykaBar.CurrentMajyka = Mathf.RoundToInt((currentMajyka / maxMajyka) * 100.0f);
        }

        private void ResetCanTakeDamage()
        {
            canTakeDamage = true;
        }

        public override void TakeDamage(int damage = 1, Character source = null, string sourceName = "")
        {
            if (canTakeDamage)
            {
                BloodTrailAmount += damage;
                BloodTrailColour = PlayerBloodColour;

                hitSoundPlayer.Stream = hitSounds[World.rng.RandiRange(0, hitSounds.Count - 1)];
                hitSoundPlayer.Play(0);

                base.TakeDamage(damage, source, sourceName);
                canTakeDamage = false;
                takeDmgTimer = GetTree().CreateTimer(0.25f, false);
                takeDmgTimer.Connect("timeout", this, nameof(ResetCanTakeDamage));

                EmitSignal(nameof(PlayerDamaged), damage);
            }
        }

        public override Direction GetFacingDirection()
        {
            return GetDirection(facingDir);
        }

        public void UpdatePlayerSpellEffects(Shader effectShader, Color outlineColour, float duration)
        {
            ShaderMaterial mat = (ShaderMaterial)charSprite.Material;

            mat.SetShaderParam("outline_width", 1.0f);
            mat.SetShaderParam("outline_colour", outlineColour);
            armOutline.Visible = true;
            armOutline.DefaultColor = outlineColour;

            ShaderMaterial pMat = (ShaderMaterial)spellParticle.Material;

            pMat.Shader = effectShader;
            pMat.SetShaderParam("base_colour", outlineColour);
            spellParticle.Emitting = true;

            spellEffectTimer?.Disconnect("timeout", this, nameof(DispelPlayerSpellEffects));
            spellEffectTimer = GetTree().CreateTimer(duration);
            spellEffectTimer.Connect("timeout", this, nameof(DispelPlayerSpellEffects));
        }

        public void DispelPlayerSpellEffects()
        {
            ShaderMaterial mat = (ShaderMaterial)charSprite.Material;

            mat.SetShaderParam("outline_width", 0.0f);
            armOutline.Visible = false;

            spellParticle.Emitting = false;
        }

        public Vector2 GetSpellDirection()
        {
            return facingDir;
        }

        public Vector2 GetSpellSpawnPos()
        {
            return spellSpawnPoint.GlobalPosition;
        }

        public Color GetSpellColour(Color baseColour)
        {
            return primarySpellColourCache;
        }

        public int GetSpellDamage(int baseDamage)
        {
            return Mathf.RoundToInt((baseDamage + currentStats[Stat.DamageFlat]) * currentStats[Stat.DamageMultiplier]);
        }

        public float GetSpellRange(float baseRange)
        {
            return baseRange * currentStats[Stat.RangeMultiplier];
        }

        public float GetSpellKnockback(float baseKnockback)
        {
            return baseKnockback * currentStats[Stat.KnockbackMultiplier];
        }

        public float GetSpellSpeed(float baseSpeed)
        {
            return baseSpeed * currentStats[Stat.SpellSpeedMultiplier];
        }

        private float GetMaxPrimarySpellCooldown()
        {
            return (maxPrimarySpellCooldown + currentStats[Stat.CooldownFlat]) * currentStats[Stat.CooldownMultplier];
        }

        public bool PickUpPotion(Potion newPotion)
        {
            if (pickupCooldown > 0.0f)
            {
                return false;
            }

            if (currentPotion != null)
            {
                // Drop current potion

                PotionPickup droppedPotion = potionScene.Instance<PotionPickup>();
                droppedPotion.potion = currentPotion;
                World.AddChild(droppedPotion);
                droppedPotion.Position = Position;
                droppedPotion.ApplyCentralImpulse(Dir * 60f);
            }

            currentPotion = newPotion;

            World.artefactNamePopup?.DisplayPopup(newPotion.name, newPotion.desc);

            UpdatePotionSlot();

            pickupCooldown = 0.5f;

            return true;
        }

        private void ConsumeCurrentPotion()
        {
            if (currentPotion == null)
                return;

            PlayGulpSound();

            GetTree().CreateTimer(0.25f).Connect("timeout", this, nameof(ThrowEmptyPotion));

            if (World.rng.Randf() <= 0.2f)
            {
                GetTree().CreateTimer(1.0f).Connect("timeout", this, nameof(PlayBurpSound));
            }

            BuffTracker tracker = ApplyPerRoomBuff(currentPotion.name, new HashSet<(Stat stat, float amount)>(currentPotion.stats), currentPotion.duration);

            tracker.ItemIcon.Texture = potionSlot.Texture;

            tracker.ItemIcon.Material = (ShaderMaterial)potionSlot.Material.Duplicate();
            ShaderMaterial mat = (ShaderMaterial)tracker.ItemIcon.Material;
            mat.SetShaderParam("colour_lerp_a", currentPotion.lerpColours[0]);
            mat.SetShaderParam("colour_lerp_b", currentPotion.lerpColours[1]);
            mat.SetShaderParam("colour_lerp_c", currentPotion.lerpColours[2]);

            currentPotion = null;

            UpdatePotionSlot();
        }

        private void ThrowEmptyPotion()
        {
            BottleSmashEffect emptyPotion = bottleSmashEffectScene.Instance<BottleSmashEffect>();
            World.AddChild(emptyPotion);
            emptyPotion.Position = Position + new Vector2(0, -11);
            emptyPotion.Start(new Vector2(World.rng.RandfRange(2.0f, 3.5f) * (World.rng.Randf() <= 0.5f ? -1.0f : 1.0f), World.rng.RandfRange(-1.0f, 1.0f)), World.rng.RandfRange(350.0f, 500.0f));
        }

        public void PlayGulpSound()
        {
            potionSoundPlayer.Stream = gulpSound;
            potionSoundPlayer.Play(0);
        }

        private void PlayBurpSound()
        {
            potionSoundPlayer.Stream = burpSounds[World.rng.RandiRange(0, burpSounds.Count - 1)];
            potionSoundPlayer.Play(0);
        }

        private void UpdatePotionSlot()
        {
            if (currentPotion == null)
            {
                potionSlot.SelfModulate = Colors.Transparent;
                potionSlot.ItemName = "";

                return;
            }

            potionSlot.SelfModulate = Colors.White;
            potionSlot.ItemName = currentPotion.name;

            ShaderMaterial shaderMat = (ShaderMaterial)potionSlot.Material;

            shaderMat.SetShaderParam("colour_lerp_a", currentPotion.lerpColours[0]);
            shaderMat.SetShaderParam("colour_lerp_b", currentPotion.lerpColours[1]);
            shaderMat.SetShaderParam("colour_lerp_c", currentPotion.lerpColours[2]);
        }

        public void PickedUpArtefact(Artefact artefact)
        {
            World.artefactNamePopup.DisplayPopup(artefact.Name, artefact.Description);
        }

        public void MixInSpellColour(Color newColour, float weight)
        {
            SpellColourMods.Add((newColour, weight));

            CachePrimarySpellColour();
        }

        private Color BlendColors(Color a, Color b, float t)
        {
            return new Color(BlendColourChannel(a.r, b.r, t), BlendColourChannel(a.g, b.g, t), BlendColourChannel(a.b, b.b, t));
        }

        private float BlendColourChannel(float a, float b, float t)
        {
            return Mathf.Sqrt((1.0f - t) * Mathf.Pow(a, 2.2f) + t * Mathf.Pow(b, 2.2f));
        }

        private void CachePrimarySpellColour()
        {
            Color baseColour = primarySpell.BaseColour;

            foreach (var modifier in SpellColourMods)
            {
                baseColour = BlendColors(baseColour, modifier.colour, modifier.weight);
            }

            primarySpellColourCache = baseColour;
            UpdateStaffGlow();
        }

        private void UpdateStaffGlow()
        {
            (staff.Material as ShaderMaterial).SetShaderParam("emission_tint", primarySpellColourCache);
        }

        public void PickUpPrimarySpell(BaseSpell spell)
        {
            if (primarySpell != null)
                DropSpellPickup(primarySpell);

            World.artefactNamePopup?.DisplayPopup("Tome of " + spell.Name, "");

            primarySpell = spell;

            primarySpellSlot.SetItemTexture(spell.Icon);
            primarySpellSlot.ItemName = spell.Name;

            CachePrimarySpellColour();
        }

        private void DropSpellPickup(BaseSpell spell)
        {
            SpellPickup droppedSpell = spellPickupScene.Instance<SpellPickup>();
            droppedSpell.SetSpell(spell);
            World.AddChild(droppedSpell);
            droppedSpell.Position = Position + new Vector2(Dir * -8.0f);
            droppedSpell.ApplyCentralImpulse(Dir * -80.0f);
        }

        public BuffTracker ApplyPerRoomBuff(string source, HashSet<(Stat stat, float amount)> stats, int duration)
        {
            foreach (BuffTracker buffTracker in PerRoomBuffs)
            {
                if (buffTracker.sourceName == source)
                {
                    buffTracker.QueueFree();
                }
            }

            PerRoomBuffs.RemoveWhere(b => { return b.sourceName == source; });

            BuffTracker newBuffTracker = buffTrackerScene.Instance<BuffTracker>();
            newBuffTracker.Init(source, stats, duration);

            buffTrackerContainer.AddChild(newBuffTracker);

            PerRoomBuffs.Add(newBuffTracker);

            buffTrackerContainer.Columns = Mathf.Min(PerRoomBuffs.Count, 5);

            RecalcStats();

            return newBuffTracker;
        }

        public override void RecalcStats()
        {
            base.RecalcStats();

            foreach (BuffTracker buffTracker in PerRoomBuffs)
            {
                foreach ((Stat stat, float amount) in buffTracker.stats)
                {
                    currentStats[stat] = currentStats[stat] + amount;
                }
            }
        }

        public void ClearedRoom()
        {
            // Decrement per room buffs and clear ones with 0 charges left

            foreach (BuffTracker buffTracker in PerRoomBuffs)
            {
                buffTracker.Charges--;

                if (buffTracker.Charges <= 0)
                {
                    buffTracker.QueueFree();
                }
            }

            int removed = PerRoomBuffs.RemoveWhere(b => b.Charges <= 0);

            if (removed > 0)
            {
                RecalcStats();
            }
        }

        private void UpdateGSTest()
        {
            ParticlesMaterial pMat = (ParticlesMaterial)gsTest.ProcessMaterial;

            Vector3 pDir = new Vector3(facingDir.x, facingDir.y, 0f);

            pMat.InitialVelocity = 40.57f;
            pMat.Spread = 17.21f;

            if (DirectionExt.FromVector(facingDir) == DirectionExt.FromVector(MovementVelocity))
            {
                pDir.x += MovementVelocity.x;
                pDir.y += MovementVelocity.y;

                pMat.InitialVelocity += (MovementVelocity.Length() * 2f);

                pMat.Spread *= 0.5f;
            }

            pMat.Direction = pDir;
            pMat.Gravity = new Vector3(0f, 49f * Mathf.Abs(facingDir.x), 0f);

            gsTest.GlobalRotation = 0.0f;
        }
    }
}