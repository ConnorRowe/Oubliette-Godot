using Godot;
using System.Collections.Generic;
using Stats;

public class Player : Character, ICastsSpells
{
    private float jumpPower = 175f;
    private float scrollScale = 1.0f;
    public BaseSpell primarySpell;
    public BaseSpell secondarySpell;
    private SceneTreeTimer spellEffectTimer;
    private SceneTreeTimer takeDmgTimer;
    private SceneTreeTimer spillageDmgTimer;
    private int spillageCount = 0;
    private bool canTakeDamage = true;
    private float primarySpellCooldown = 0.0f;
    public float maxPrimarySpellCooldown = 0.5f;
    private bool isSpellActive = false;
    private float maxMajyka = 100.0f;
    private float currentMajyka = 100.0f;
    public float staffRot = 0.0f;
    public List<Artefact.ArtefactTextureSet> artefactTextureSets = new List<Artefact.ArtefactTextureSet>();
    private Vector2 armSocket = Vector2.Zero;
    private Vector2 facingDir = Vector2.Zero;
    private Godot.Collections.Dictionary<Direction, Vector2> staffOrigins = new Godot.Collections.Dictionary<Direction, Vector2>() { { Direction.Up, new Vector2(4.0f, -10.0f) }, { Direction.Right, new Vector2(0.0f, -8.0f) }, { Direction.Down, new Vector2(-4.0f, -10.0f) }, { Direction.Left, new Vector2(0.0f, -10.0f) } };
    private Godot.Collections.Dictionary<Direction, Vector2> armOrigins = new Godot.Collections.Dictionary<Direction, Vector2>() { { Direction.Up, new Vector2(2.5f, -11.0f) }, { Direction.Right, new Vector2(-0.5f, -11.0f) }, { Direction.Down, new Vector2(-2.5f, -11.0f) }, { Direction.Left, new Vector2(0.5f, -11.0f) } };
    private Physics2DShapeQueryParameters hitAreaShapeQuery;
    private HashSet<Node> intersectedAreas = new HashSet<Node>();
    private Potion currentPotion;
    private float pickupCooldown = 0.0f;

    // Nodes
    public Camera2D camera;
    private Spells _Spells;
    private Particles2D spellParticle;
    private MajykaContainer majykaBar;
    private Sprite staff;
    private World world;
    private Line2D arm;
    private Line2D armOutline;
    private Light2D staffLight;
    private Node2D spellSpawnPoint;
    private ItemDisplaySlot potionSlot;

    // Input
    private bool inputMoveUp = false;
    private bool inputMoveDown = false;
    private bool inputMoveLeft = false;
    private bool inputMoveRight = false;

    // Assets
    private Texture debugPoint;
    private PackedScene projectileScene;
    private PackedScene potionScene;

    // Export
    [Export]
    private NodePath _cameraPath;
    [Export]
    private Shape2D hitBoxTraceShape;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();

        Area2D feetArea = GetNode<Area2D>("FeetArea");
        feetArea.Connect("area_entered", this, nameof(OnAreaEntered));
        feetArea.Connect("area_exited", this, nameof(OnAreaExited));

        debugPoint = GD.Load<Texture>("res://textures/2x2_white.png");
        projectileScene = GD.Load<PackedScene>("res://scenes/Projectile.tscn");
        potionScene = GD.Load<PackedScene>("res://scenes/Potion.tscn");

        _Spells = (Spells)GetNode("/root/Spells");
        majykaBar = GetParent().GetNode<MajykaContainer>("CanvasLayer/MajykaContainer");
        spellParticle = GetNode<Particles2D>("CharSprite/SpellParticle");
        spellParticle.Material = new ShaderMaterial();
        staff = GetNode<Sprite>("CharSprite/staff");
        camera = GetNode<Camera2D>(_cameraPath);
        world = GetParent<World>();
        arm = GetNode<Line2D>("CharSprite/Arm");
        armOutline = GetNode<Line2D>("CharSprite/ArmOutline");
        staffLight = GetNode<Light2D>("CharSprite/staff/StaffLight");
        spellSpawnPoint = GetNode<Node2D>("CharSprite/staff/SpellSpawnPoint");
        potionSlot = world.GetNode<ItemDisplaySlot>("CanvasLayer/PotionSlot");

        primarySpell = _Spells.MagicMissile;
        secondarySpell = _Spells.IceSkin;

        DebugOverlay debug = world.GetDebugOverlay();
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

        checkSlideCollisions = true;

        UpdatePotionSlot();
    }

    public override void _Input(InputEvent evt)
    {
        base._Input(evt);

        if (evt.IsActionPressed("g_cast_secondary_spell"))
        {
            CastSecondarySpell();
        }

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
            Vector2 gMPos = camera.GetGlobalMousePosition();
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

        if (Input.IsActionPressed("g_cast_primary_spell"))
        {
            if (currentMajyka >= GetSpellCost(primarySpell.majykaCost) && primarySpellCooldown == 0.0f)
            {
                // Cast primary spell
                primarySpell.Cast(this);

                currentMajyka -= GetSpellCost(primarySpell.majykaCost);
                UpdateMajykaBar();

                primarySpellCooldown = GetMaxPrimarySpellCooldown();

            }
        }

        float primarySpellCDPercent = primarySpellCooldown / GetMaxPrimarySpellCooldown();
        if (primarySpellCDPercent < 1.0f)
            majykaBar.UpdateSpellCooldown(primarySpellCDPercent);
        else
            majykaBar.UpdateSpellCooldown(0.0f);

        Update();
    }

    public override void _Draw()
    {
        DrawEquippedArtefacts(GetFacingDirection());
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

        switch (GetDirection(dir))
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
        if (currentMajyka >= GetSpellCost(secondarySpell.majykaCost))
        {
            secondarySpell.Cast(this);

            currentMajyka -= GetSpellCost(secondarySpell.majykaCost);
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

    public override void TakeDamage(int damage = 1, Character source = null)
    {
        if (canTakeDamage)
        {
            base.TakeDamage(damage, source);
            canTakeDamage = false;
            takeDmgTimer = GetTree().CreateTimer(0.25f, false);
            takeDmgTimer.Connect("timeout", this, nameof(ResetCanTakeDamage));
        }
    }

    public override Direction GetFacingDirection()
    {
        return GetDirection(facingDir);
    }

    public override float GetMaxSpeed()
    {
        return base.GetMaxSpeed() * currentStats[Stat.MoveSpeedMultiplier];
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

    public void OnAreaEntered(Area2D area)
    {
        if (area is SpillageHazard)
        {
            spillageCount++;

            if (spillageCount == 1)
            {
                if (spillageDmgTimer != null && spillageDmgTimer.TimeLeft > 0.0f)
                    return;

                SpillageDamage();
            }
        }
    }

    public void OnAreaExited(Area2D area)
    {
        if (area is SpillageHazard)
        {
            spillageCount--;
        }
    }

    private void SpillageDamage()
    {
        if (spillageCount > 0)
        {
            TakeDamage();

            spillageDmgTimer = GetTree().CreateTimer(1.0f, false);
            spillageDmgTimer.Connect("timeout", this, nameof(SpillageDamage));
        }
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
            world.AddChild(droppedPotion);
            droppedPotion.Position = Position;
            droppedPotion.ApplyCentralImpulse(dir * 60f);
        }

        currentPotion = newPotion;

        UpdatePotionSlot();

        pickupCooldown = 0.5f;

        return true;
    }

    private void ConsumeCurrentPotion()
    {
        if (currentPotion == null)
            return;

        ApplyBuff(Stats.Buffs.CreateBuff(currentPotion.name, new List<(Stat stat, float amount)>(currentPotion.buffs), currentPotion.duration));

        currentPotion = null;

        UpdatePotionSlot();
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
        world.artefactNamePopup.DisplayPopup(artefact.Name, artefact.Description);
    }
}