using Godot;
using System;

public class AICharacterWithWeaponRanged : AICharacterWithWeapon
{
    private bool isCharging = false;
    private Particles2D weaponParticles;
    private BaseSpell currentSpell;

    [Export]
    private NodePath _weaponParticlesPath;
    [Export]
    private float projHueAdjust = 0.0f;

    public override void _Ready()
    {
        base._Ready();

        currentSpell = Spells.Shadowbolt;

        weaponParticles = GetNode<Particles2D>(_weaponParticlesPath);

        ProjectileBehaviour projBehaviour = new ProjectileBehaviour(aIManager, currentSpell as ProjectileSpell, new Func<AIBehaviour.TransitionTestResult>[] {
            
            // attack_target -> follow_target
            () => {
                return new AIBehaviour.TransitionTestResult(GlobalPosition.DistanceTo(aIManager.lastTarget.GlobalPosition) > attackRange, "follow_target");
            },
            // attack_target -> path_to_last_pos
            () => {
                // If target is not visible
                Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
                return new AIBehaviour.TransitionTestResult(!AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.visibilityLayer, new Godot.Collections.Array() {this}), "path_to_last_pos");
            },

            // // attack -> path_to_last_pos
            // () => {
            //     // If player is not visible or projectile has no LoS
            //     Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
            //     return new AIBehaviour.TransitionTestResult(!isCharging && (!AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.visibilityLayer, new Godot.Collections.Array() {this}) || !AIManager.TraceToTarget(weaponParticles.GlobalPosition, aIManager.lastTarget, spaceState, AIManager.visibilityLayer, new Godot.Collections.Array() {this})), "path_to_last_pos");
            // },

            // // attack -> wander
            // () => {
            //     // If player is too far away or player is not visible
            //     Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
            //     return new AIBehaviour.TransitionTestResult(!isCharging && GlobalPosition.DistanceTo(aIManager.lastTarget.GlobalPosition) > attackRange || !AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.visibilityLayer, new Godot.Collections.Array() {this}), "wander");
            // }
        });

        aIManager.RemoveBehaviour("attack_target");
        aIManager.AddBehaviour("attack_target", projBehaviour);

        aIManager.Connect(nameof(AIManager.BehaviourChanged), this, nameof(BehaviourChanged));
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (aIManager.CurrentBehaviour == "attack_target")
        {
            dir = GlobalPosition.DirectionTo(aIManager.lastTarget.GlobalPosition);
        }
    }

    private void BehaviourChanged(string behaviour)
    {
        if (!isCharging && behaviour == "attack_target")
        {
            ChargeSpell();
        }

        isCharging = false;
    }

    private void ChargeSpell()
    {
        tween.Remove(this, nameof(SetShownParticles));
        tween.Remove(this, nameof(Fire));

        tween.InterpolateMethod(this, nameof(SetShownParticles), 0, 32, this.attackSpeed, Tween.TransitionType.Quad);
        tween.InterpolateCallback(this, this.attackSpeed, nameof(Fire));
        tween.Start();

        isCharging = true;
    }

    private void Fire()
    {
        isCharging = false;

        aIManager.EmitSignal(nameof(AIManager.Fire));

        SetShownParticles(0);

        aIManager.TryTransition();

        if (aIManager.CurrentBehaviour == "attack_target")
        {
            ChargeSpell();
        }
    }

    private void SetShownParticles(int num)
    {
        (weaponParticles.ProcessMaterial as ShaderMaterial)?.SetShaderParam("number_particles_shown", num);
    }

    public override Vector2 GetSpellSpawnPos()
    {
        return weaponParticles.GlobalPosition;
    }

    public override float GetSpellRange(float baseRange)
    {
        return 999.0f;
    }

    public override void Die()
    {
        // stop spell from charging and firing
        tween.StopAll();
        weaponParticles.QueueFree();

        tween.InterpolateMethod(this, nameof(SetWeaponGlow), 20.0f, 1.0f, 4.0f, Tween.TransitionType.Cubic, Tween.EaseType.In);

        base.Die();
    }

    private void SetWeaponGlow(float intensity)
    {
        (weapon.Material as ShaderMaterial).SetShaderParam("intensity", intensity);
    }
}