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
            mgr.targetPosCache = mgr.lastTarget.GlobalPosition;
        }

        public override void OnBehaviourEnd() { }

        public override Vector2 Steer()
        {
            return GetDirectionTowardsTarget();
        }

        private Vector2 GetDirectionTowardsTarget()
        {
            return (mgr.lastTarget.GlobalPosition - mgr.owner.GlobalPosition).Normalized();
        }

        private bool CanSeePlayer()
        {
            return AIManager.TraceToTarget(mgr.owner.GlobalPosition, mgr.lastTarget, mgr.owner.GetWorld2d().DirectSpaceState, AIManager.visibilityLayer, new Godot.Collections.Array() { mgr.owner });
        }

        private bool IsDirectionObstructed()
        {
            return AIManager.TraceToTarget(mgr.owner.GlobalPosition, mgr.lastTarget, mgr.owner.GetWorld2d().DirectSpaceState, AIManager.tilesLayer, new Godot.Collections.Array() { mgr.owner });
        }
    }
}