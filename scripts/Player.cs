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
    private float primarySpellCooldown = 0.0f;
    public float maxPrimarySpellCooldown = 0.5f;
    private bool isSpellActive = false;
    private float maxMajyka = 100.0f;
    private float currentMajyka = 100.0f;
    public float staffRot = 0.0f;
    private Vector2 armSocket = Vector2.Zero;
    private Vector2 facingDir = Vector2.Zero;
    private Godot.Collections.Dictionary<Direction, Vector2> staffOrigins = new Godot.Collections.Dictionary<Direction, Vector2>() { { Direction.Up, new Vector2(4.0f, -10.0f) }, { Direction.Right, new Vector2(0.0f, -8.0f) }, { Direction.Down, new Vector2(-4.0f, -10.0f) }, { Direction.Left, new Vector2(0.0f, -10.0f) } };
    private Godot.Collections.Dictionary<Direction, Vector2> armOrigins = new Godot.Collections.Dictionary<Direction, Vector2>() { { Direction.Up, new Vector2(2.5f, -11.0f) }, { Direction.Right, new Vector2(-0.5f, -11.0f) }, { Direction.Down, new Vector2(-2.5f, -11.0f) }, { Direction.Left, new Vector2(0.5f, -11.0f) } };

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

    // Input
    private bool inputMoveUp = false;
    private bool inputMoveDown = false;
    private bool inputMoveLeft = false;
    private bool inputMoveRight = false;

    // Assets
    private Texture debugPoint;
    private PackedScene projectile;

    // Export
    [Export]
    private NodePath _cameraPath;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();

        debugPoint = GD.Load<Texture>("res://textures/2x2_white.png");
        projectile = GD.Load<PackedScene>("res://scenes/Projectile.tscn");

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

        primarySpell = _Spells.MagicMissile;
        secondarySpell = _Spells.IceSkin;

        DebugOverlay debug = world.GetDebugOverlay();
        debug.TrackFunc(nameof(GetStatValue), this, "Dmg Res", Stat.ResistDamageFlat);
        debug.TrackFunc(nameof(GetStatValue), this, "Dmg Refl", Stat.ReflectDamageFlat);
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
            if (currentMajyka >= primarySpell.majykaCost && primarySpellCooldown == 0.0f)
            {
                // Cast primary spell
                primarySpell.Cast(this);

                currentMajyka -= primarySpell.majykaCost;
                UpdateMajykaBar();

                primarySpellCooldown = GetMaxPrimarySpellCooldown();

            }
        }

        float primarySpellCDPercent = primarySpellCooldown / GetMaxPrimarySpellCooldown();
        if (primarySpellCDPercent < 1.0f)
            majykaBar.UpdateSpellCooldown(primarySpellCDPercent);
        else
            majykaBar.UpdateSpellCooldown(0.0f);
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);


        var spaceState = GetWorld2d().DirectSpaceState;
        // var result = spaceState.IntersectPoint(lightArea.GlobalPosition, collideWithAreas:true, collisionLayer: 0b0001);
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
        currentMajyka += (25.0f * delta);

        if (currentMajyka > maxMajyka)
            currentMajyka = maxMajyka;

        UpdateMajykaBar();
    }

    private void UpdateMajykaBar()
    {
        majykaBar.CurrentMajyka = Mathf.RoundToInt((currentMajyka / maxMajyka) * 100.0f);
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
}