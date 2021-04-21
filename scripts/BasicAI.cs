using Godot;
using System;

public class BasicAI : Character
{
    private enum AIState
    {
        Idle,
        FollowingPath,
        FollowingTarget,
        Attack,
        Dead,
    }

    private const uint visibilityLayer = 0b0100;
    private const uint tilesLayer = 0b0001;

    private Godot.Collections.Array<Vector2> path;
    private bool isOnPath = false;
    private bool canAttack = true;
    private bool canWander = false;
    private AIState aIState = AIState.Idle;
    private Vector2 lastKnownTargetPos;
    private RandomNumberGenerator rng = new RandomNumberGenerator();

    // Nodes
    private World world;
    private Timer aiTimer;
    private Area2D hitbox;
    private Player targetPlayer;
    private Line2D line;

    //Resource
    private CircleShape2D detectionShape = new CircleShape2D();

    // Export
    [Export]
    private NodePath _aiTimerPath;
    [Export]
    private float detectionRadius = 80;
    [Export(hintString: "Default = 64")]
    private float maxMovementSpeed = 64f;
    [Export]
    private float attackSpeed = 1.0f;


    public override void _Ready()
    {
        base._Ready();
        rng.Randomize();

        this.maxSpeed = maxMovementSpeed;
        detectionShape.Radius = detectionRadius;
        world = GetParent<World>();
        hitbox = GetNode<Area2D>("hitbox");
        aiTimer = GetNode<Timer>(_aiTimerPath);

        aiTimer.Connect("timeout", this, nameof(CheckForPlayer));

        line = GetParent().GetNode<Line2D>("Line2D");


        world.GetDebugOverlay().TrackFunc(nameof(GetAIStateName), this, "Slime state");
        StartWanderTimer();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
    }

    public override Vector2 GetInputAxis(float delta)
    {
        if(isDead)
            return Vector2.Zero;

        switch (aIState)
        {
            case AIState.Idle:
                if (path != null && path.Count > 0)
                {
                    return SteerToNextPoint();
                }
                return Vector2.Zero;

            case AIState.FollowingPath:
                if (isOnPath)
                {
                    // float moveDistance = maxSpeed * delta;
                    // return (MoveAlongPath(moveDistance) - GlobalPosition).Normalized();

                    return SteerToNextPoint();
                }
                break;

            case AIState.FollowingTarget:
                return (targetPlayer.GlobalPosition - GlobalPosition).Normalized();
            case AIState.Attack:
                break;
            case AIState.Dead:
                break;
        }

        return Vector2.Zero;
    }

    public override float GetMaxSpeed()
    {
        switch (aIState)
        {
            case AIState.Idle:
                return maxSpeed / 4.0f;
        }

        return base.GetMaxSpeed();
    }

    public string GetAIStateName()
    {
        switch (aIState)
        {
            case AIState.Idle:
                return "Idle";
            case AIState.FollowingTarget:
                return "FollowingTarget";
            case AIState.FollowingPath:
                return "FollowingPath";
            case AIState.Attack:
                return "Attack";
            case AIState.Dead:
                return "Dead";
        }

        return "error";
    }

    public void SetPath(Vector2[] path)
    {
        this.path = new Godot.Collections.Array<Vector2>(path);

        if (path.Length > 0)
        {
            isOnPath = true;
        }

        line.Points = path;
        line.DefaultColor = Colors.NavyBlue;
    }

    private Vector2 SteerToNextPoint()
    {
        Vector2 startPos = Position;
        Vector2 targetDirection = Vector2.Zero;

        // Should be at least collision width
        float tolerance = 3f;

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

            // Reached path end
            SetAiIdle();

            return Vector2.Zero;
        }

        return targetDirection;
    }

    private void CheckForPlayer()
    {
        Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;

        if (aIState == AIState.Idle)
        {
            //Check if player's in range and visible

            Physics2DShapeQueryParameters shapeQuery = new Physics2DShapeQueryParameters()
            {
                CollisionLayer = 512,
                Transform = new Transform2D(0.0f, this.GlobalPosition),
                ShapeRid = detectionShape.GetRid(),
                Exclude = new Godot.Collections.Array { this },
                CollideWithAreas = true,
                CollideWithBodies = true,
            };

            shapeQuery.SetShape(detectionShape);

            Godot.Collections.Array results = spaceState.IntersectShape(shapeQuery);

            foreach (Godot.Collections.Dictionary result in results)
            {
                if (result["collider"] != null)
                {
                    if (result["collider"] is Player player)
                    {
                        // Check line of sight
                        if (TraceToTarget(player, spaceState, visibilityLayer))
                        {
                            // Player is visible
                            targetPlayer = player;

                            // Check for player more often now
                            aiTimer.WaitTime = 0.5f;


                            // Can enemy move directly to player?
                            if (TraceToTarget(player, spaceState, tilesLayer))
                            {
                                // Move directly towards
                                aIState = AIState.FollowingTarget;
                                lastKnownTargetPos = targetPlayer.GlobalPosition;
                            }
                            else
                            {
                                // Move via Nav Path
                                SetPath(world.GetNavPath(this.GlobalPosition, targetPlayer.GlobalPosition));
                                aIState = AIState.FollowingPath;
                            }
                        }
                    }
                }
            }
        }
        else if(aIState != AIState.Dead) // AI state is not idle
        {
            // Update target path
            if (TraceToTarget(targetPlayer, spaceState, visibilityLayer))
            {
                // found

                // Can enemy move directly to player?
                if (TraceToTarget(targetPlayer, spaceState, tilesLayer))
                {
                    // Move directly towards
                    aIState = AIState.FollowingTarget;
                    lastKnownTargetPos = targetPlayer.GlobalPosition;
                }
                else
                {
                    // Move via Nav Path
                    SetPath(world.GetNavPath(this.GlobalPosition, targetPlayer.GlobalPosition));
                    aIState = AIState.FollowingPath;
                }

                if (canAttack && this.GlobalPosition.DistanceTo(targetPlayer.GlobalPosition) <= 10f)
                {
                    canAttack = false;
                    targetPlayer.TakeDamage(1, this);

                    // Cooldown timer to reset canAttack
                    GetTree().CreateTimer(attackSpeed, false).Connect("timeout", this, nameof(ResetCanAttack));
                }
            }
            else
            {
                // Player no longer visible, move to last known location
                if (aIState == AIState.FollowingTarget)
                {
                    SetPath(world.GetNavPath(this.GlobalPosition, lastKnownTargetPos));
                    aIState = AIState.FollowingPath;
                }
            }
        }
    }

    private bool TraceToTarget(Node2D target, Physics2DDirectSpaceState spaceState, uint collisionLayer)
    {
        var los = spaceState.IntersectRay(this.GlobalPosition + new Vector2(0, 0), target.GlobalPosition, new Godot.Collections.Array { this, hitbox }, collisionLayer, collideWithAreas: true, collideWithBodies: true);

        // debug
        if (los.Count == 0)
        {
            line.Points = new Vector2[] { this.GlobalPosition + new Vector2(0, -9), target.GlobalPosition };
            line.DefaultColor = Colors.Green;
        }

        return los.Count <= 0; //|| (los.Contains("collider") && (los["collider"] as Node).Owner != target);
    }

    public void ResetCanAttack()
    {
        canAttack = true;
    }

    private void StartWanderTimer()
    {
        GetTree().CreateTimer(2.0f).Connect("timeout", this, nameof(ResetCanWander));
    }

    public void ResetCanWander()
    {
        canWander = true;

        if (aIState == AIState.Idle)
        {
            int tries = 10;
            Vector2[] newPath = { };

            do
            {
                newPath = world.GetNavPath(GlobalPosition, GlobalPosition + new Vector2(rng.RandfRange(-20, 20), rng.RandfRange(-20, 20)));

                --tries;
            } while (tries > 0 && newPath.Length > 0);

            if (newPath.Length > 0)
            {
                SetPath(newPath);
            }
        }

        StartWanderTimer();
    }

    private void SetAiIdle()
    {
        aIState = AIState.Idle;
        aiTimer.WaitTime = 1.0f;
    }

    public override void Die()
    {
        this.aIState = AIState.Dead;

        base.Die();
    }
}
