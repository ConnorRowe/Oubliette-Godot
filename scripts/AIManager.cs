using Godot;
using System;

namespace Oubliette.AI
{
    public class AIManager : Godot.Reference
    {
        public static uint VisibilityLayer { get; set; } = 0b0100;
        public static uint TilesLayer { get; set; } = 0b0001;

        public readonly World World;
        public readonly Character Owner;
        public Godot.Collections.Dictionary<string, AIBehaviour> Behaviours { get; set; } = new Godot.Collections.Dictionary<string, AIBehaviour>();
        public Func<AIBehaviour.TransitionTestResult>[] GlobalTransitions { get; set; } = new Func<AIBehaviour.TransitionTestResult>[] { };
        public string CurrentBehaviour { get; set; } = "";
        public Character LastTarget { get; set; }
        public Vector2 TargetPosCache { get; set; } = Vector2.Zero;
        public RandomNumberGenerator rng { get; set; } = new RandomNumberGenerator();
        public bool CanTryTransition { get; set; } = true;
        public Func<Vector2> SteerOverride { get; set; } = null;

        private SceneTreeTimer transitionTimer;

        [Signal]
        public delegate void BehaviourChanged(string behavior);
        [Signal]
        public delegate void Fire();

        public AIManager() { }
        public AIManager(Character owner, World world)
        {
            this.Owner = owner;
            this.World = world;

            transitionTimer = owner.GetTree().CreateTimer(0.5f);
            transitionTimer.Connect("timeout", this, nameof(TryTransition));

            rng.Randomize();
        }

        public void TryTransition()
        {
            if (CurrentBehaviour.Empty())
                return;

            transitionTimer = Owner.GetTree().CreateTimer(0.5f);
            transitionTimer.Connect("timeout", this, nameof(TryTransition));

            if (!CanTryTransition)
                return;

            if (!CurrentBehaviour.Empty())
            {
                AIBehaviour behaviour = Behaviours[CurrentBehaviour];

                foreach (Func<AIBehaviour.TransitionTestResult> transitionTest in behaviour.transitions)
                {
                    AIBehaviour.TransitionTestResult result = transitionTest();

                    if (result.Success)
                    {
                        MakeTransition(result.NextBehaviour);
                        return;
                    }
                }
            }

            // Global Transitions
            foreach (Func<AIBehaviour.TransitionTestResult> transitionTest in GlobalTransitions)
            {
                AIBehaviour.TransitionTestResult result = transitionTest();

                if (result.Success)
                {
                    MakeTransition(result.NextBehaviour);
                    break;
                }
            }
        }

        private void MakeTransition(string behaviour)
        {
            if (!CurrentBehaviour.Empty())
            {
                Behaviours[CurrentBehaviour].OnBehaviourEnd();
            }

            CurrentBehaviour = behaviour;
            if (!CurrentBehaviour.Empty())
            {
                Behaviours[CurrentBehaviour].OnBehaviourStart();
            }

            EmitSignal(nameof(BehaviourChanged), CurrentBehaviour);
        }

        public void Process(float delta)
        {
            if (!CurrentBehaviour.Empty())
            {
                Behaviours[CurrentBehaviour].Process(delta);
            }
        }

        public Vector2 Steer()
        {
            if (SteerOverride != null)
                return SteerOverride();

            if (!CurrentBehaviour.Empty())
            {
                return Behaviours[CurrentBehaviour].Steer();
            }

            return Vector2.Zero;
        }

        public void SetCurrentBehaviour(string behaviourKey)
        {
            MakeTransition(behaviourKey);
        }

        public void AddBehaviour(string behaviourKey, AIBehaviour behaviour)
        {
            Behaviours.Add(behaviourKey, behaviour);
        }

        // Does not remove transitions to this behaviour - be careful
        public void RemoveBehaviour(string behaviourKey)
        {
            Behaviours.Remove(behaviourKey);
        }

        public void StopTryTransitionLoop()
        {
            SetCurrentBehaviour("");

            if (transitionTimer.IsConnected("timeout", this, nameof(TryTransition)))
                transitionTimer.Disconnect("timeout", this, nameof(TryTransition));
        }

        public static Player CheckForPlayer(Physics2DShapeQueryParameters shapeQuery, Physics2DDirectSpaceState spaceState)
        {
            //Check if player is inside shape query

            Godot.Collections.Array results = spaceState.IntersectShape(shapeQuery);

            foreach (Godot.Collections.Dictionary result in results)
            {
                if (result["collider"] != null)
                {
                    if (result["collider"] is Player player)
                    {
                        return player;
                    }
                }
            }

            return null;
        }

        public static bool TraceToTarget(Vector2 startPos, Node2D target, Physics2DDirectSpaceState spaceState, uint collisionLayer, Godot.Collections.Array exclude)
        {
            var los = spaceState.IntersectRay(startPos + new Vector2(0, 0), target.GlobalPosition, exclude, collisionLayer, collideWithAreas: true, collideWithBodies: true);

            return los.Count <= 0; //|| (los.Contains("collider") && (los["collider"] as Node).Owner != target);
        }

        public static Vector2[] GetNavPathGlobal(Vector2 pointA, Vector2 pointB, Navigation2D navigation)
        {
            Vector2 localA = navigation.ToLocal(pointA);
            Vector2 localB = navigation.ToLocal(pointB);
            Vector2[] navpath = navigation.GetSimplePath(localA, localB, optimize: true);
            for (int i = 0; i < navpath.Length; ++i)
            {
                navpath[i] = navigation.ToGlobal(navpath[i]);
            }
            return navpath;
        }
    }
}