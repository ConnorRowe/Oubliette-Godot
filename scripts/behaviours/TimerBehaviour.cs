using Godot;
using System;

namespace Oubliette.AI
{
    public class TimerBehaviour : AIBehaviour
    {
        private float cooldownTime;

        public TimerBehaviour(AIManager manager, float cooldownTime, Func<TransitionTestResult>[] transitions) : base(manager, transitions)
        {
            this.cooldownTime = cooldownTime;
        }

        public override void OnBehaviourStart()
        {
            mgr.CanTryTransition = false;
            mgr.Owner.GetTree().CreateTimer(cooldownTime).Connect("timeout", this, nameof(Timeout));
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

        private void Timeout()
        {
            mgr.CanTryTransition = true;
        }
    }
}