using Godot;
using System;

namespace Oubliette.AI
{
    public class GoblinWBearTrap : AICharacter
    {
        private float trapCD = 0.0f;
        private float returnFromFleeCD = 0.0f;

        public override void _Ready()
        {
            base._Ready();

            Godot.Collections.Dictionary<string, AIBehaviour> behaviors = new Godot.Collections.Dictionary<string, AIBehaviour>();

            behaviors.Add("idle", new NoBehaviour(AIManager, new Func<AIBehaviour.TransitionTestResult>[] {
                // idle -> wander
                () => {
                    return new AIBehaviour.TransitionTestResult(HasTarget, "wander");
                }
            }));

            behaviors.Add("wander", new WanderBehaviour(16f, new Vector2(3f, 8f), AIManager, new Func<AIBehaviour.TransitionTestResult>[] {
                // wander -> place_trap
                () => {
                    // If cooldown is 0, target is within range, target is visible and no tiles block path
                    Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
                    return new AIBehaviour.TransitionTestResult(HasTarget && trapCD <= 0.0f && GlobalPosition.DistanceTo(AIManager.LastTarget.GlobalPosition) <= attackRange && AIManager.TraceToTarget(GlobalPosition, AIManager.LastTarget, spaceState, AIManager.VisibilityLayer, new Godot.Collections.Array() {this}) && AIManager.TraceToTarget(GlobalPosition, AIManager.LastTarget, spaceState, AIManager.TilesLayer, new Godot.Collections.Array() {this}), "place_trap");
                },
                // wander -> flee_player
                () => {
                    // If target is within range, target is visible and no tiles block path
                    Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
                    return new AIBehaviour.TransitionTestResult(HasTarget && GlobalPosition.DistanceTo(AIManager.LastTarget.GlobalPosition) <= attackRange && AIManager.TraceToTarget(GlobalPosition, AIManager.LastTarget, spaceState, AIManager.VisibilityLayer, new Godot.Collections.Array() {this}) && AIManager.TraceToTarget(GlobalPosition, AIManager.LastTarget, spaceState, AIManager.TilesLayer, new Godot.Collections.Array() {this}), "place_trap");
                }
            }));

            behaviors.Add("place_trap", new PlaceTrapBehaviour(GD.Load<PackedScene>("res://scenes/BearTrap.tscn"), AIManager, new Func<AIBehaviour.TransitionTestResult>[] {
                // place_trap -> flee_player
                () => {
                    return new AIBehaviour.TransitionTestResult(true, "flee_player");
                }
            }));

            behaviors.Add("flee_player", new FleeBehaviour(AIManager, new Func<AIBehaviour.TransitionTestResult>[] {
                // flee_player -> place_trap
                () => {
                    Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
                    return new AIBehaviour.TransitionTestResult(returnFromFleeCD <= 0.0f && trapCD <= 0.0f && GlobalPosition.DistanceTo(AIManager.LastTarget.GlobalPosition) <= attackRange && AIManager.TraceToTarget(GlobalPosition, AIManager.LastTarget, spaceState, AIManager.VisibilityLayer, new Godot.Collections.Array() {this}) && AIManager.TraceToTarget(GlobalPosition, AIManager.LastTarget, spaceState, AIManager.TilesLayer, new Godot.Collections.Array() {this}), "place_trap");
                },
                // flee_player -> wander
                () => {
                    Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
                    return new AIBehaviour.TransitionTestResult(returnFromFleeCD <= 0.0f && GlobalPosition.DistanceTo(AIManager.LastTarget.GlobalPosition) > attackRange || !AIManager.TraceToTarget(GlobalPosition, AIManager.LastTarget, spaceState, AIManager.VisibilityLayer, new Godot.Collections.Array() {this}) || !AIManager.TraceToTarget(GlobalPosition, AIManager.LastTarget, spaceState, AIManager.TilesLayer, new Godot.Collections.Array() {this}), "wander");

                }
            }));

            behaviors.Add("dead", new NoBehaviour(AIManager, new Func<AIBehaviour.TransitionTestResult>[] { }));

            AIManager.Behaviours = behaviors;
            AIManager.SetCurrentBehaviour("idle");

            AIManager.Connect(nameof(AIManager.BehaviourChanged), this, nameof(BehaviourChanged));
        }

        private void BehaviourChanged(string behavior)
        {
            if (behavior == "flee_player")
            {
                returnFromFleeCD += 2f;
            }
            else if (behavior == "place_trap")
            {
                trapCD += 5f;
            }
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            ProcessCooldown(ref trapCD, delta);
            ProcessCooldown(ref returnFromFleeCD, delta);
        }

        private void ProcessCooldown(ref float CD, float delta)
        {
            if (CD > 0.0f)
            {
                CD -= delta;

                if (CD < 0.0f)
                {
                    CD = 0.0f;
                }
            }
        }
    }
}