using Godot;
using System;

public class ProjectileBehaviour : AIBehaviour
{
    PackedScene projectile;
    private float projSpeed;
    Func<Vector2> getProjStartPos;
    Action<Projectile> modifyProj;

    public ProjectileBehaviour(AIManager manager, PackedScene projectile, float projSpeed, Func<Vector2> getProjStartPos, Action<Projectile> modifyProj, Func<TransitionTestResult>[] transitions) : base(manager, transitions)
    {
        this.projectile = projectile;
        this.projSpeed = projSpeed;
        this.getProjStartPos = getProjStartPos;
        this.modifyProj = modifyProj;

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
        Projectile proj = projectile.Instance<Projectile>();
        mgr.world.AddChild(proj);
        proj.GlobalPosition = getProjStartPos();
        proj.direction = mgr.owner.dir;
        proj.source = mgr.owner;
        proj.SetSpellStats(1, 999, 100.0f, projSpeed);
        modifyProj(proj);
    }
}
