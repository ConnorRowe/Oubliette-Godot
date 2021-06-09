using Godot;
using System;
using Oubliette.AI;

namespace Oubliette
{
    public class Snail : AICharacter
    {
        private Direction chargeDirection = Direction.Up;
        private float chargeSpeed = 72.0f;
        private float chargeCD = 0.0f;
        private float chargeCDMax = 4.0f;

        public bool IsCharging { get; set; }

        public override void _Ready()
        {
            base._Ready();

            Godot.Collections.Dictionary<string, AIBehaviour> snailBehaviors = new Godot.Collections.Dictionary<string, AIBehaviour>();

            snailBehaviors.Add("idle", new NoBehaviour(aIManager, new Func<AIBehaviour.TransitionTestResult>[] {
            // idle -> wander
            () => {
                return new AIBehaviour.TransitionTestResult(hasTarget, "wander");
            }
        }));

            snailBehaviors.Add("wander", new WanderStraightBehaviour(aIManager, new Func<AIBehaviour.TransitionTestResult>[] {
            // wander -> charge
            () => {
                // If has target, charge cooldown <= 0, target is visible, and positions are lined up
                return new AIBehaviour.TransitionTestResult(hasTarget && chargeCD <= 0.0f && AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, GetWorld2d().DirectSpaceState, AIManager.visibilityLayer, new Godot.Collections.Array() {this}) && DoPositionsLineUp(GlobalPosition, aIManager.lastTarget.GlobalPosition), "charge");
            }
        }));

            snailBehaviors.Add("charge", new ChargeBehaviour(aIManager, () => { return chargeDirection; }, new Func<AIBehaviour.TransitionTestResult>[] {
            // charge -> wander
            () => {
                // if has target, and is not charging
                return new AIBehaviour.TransitionTestResult(hasTarget && !IsCharging, "wander");
            }
         }));

            snailBehaviors.Add("dead", new NoBehaviour(aIManager, new Func<AIBehaviour.TransitionTestResult>[] { }));

            aIManager.Behaviours = snailBehaviors;

            checkSlideCollisions = true;
            acceleration = 0.25f;
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            if (chargeCD > 0.0f)
            {
                chargeCD -= delta;

                if (chargeCD < 0.0f)
                    chargeCD = 0.0f;
            }
        }

        public override float GetMaxSpeed()
        {
            if (IsCharging)
                return chargeSpeed;

            return base.GetMaxSpeed();
        }

        public override (string name, bool flipH, int freezeFrame) GetSpriteAnimation()
        {
            if (IsCharging)
            {
                var chargeAnim = base.GetSpriteAnimation();
                chargeAnim.name = "shell_" + chargeAnim.name;

                return chargeAnim;
            }
            else
            {
                return base.GetSpriteAnimation();
            }
        }

        public void ResetChargeCooldown()
        {
            chargeCD = chargeCDMax;
        }

        private bool DoPositionsLineUp(Vector2 posA, Vector2 posB)
        {
            if (Mathf.IsEqualApprox(posA.y, posB.y, 8.0f))
            {
                // Lines up on y axis
                if (posA.x > posB.x)
                {
                    chargeDirection = Direction.Left;
                }
                else
                {
                    chargeDirection = Direction.Right;
                }

                return true;
            }
            else if (Mathf.IsEqualApprox(posA.x, posB.x, 8.0f))
            {
                // Lines up on x axis
                if (posA.y > posB.y)
                {
                    chargeDirection = Direction.Up;
                }
                else
                {
                    chargeDirection = Direction.Down;
                }

                return true;
            }

            return false;
        }
    }
}