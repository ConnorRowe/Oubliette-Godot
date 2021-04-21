using Godot;
using System;

// Behaviour that does nothing
public class NoBehaviour : AIBehaviour
{
    public NoBehaviour(AIManager manager, Func<TransitionTestResult>[] transitions) : base(manager, transitions) { }
    public override void OnBehaviourStart() { }
    public override void Process(float delta) { }
    public override void OnBehaviourEnd() { }

    public override Vector2 Steer()
    {
        return Vector2.Zero;
    }
}