using Godot;
using System;

namespace Oubliette.AI
{
    public class ChargeBehaviour : MovementBehaviour
    {
        Func<Direction> getDirection;
        bool shouldStop = false;

        public ChargeBehaviour(AIManager manager, Func<Direction> getDirection, Func<TransitionTestResult>[] transitions) : base(manager, transitions)
        {
            this.getDirection = getDirection;
        }

        public override void OnBehaviourStart()
        {
            (mgr.owner as Snail).ResetChargeCooldown();
            (mgr.owner as Snail).IsCharging = true;
            shouldStop = false;

            if (!mgr.owner.IsConnected(nameof(Character.SlideCollision), this, nameof(SlideCollision)))
            {
                mgr.owner.Connect(nameof(Character.SlideCollision), this, nameof(SlideCollision));
            }

            if (!mgr.owner.IsConnected(nameof(AICharacter.PlayerIntersected), this, nameof(HitPlayer)))
            {
                mgr.owner.Connect(nameof(AICharacter.PlayerIntersected), this, nameof(HitPlayer));
            }
        }

        public override void Process(float delta)
        {
            mgr.targetPosCache = mgr.lastTarget.GlobalPosition;
        }

        public override void OnBehaviourEnd()
        {
            (mgr.owner as Snail).IsCharging = false;

            if (mgr.owner.IsConnected(nameof(Character.SlideCollision), this, nameof(SlideCollision)))
            {
                mgr.owner.Disconnect(nameof(Character.SlideCollision), this, nameof(SlideCollision));
            }

            if (!mgr.owner.IsConnected(nameof(AICharacter.PlayerIntersected), this, nameof(HitPlayer)))
            {
                mgr.owner.Disconnect(nameof(AICharacter.PlayerIntersected), this, nameof(HitPlayer));
            }
        }

        public override Vector2 Steer()
        {
            if (shouldStop)
                return Vector2.Zero;

            return getDirection().AsVector();
        }

        private void SlideCollision(KinematicCollision2D collision)
        {
            StopCharge(collision.Collider as Node);
        }

        private void HitPlayer(Player player)
        {
            StopCharge(player);
        }

        private void StopCharge(Node node)
        {
            if (!shouldStop)
            {
                if (node is Player player)
                {
                    player.TakeDamage(1, mgr.owner);
                }

                if (node is Character character)
                {
                    character.ApplyKnockBack(getDirection().AsVector() * 200.0f);
                }

                mgr.owner.ApplyKnockBack(getDirection().Opposite().AsVector() * 100.0f);

                shouldStop = true;
                (mgr.owner as Snail).IsCharging = false;
            }
        }
    }
}