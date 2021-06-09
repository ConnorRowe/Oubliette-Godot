using Godot;
using System;

namespace Oubliette.AI
{
    public class FollowTargetBehaviour : MovementBehaviour
    {
        public FollowTargetBehaviour(AIManager manager, Func<TransitionTestResult>[] transitions) : base(manager, transitions) { }

        public override void OnBehaviourStart() { }

        public override void Process(float delta)
        {
            mgr.TargetPosCache = mgr.LastTarget.GlobalPosition;
        }

        public override void OnBehaviourEnd() { }

        public override Vector2 Steer()
        {
            return GetDirectionTowardsTarget();
        }

        private Vector2 GetDirectionTowardsTarget()
        {
            return (mgr.LastTarget.GlobalPosition - mgr.Owner.GlobalPosition).Normalized();
        }

        private bool CanSeePlayer()
        {
            return AIManager.TraceToTarget(mgr.Owner.GlobalPosition, mgr.LastTarget, mgr.Owner.GetWorld2d().DirectSpaceState, AIManager.VisibilityLayer, new Godot.Collections.Array() { mgr.Owner });
        }

        private bool IsDirectionObstructed()
        {
            return AIManager.TraceToTarget(mgr.Owner.GlobalPosition, mgr.LastTarget, mgr.Owner.GetWorld2d().DirectSpaceState, AIManager.TilesLayer, new Godot.Collections.Array() { mgr.Owner });
        }
    }
}