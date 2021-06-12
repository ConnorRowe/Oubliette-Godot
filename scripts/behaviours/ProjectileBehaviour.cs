using Godot;
using System;

namespace Oubliette.AI
{
    public class ProjectileBehaviour : AIBehaviour
    {
        Spells.ProjectileSpell projectileSpell;

        public ProjectileBehaviour(AIManager manager, Spells.ProjectileSpell projectileSpell, Func<TransitionTestResult>[] transitions) : base(manager, transitions)
        {
            this.projectileSpell = projectileSpell;

            mgr.Connect(nameof(AIManager.Fire), this, nameof(FireProjectile));
        }

        public override void OnBehaviourStart()
        {
        }

        public override void Process(float delta)
        {
            mgr.TargetPosCache = mgr.LastTarget.GlobalPosition;
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
            projectileSpell.Cast(mgr.Owner as Spells.ICastsSpells);
        }
    }
}