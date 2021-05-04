using Godot;
using System;

public class ProjectileBehaviour : AIBehaviour
{
    ProjectileSpell projectileSpell;

    public ProjectileBehaviour(AIManager manager, ProjectileSpell projectileSpell, Func<TransitionTestResult>[] transitions) : base(manager, transitions)
    {
        this.projectileSpell = projectileSpell;

        mgr.Connect(nameof(AIManager.Fire), this, nameof(FireProjectile));
    }

    public override void OnBehaviourStart()
    {
    }

    public override void Process(float delta)
    {
        mgr.targetPosCache = mgr.lastTarget.GlobalPosition;
    }

    public override Vector2 Steer()
    {
        return Vector2.Zero;
    }

    public override void OnBehaviourEnd()
    {
    }

    private void FireProjectile()
    {
        projectileSpell.Cast(mgr.owner as ICastsSpells);
    }
}
