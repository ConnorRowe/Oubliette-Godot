using Godot;
using System;

public class FollowPathBehaviour : MovementBehaviour
{
    private Func<Vector2[]> getPath;
    private Godot.Collections.Array<Vector2> path = new Godot.Collections.Array<Vector2>() { };
    private SceneTreeTimer timer;
    public bool cacheTargetPos = false;

    public FollowPathBehaviour(AIManager manager, Func<Vector2[]> getPath, Func<TransitionTestResult>[] transitions) : base(manager, transitions)
    {
        this.getPath = getPath;
    }

    public override void OnBehaviourStart()
    {
        UpdatePath();
    }

    public override void Process(float delta)
    {
        if (cacheTargetPos)
        {
            mgr.targetPosCache = mgr.lastTarget.GlobalPosition;
        }
    }
    public override void OnBehaviourEnd()
    {
        timer.Disconnect("timeout", this, nameof(UpdatePath));
    }

    public override Vector2 Steer()
    {
        return SteerToNextPoint(path, (mgr.owner as AICharacter).PathTolerance);
    }

    public void UpdatePath()
    {
        Vector2[] pathArray = getPath();
        path = new Godot.Collections.Array<Vector2>(pathArray);

        timer = mgr.owner.GetTree().CreateTimer(0.5f);
        timer.Connect("timeout", this, nameof(UpdatePath));

        // mgr.world.GetDebugOverlay().TrackLine(pathArray);
    }


}
