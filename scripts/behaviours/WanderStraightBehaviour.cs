using Godot;
using System;

namespace Oubliette.AI
{
    public class WanderStraightBehaviour : MovementBehaviour
    {
        Direction currentDirection = Direction.Left;

        public WanderStraightBehaviour(AIManager manager, Func<TransitionTestResult>[] transitions) : base(manager, transitions) { }

        public override void OnBehaviourStart()
        {
            currentDirection = DirectionExt.FromInt(mgr.rng.RandiRange(0, 3));

            if (!mgr.Owner.IsConnected(nameof(Character.SlideCollision), this, nameof(SlideCollision)))
            {
                mgr.Owner.Connect(nameof(Character.SlideCollision), this, nameof(SlideCollision));
            }
        }

        public override void Process(float delta)
        {
            // mgr.targetPosCache = mgr.lastTarget.GlobalPosition;
        }

        public override void OnBehaviourEnd()
        {
            if (mgr.Owner.IsConnected(nameof(Character.SlideCollision), this, nameof(SlideCollision)))
            {
                mgr.Owner.Disconnect(nameof(Character.SlideCollision), this, nameof(SlideCollision));
            }
        }

        public override Vector2 Steer()
        {
            return currentDirection.AsVector();
        }

        private void SlideCollision(KinematicCollision2D collision)
        {
            // cycle direction on collision
            currentDirection = DirectionExt.Directions()[Mathf.PosMod((int)currentDirection + 1, 4)];
        }
    }
}