using Godot;
using System;

public class AIManager : Godot.Object
{
    public static uint visibilityLayer = 0b0100;
    public static uint tilesLayer = 0b0001;

    public readonly World world;
    public readonly Character owner;
    public Godot.Collections.Dictionary<string, AIBehaviour> Behaviours = new Godot.Collections.Dictionary<string, AIBehaviour>();
    public Func<AIBehaviour.TransitionTestResult>[] globalTransitions;

    public string CurrentBehaviour = "";
    public Character lastTarget;
    public Vector2 targetPosCache = Vector2.Zero;
    public RandomNumberGenerator rng = new RandomNumberGenerator();

    private SceneTreeTimer transitionTimer;

    [Signal]
    public delegate void BehaviourChanged(string behavior);
    [Signal]
    public delegate void Fire();

    public AIManager() { }
    public AIManager(Character owner, World world)
    {
        this.owner = owner;
        this.world = world;
        
        transitionTimer = owner.GetTree().CreateTimer(0.5f);
        transitionTimer.Connect("timeout", this, nameof(TryTransition));

        rng.Randomize();
    }

    public void TryTransition()
    {
        transitionTimer = owner.GetTree().CreateTimer(0.5f);
        transitionTimer.Connect("timeout", this, nameof(TryTransition));

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
        foreach (Func<AIBehaviour.TransitionTestResult> transitionTest in globalTransitions)
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
        Behaviours[CurrentBehaviour].OnBehaviourStart();

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

}
