using Godot;
using System;

public class Lockpicking : Node2D
{
    private enum PickState
    {
        Idle,
        Moving,
        PickingUp,
        Falling
    }

    private int[] pinPositions = { 38, 74, 110, 146, 182 };
    private int pickPosition = 0;
    private int maxPickPosition = 4;
    private PickState pickState = PickState.Idle;

    // Nodes
    private Sprite lockpick;
    private Sprite[] pins = { null, null, null, null, null };
    private Tween tween;

    public override void _Ready()
    {
        lockpick = GetNode<Sprite>("Lockpick");
        for (int i = 0; i < pinPositions.Length; ++i)
        {
            pins[i] = GetNode<Sprite>("Pin" + (i + 1));
        }
        tween = GetNode<Tween>("Tween");

        tween.Connect("tween_all_completed", this, nameof(TweenAllComplete));
    }

    public override void _Input(InputEvent evt)
    {
        base._Input(evt);

        if (evt is InputEventMouseMotion mme)
        {
            Vector2 d = mme.Relative;

            if (Mathf.Abs(d.x) > 20.0f && pickState == PickState.Idle)
            {
                if (d.x > 0 && pickPosition < maxPickPosition)
                {
                    pickState = PickState.Moving;
                    MovePick(false);
                }
                else if (pickPosition > 0)
                {
                    pickState = PickState.Moving;
                    MovePick(true);
                }
            }
            else if(d.y <= -20.0f && pickState == PickState.Idle)
            {
                pickState = PickState.PickingUp;
                StartPicking();
            }
        }
    }

    private void MovePick(bool reverse)
    {
        int idx = pickPosition + (reverse ? -1 : 1);
        int xPos = pinPositions[idx];
        tween.InterpolateProperty(lockpick, "position", lockpick.Position, new Vector2(xPos, 138.0f), 0.5f);

        tween.Start();
        pickPosition += (reverse ? -1 : 1);
    }

    private void StartPicking()
    {
        tween.InterpolateProperty(lockpick, "position", lockpick.Position, lockpick.Position + new Vector2(0, -48), 0.25f);

        tween.Start();
    }

    private void StartFalling()
    {
        tween.InterpolateProperty(lockpick, "position", lockpick.Position, lockpick.Position + new Vector2(0, 48), 0.5f);

        tween.Start();
    }

    private void TweenAllComplete()
    {
        switch (pickState)
        {
            case PickState.Moving:
                pickState = PickState.Idle;
                break;

            case PickState.PickingUp:
                pickState = PickState.Falling;
                StartFalling();
                break;

            case PickState.Falling:
                pickState = PickState.Idle;
                break;

            default:
                break;
        }
    }
}
