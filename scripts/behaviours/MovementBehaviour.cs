using Godot;
using System;

public abstract class MovementBehaviour : AIBehaviour
{
    public bool isOnPath = false;

    public MovementBehaviour(AIManager manager, Func<TransitionTestResult>[] transitions) : base(manager, transitions)
    {

    }

    // Tolerance should be at least collision width
    protected Vector2 SteerToNextPoint(Godot.Collections.Array<Vector2> path, float tolerance = 3.0f)
    {
        Vector2 startPos = mgr.owner.GlobalPosition;
        Vector2 targetDirection = Vector2.Zero;

        for (int i = 0; i < path.Count; ++i)
        {
            float distToNextPoint = startPos.DistanceTo(path[0]);

            targetDirection = (path[0] - startPos).Normalized();

            if (distToNextPoint > tolerance)
            {
                break;
            }
            else
            {
                // Reached next point
                path.RemoveAt(0);
            }
        }

        if (path.Count == 0)
        {
            isOnPath = false;

            return Vector2.Zero;
        }

        return targetDirection;
    }
}