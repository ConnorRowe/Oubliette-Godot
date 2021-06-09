using Godot;
using System;

namespace Oubliette.AI
{
    public class GiantSlime : BossEnemy, ICastsSpells
    {
        private bool attackSwitch = false;
        private ProjectileSpell slimeBallSpell = new ProjectileSpell("Slime Ball", 1, 400.0f, 100.0f, 140.0f, 0.0f, new Color(0.682353f, 0.917647f, 0.301961f), null, null, "res://scenes/SlimeBallProjectile.tscn");
        private ProjectileSpell tripleSlimeBallSpell = new MultiSpreadProjectileSpell(3, Mathf.Pi / 5f, "Triple Slime Ball", 1, 400.0f, 100.0f, 140.0f, 0.0f, new Color(0.682353f, 0.917647f, 0.301961f), null, null, "res://scenes/SlimeBallProjectile.tscn");

        public override void _Ready()
        {
            base._Ready();

            AIManager.Connect(nameof(AIManager.BehaviourChanged), this, nameof(BehaviourChanged));

            Godot.Collections.Dictionary<string, AIBehaviour> behaviors = new Godot.Collections.Dictionary<string, AIBehaviour>();

            behaviors.Add("idle", new NoBehaviour(AIManager, new Func<AIBehaviour.TransitionTestResult>[] {
            // idle -> wait_to_attack
            () => {
                return new AIBehaviour.TransitionTestResult(hasTarget, "wait_to_attack");
            }
        }));

            behaviors.Add("wait_to_attack", new TimerBehaviour(AIManager, 2.5f, new Func<AIBehaviour.TransitionTestResult>[] {
            // wait_to_attack -> attack_anim
            () => {
                return new AIBehaviour.TransitionTestResult(hasTarget, "attack_anim");
            }
        }));

            behaviors.Add("attack_anim", new PlayAnimationBehaviour(AIManager, GetNode<AnimationPlayer>("BossAnimations"), "Jump", new Func<AIBehaviour.TransitionTestResult>[] {
            // attack_anim -> shoot_once
            () => {
                return new AIBehaviour.TransitionTestResult(hasTarget && !attackSwitch, "shoot_once");
            },
            // attack_anim -> shoot_triple
            () => {
                return new AIBehaviour.TransitionTestResult(hasTarget && attackSwitch, "shoot_triple");
            }
        }));

            behaviors.Add("shoot_once", new CastSpellOnceBehaviour(AIManager, slimeBallSpell, new Func<AIBehaviour.TransitionTestResult>[] {
            // shoot_once -> wait_to_attack
            () => {
                return new AIBehaviour.TransitionTestResult(hasTarget, "wait_to_attack");
            }
        }));

            behaviors.Add("shoot_triple", new CastSpellOnceBehaviour(AIManager, tripleSlimeBallSpell, new Func<AIBehaviour.TransitionTestResult>[] {
            // shoot_triple -> wait_to_attack
            () => {
                return new AIBehaviour.TransitionTestResult(hasTarget, "wait_to_attack");
            }
        }));

            behaviors.Add("dead", new NoBehaviour(AIManager, new Func<AIBehaviour.TransitionTestResult>[] { }));

            AIManager.Behaviours = behaviors;

            AIManager.SteerOverride = StayInRangeOfPlayer;
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            debugLabel.Text = GetSpellDirection().ToString();
        }

        protected override Vector2 GetSpellDirection()
        {
            if (AIManager.LastTarget != null)
                return (AIManager.LastTarget.GlobalPosition - GlobalPosition).Normalized();
            else
                return Vector2.Zero;
        }
        public override Vector2 GetSpellSpawnPos()
        {
            return GlobalPosition + (GetSpellDirection() * 16.0f);
        }

        private void BehaviourChanged(string behaviour)
        {
            if (behaviour.Equals("attack_anim"))
            {
                attackSwitch = !attackSwitch;
            }
        }

        private Vector2 StayInRangeOfPlayer()
        {
            if(AIManager.LastTarget == null)
                return Vector2.Zero;

            Vector2 playerPos = AIManager.LastTarget.Position;

            if(GlobalPosition.DistanceTo(playerPos) <= 256.0f)
                return Vector2.Zero;

            return GlobalPosition.DirectionTo(playerPos);
        }
    }
}