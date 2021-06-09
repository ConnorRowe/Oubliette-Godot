using Godot;
using System;

namespace Oubliette.AI
{
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
            if (!IsInstanceValid(mgr.Owner))
                return;

            if (mgr.Owner.GlobalPosition.DistanceTo(mgr.LastTarget.GlobalPosition) < attackRange && !mgr.Owner.IsDead)
            {
                mgr.LastTarget.TakeDamage(source: mgr.Owner, sourceName: mgr.Owner.DamageSourceName);
                mgr.LastTarget.ApplyKnockBack(mgr.Owner.Dir * 80.0f);
            }

            if (!mgr.Owner.IsDead)
            {
                timer = mgr.Owner.GetTree().CreateTimer(attackSpeed);
                timer.Connect("timeout", this, nameof(TryAttack));
            }
        }

        public override Vector2 Steer()
        {
            return Vector2.Zero;
        }
    }
}