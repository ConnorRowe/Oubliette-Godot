using Godot;
using System;

public class MeleeAttackBehaviour : AIBehaviour
{
    private float attackSpeed;
    private float attackRange;
    private SceneTreeTimer timer;

    public MeleeAttackBehaviour(AIManager manager, float attackSpeed, float attackRange, Func<TransitionTestResult>[] transitions) : base(manager, transitions)
    {
        this.attackSpeed = attackSpeed;
        this.attackRange = attackRange;
    }

    public override void OnBehaviourStart()
    {
        TryAttack();
    }

    public override void Process(float delta) { }
    public override void OnBehaviourEnd()
    {
        if (timer != null && timer.IsConnected("timeout", this, nameof(TryAttack)))
            timer.Disconnect("timeout", this, nameof(TryAttack));
    }

    private void TryAttack()
    {
        if (!IsInstanceValid(mgr.owner))
            return;

        if (mgr.owner.GlobalPosition.DistanceTo(mgr.lastTarget.GlobalPosition) < attackRange && !mgr.owner.isDead)
        {
            mgr.lastTarget.TakeDamage(source: mgr.owner, sourceName: mgr.owner.damageSourceName);
            mgr.lastTarget.ApplyKnockBack(mgr.owner.dir * 80.0f);
        }

        if (!mgr.owner.isDead)
        {
            timer = mgr.owner.GetTree().CreateTimer(attackSpeed);
            timer.Connect("timeout", this, nameof(TryAttack));
        }
    }

    public override Vector2 Steer()
    {
        return Vector2.Zero;
    }
}
