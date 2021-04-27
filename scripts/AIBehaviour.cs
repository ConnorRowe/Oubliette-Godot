using Godot;
using System;

public abstract class AIBehaviour : Godot.Reference
{
    public struct TransitionTestResult
    {
        public bool Success;
        public string NextBehaviour;

        public TransitionTestResult(bool success, string nextBehaviour)
        {
            this.Success = success;
            this.NextBehaviour = nextBehaviour;
        }
    }

    public static TransitionTestResult BlankTTR()
    {
        return new TransitionTestResult(false, "");
    }

    protected readonly AIManager mgr;
    // Array of what behaviours it is able to transition to, returns true if it can transition
    public Func<TransitionTestResult>[] transitions;
    // Direction to steer the character
    public abstract Vector2 Steer();
    public abstract void OnBehaviourStart();
    public abstract void OnBehaviourEnd();
    public abstract void Process(float delta);

    public AIBehaviour() { }
    public AIBehaviour(AIManager manager, Func<TransitionTestResult>[] transitions)
    {
        mgr = manager;
        this.transitions = transitions;
    }
}
