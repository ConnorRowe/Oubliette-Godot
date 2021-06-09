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
            (mgr.Owner as Snail).ResetChargeCooldown();
            (mgr.Owner as Snail).IsCharging = true;
            shouldStop = false;

            if (!mgr.Owner.IsConnected(nameof(Character.SlideCollision), this, nameof(SlideCollision)))
            {
                mgr.Owner.Connect(nameof(Character.SlideCollision), this, nameof(SlideCollision));
            }

            if (!mgr.Owner.IsConnected(nameof(AICharacter.PlayerIntersected), this, nameof(HitPlayer)))
            {
                mgr.Owner.Connect(nameof(AICharacter.PlayerIntersected), this, nameof(HitPlayer));
            }
        }

        public override void Process(float delta)
        {
            mgr.TargetPosCache = mgr.LastTarget.GlobalPosition;
        }

        public override void OnBehaviourEnd()
        {
            (mgr.Owner as Snail).IsCharging = false;

            if (mgr.Owner.IsConnected(nameof(Character.SlideCollision), this, nameof(SlideCollision)))
            {
                mgr.Owner.Disconnect(nameof(Character.SlideCollision), this, nameof(SlideCollision));
            }

            if (!mgr.Owner.IsConnected(nameof(AICharacter.PlayerIntersected), this, nameof(HitPlayer)))
            {
                mgr.Owner.Disconnect(nameof(AICharacter.PlayerIntersected), this, nameof(HitPlayer));
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
                    player.TakeDamage(1, mgr.Owner);
                }

                if (node is Character character)
                {
                    character.ApplyKnockBack(getDirection().AsVector() * 200.0f);
                }

                mgr.Owner.ApplyKnockBack(getDirection().Opposite().AsVector() * 100.0f);

                shouldStop = true;
                (mgr.Owner as Snail).IsCharging = false;
            }
        }
    }
}