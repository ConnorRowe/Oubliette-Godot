using Godot;
using System;

public class AICharacterWithWeapon : AICharacter
{
    private float weaponRot = 0.0f;
    private Godot.Collections.Dictionary<Direction, Vector2> weaponOrigins = new Godot.Collections.Dictionary<Direction, Vector2>();
    private Godot.Collections.Dictionary<Direction, Vector2> armOrigins = new Godot.Collections.Dictionary<Direction, Vector2>();
    private Vector2 wepFallPos = Vector2.Zero;

    // Nodes
    protected Sprite weapon;
    private Line2D arm;

    // Export
    [Export]
    private NodePath _weaponPath;
    [Export]
    private NodePath _armPath;
    [Export]
    private float weaponRotOffset = 0.0f;
    [Export]
    private float weaponRotDistance = 4.0f;
    [Export]
    private Godot.Collections.Dictionary<String, Vector2> weaponOriginsString = new Godot.Collections.Dictionary<String, Vector2>() { { "up", new Vector2() }, { "right", new Vector2() }, { "down", new Vector2() }, { "left", new Vector2() } };
    [Export]
    private Godot.Collections.Dictionary<String, Vector2> armOriginsString = new Godot.Collections.Dictionary<String, Vector2>() { { "up", new Vector2() }, { "right", new Vector2() }, { "down", new Vector2() }, { "left", new Vector2() } };

    public override void _Ready()
    {
        base._Ready();

        weapon = GetNode<Sprite>(_weaponPath);
        arm = GetNode<Line2D>(_armPath);

        foreach (string key in weaponOriginsString.Keys)
        {
            weaponOrigins.Add(DirectionExt.FromString(key), weaponOriginsString[key]);
        }
        foreach (string key in armOriginsString.Keys)
        {
            armOrigins.Add(DirectionExt.FromString(key), armOriginsString[key]);
        }
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (!isDead)
        {
            Direction fDir = GetFacingDirection();
            weaponRot = Mathf.Atan2(dir.y, dir.x) + (weaponRotOffset * (fDir == Direction.Up || fDir == Direction.Left ? -1.0f : 1.0f));

            weapon.Rotation = weaponRot;
            weapon.Position = weaponOrigins[fDir] + (dir * weaponRotDistance);
            weapon.ShowBehindParent = fDir == Direction.Up;

            weapon.FlipV = (fDir == Direction.Up || fDir == Direction.Left);

            arm.Points = new Vector2[] { armOrigins[fDir] + GetFrameWepOffset(), weapon.Position };
        }
    }

    public virtual Vector2 GetFrameWepOffset()
    {
        if (charSprite.Frame % 3 == 0)
            return new Vector2(0, -1);

        return Vector2.Zero;
    }

    public override void Die()
    {
        base.Die();

        float dropDist = 16.0f;
        wepFallPos = weapon.Position + new Vector2(aIManager.rng.RandfRange(-dropDist, dropDist), aIManager.rng.RandfRange(-dropDist, dropDist));

        float duration = 0.35f;

        detectionNotifier.Visible = false;

        tween.InterpolateProperty(weapon, "position", weapon.Position, weapon.Position.LinearInterpolate(wepFallPos, 0.5f) + new Vector2(0.0f, -16.0f), duration);
        tween.InterpolateCallback(this, duration, nameof(DropWeapon));
        tween.InterpolateProperty(weapon, "rotation", weapon.Rotation, 0.0f, duration, Tween.TransitionType.Bounce);
        tween.Start();
    }

    private void DropWeapon()
    {
        tween.InterpolateProperty(weapon, "position", weapon.Position, wepFallPos, 0.25f);
        tween.Start();
    }
}
