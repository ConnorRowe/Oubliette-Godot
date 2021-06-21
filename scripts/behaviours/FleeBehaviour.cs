using Godot;
using System;

namespace Oubliette.AI
{
    // Run away from target
    public class FleeBehaviour : AIBehaviour
    {
        private const float quartPi = Mathf.Pi / 4f;

        private SceneTreeTimer timer;
        private float steerRotate = 0f;

        public FleeBehaviour(AIManager manager, Func<TransitionTestResult>[] transitions) : base(manager, transitions) { }
        public override void OnBehaviourStart()
        {
            ChangeDirection();
        }

        public override void Process(float delta) { }
        public override void OnBehaviourEnd()
        {
            if (timer != null && timer.IsConnected("timeout", this, nameof(ChangeDirection)))
                timer.Disconnect("timeout", this, nameof(ChangeDirection));
        }

        public override Vector2 Steer()
        {
            return mgr.LastTarget.GlobalPosition.DirectionTo(mgr.Owner.GlobalPosition).Rotated(steerRotate);
        }

        private void ChangeDirection()
        {
            steerRotate = (mgr.rng.Randf() <= 0.5f) ? quartPi : -quartPi;

            timer = mgr.Owner.GetTree().CreateTimer(0.5f);
            timer.Connect("timeout", this, nameof(ChangeDirection));
        }

    }
}