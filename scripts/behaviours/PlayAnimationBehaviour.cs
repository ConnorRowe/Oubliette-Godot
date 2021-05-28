using Godot;
using System;

public class PlayAnimationBehaviour : AIBehaviour
{
    AnimationPlayer animationPlayer;
    string animationName;

    public PlayAnimationBehaviour(AIManager manager, AnimationPlayer animationPlayer, string animationName, Func<TransitionTestResult>[] transitions) : base(manager, transitions)
    {
        this.animationPlayer = animationPlayer;
        this.animationName = animationName;

        animationPlayer.Connect("animation_finished", this, nameof(AnimationFinished));
    }

    public override void OnBehaviourStart()
    {
        mgr.CanTryTransition = false;
        animationPlayer.Play(animationName);
    }

    public override void Process(float delta)
    {
        mgr.targetPosCache = mgr.lastTarget.GlobalPosition;
    }

    public override Vector2 Steer()
    {
        return Vector2.Zero;
    }

    public override void OnBehaviourEnd()
    {
    }

    public void AnimationFinished(string animationName)
    {
        mgr.CanTryTransition = true;
    }
}
