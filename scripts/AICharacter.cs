using Godot;
using System;

public class AICharacter : Character, ICastsSpells, IIntersectsPlayerHitArea
{
    public AIManager aIManager;
    protected Particles2D deathParticles;
    private CircleShape2D detectionShape = new CircleShape2D();
    protected Label debugLabel;
    protected Tween tween;
    protected Sprite detectionNotifier;
    public bool hasTarget = false;

    public Physics2DShapeQueryParameters shapeQuery;
    public World world;
    public float ogMaxSpeed;
    public IProvidesNav navProvider;
    public Vector2 initPos = Vector2.Zero;

    // Export
    [Export]
    private NodePath _deathParticlesPath;
    [Export]
    private NodePath _tweenPath;
    [Export]
    private NodePath _detectionNotifierPath;

    [Export(hintString: "How fast the character can run")]
    private float maxMovementSpeed = 64f;
    [Export(hintString: "How frequent the character can attack")]
    protected float attackSpeed = 1.0f;
    [Export(hintString: "How close the character has to be to its target to attack")]
    protected float attackRange = 10.0f;
    [Export(hintString: "The radius of its detection sphere")]
    private float detectionRadius = 80;
    [Export(hintString: "How close the character must be to a path point for it to count as reached.")]
    public float PathTolerance = 7.5f;

    // Signals
    [Signal]
    public delegate void Died(AICharacter aICharacter);
    [Signal]
    public delegate void PlayerIntersected(Player player);


    public override void _Ready()
    {
        base._Ready();

        this.maxSpeed = maxMovementSpeed;
        ogMaxSpeed = this.maxSpeed;

        if (GetParent() is World)
            world = GetParent<World>();
        else if (GetParent().GetParent() is World)
            world = GetParent().GetParent<World>();

        debugLabel = GetNode<Label>("debugLabel");
        deathParticles = GetNode<Particles2D>(_deathParticlesPath);
        tween = GetNode<Tween>(_tweenPath);
        detectionNotifier = GetNode<Sprite>(_detectionNotifierPath);
        detectionShape.Radius = detectionRadius;

        shapeQuery = new Physics2DShapeQueryParameters()
        {
            CollisionLayer = 512,
            Transform = new Transform2D(0.0f, this.GlobalPosition),
            ShapeRid = detectionShape.GetRid(),
            Exclude = new Godot.Collections.Array { this },
            CollideWithAreas = true,
            CollideWithBodies = true,
        };

        shapeQuery.SetShape(detectionShape);

        aIManager = new AIManager(this, world);

        Godot.Collections.Dictionary<string, AIBehaviour> behaviors = new Godot.Collections.Dictionary<string, AIBehaviour>();

        // old behaviours that worked in overworld
        // behaviors.Add("wander", new WanderBehaviour(aIManager, new Func<AIBehaviour.TransitionTestResult>[] {

        //     // wander -> attack
        //     () => {
        //         return new AIBehaviour.TransitionTestResult(aIManager.lastTarget != null && GlobalPosition.DistanceTo(aIManager.lastTarget.GlobalPosition) <= attackRange, "attack");
        //     },

        //     // wander -> follow_target
        //     () => {
        //     AIBehaviour.TransitionTestResult result = new AIBehaviour.TransitionTestResult(false, "follow_target");

        //     Player player = AIManager.CheckForPlayer(GetDetectionShapeQuery(), GetWorld2d().DirectSpaceState);

        //     // Player is in range and visible
        //     if(player != null  && AIManager.TraceToTarget(GlobalPosition, player, GetWorld2d().DirectSpaceState, AIManager.visibilityLayer, new Godot.Collections.Array() { this }))
        //     {
        //         aIManager.lastTarget = player;
        //         result.Success = true;
        //     }

        //     return result;
        // }}));

        // behaviors.Add("follow_target", new FollowTargetBehaviour(aIManager, new Func<AIBehaviour.TransitionTestResult>[] {

        //     // follow_target -> attack
        //     () => {
        //         return new AIBehaviour.TransitionTestResult(GlobalPosition.DistanceTo(aIManager.lastTarget.GlobalPosition) <= attackRange, "attack");
        //     },

        //     // follow_target -> path_to_target
        //     () => {

        //         // If player is visible but tiles block path
        //         Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
        //         return new AIBehaviour.TransitionTestResult(AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.visibilityLayer, new Godot.Collections.Array() {this}) && !AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.tilesLayer, new Godot.Collections.Array() {this}), "path_to_target");
        //     },

        //     // follow_target -> path_to_last_pos
        //     () => {
        //         // If player is not visible
        //         Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
        //         return new AIBehaviour.TransitionTestResult(!AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.visibilityLayer, new Godot.Collections.Array() {this}), "path_to_last_pos");
        //     }
        // }));

        // behaviors.Add("path_to_target", new FollowPathBehaviour(aIManager, () =>
        // {

        //     return navProvider.GetNavigation().GetSimplePath(GlobalPosition, aIManager.lastTarget.GlobalPosition);
        // }, new Func<AIBehaviour.TransitionTestResult>[] {

        //     // path_to_target -> attack
        //     () => {
        //         return new AIBehaviour.TransitionTestResult(GlobalPosition.DistanceTo(aIManager.lastTarget.GlobalPosition) <= attackRange, "attack");
        //     },

        //     // path_to_target -> follow_target
        //     () => {
        //         // If player is visible and no tiles block path
        //         Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
        //         return new AIBehaviour.TransitionTestResult(AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.visibilityLayer, new Godot.Collections.Array() {this}) && AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.tilesLayer, new Godot.Collections.Array() {this}), "follow_target");
        //     },

        //     // path_to_target -> path_to_last_pos
        //     () => {
        //         // If player is not visible
        //         Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
        //         return new AIBehaviour.TransitionTestResult(!AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.visibilityLayer, new Godot.Collections.Array() {this}), "path_to_last_pos");
        //     }
        // })
        // { cacheTargetPos = true });

        // behaviors.Add("path_to_last_pos", new FollowPathBehaviour(aIManager, () =>
        // {
        //     return navProvider.GetNavigation().GetSimplePath(GlobalPosition, aIManager.targetPosCache);
        // }, new Func<AIBehaviour.TransitionTestResult>[] {

        //     // path_to_last_pos -> follow_target
        //     () => {
        // // If player is visible and no tiles block path
        // Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
        // return new AIBehaviour.TransitionTestResult(AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.visibilityLayer, new Godot.Collections.Array() {this}) && AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.tilesLayer, new Godot.Collections.Array() {this}), "follow_target");
        //     },

        //     // path_to_last_pos -> wander
        //     () => {
        //         // If close to targetPosCache
        //         return new AIBehaviour.TransitionTestResult(GlobalPosition.DistanceTo(aIManager.targetPosCache) <= 10.0f, "wander");
        //     }
        // }));

        // behaviors.Add("attack", new MeleeAttackBehaviour(aIManager, this.attackSpeed, this.attackRange, new Func<AIBehaviour.TransitionTestResult>[] {

        //     // attack -> follow_target
        //     () => {
        //         // If player is too far away but still visible
        //         Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
        //         return new AIBehaviour.TransitionTestResult(GlobalPosition.DistanceTo(aIManager.lastTarget.GlobalPosition) > attackRange && AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.visibilityLayer, new Godot.Collections.Array() {this}), "follow_target");
        //     },

        //     // attack -> wander
        //     () => {
        //         // If player is too far away or player is not visible
        //         Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
        //         return new AIBehaviour.TransitionTestResult(GlobalPosition.DistanceTo(aIManager.lastTarget.GlobalPosition) > attackRange || !AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.visibilityLayer, new Godot.Collections.Array() {this}), "wander");
        //     }
        // }));

        // behaviors.Add("dead", new NoBehaviour(aIManager, new Func<AIBehaviour.TransitionTestResult>[] { }));

        // aIManager.globalTransitions = new Func<AIBehaviour.TransitionTestResult>[] {() =>
        // {
        //     return new AIBehaviour.TransitionTestResult(this.isDead, "dead");
        // }};

        // aIManager.Behaviours = behaviors;
        // aIManager.SetCurrentBehaviour("wander");

        // behaviours for generated levels

        behaviors.Add("idle", new NoBehaviour(aIManager, new Func<AIBehaviour.TransitionTestResult>[] {
            // idle -> attack_target
            () => {
                // If has target, target is within range, target is visible and no tiles block path
                Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
                return new AIBehaviour.TransitionTestResult(hasTarget && GlobalPosition.DistanceTo(aIManager.lastTarget.GlobalPosition) <= attackRange && AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.visibilityLayer, new Godot.Collections.Array() {this}) && AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.tilesLayer, new Godot.Collections.Array() {this}), "attack_target");
            },
            // idle -> follow_target
            () => {
                return new AIBehaviour.TransitionTestResult(hasTarget, "follow_target");
            }
        }));

        behaviors.Add("follow_target", new FollowTargetBehaviour(aIManager, new Func<AIBehaviour.TransitionTestResult>[] {
            // follow_target -> attack_target
            () => {
                // If target is within range, target is visible and no tiles block path
                Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
                return new AIBehaviour.TransitionTestResult(GlobalPosition.DistanceTo(aIManager.lastTarget.GlobalPosition) <= attackRange && AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.visibilityLayer, new Godot.Collections.Array() {this}) && AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.tilesLayer, new Godot.Collections.Array() {this}), "attack_target");
            },
            // follow_target -> path_to_last_pos
            () => {
                // If target is not visible
                Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
                return new AIBehaviour.TransitionTestResult(!AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.visibilityLayer, new Godot.Collections.Array() {this}), "path_to_last_pos");
            },
        }));

        behaviors.Add("path_to_last_pos", new FollowPathBehaviour(aIManager, () =>
        {
            return AIManager.GetNavPathGlobal(GlobalPosition, aIManager.targetPosCache, navProvider.GetNavigation());
        }, new Func<AIBehaviour.TransitionTestResult>[] {
            // path_to_last_pos -> attack_target
            () => {
                // If target is within range, target is visible and no tiles block path
                Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
                return new AIBehaviour.TransitionTestResult(GlobalPosition.DistanceTo(aIManager.lastTarget.GlobalPosition) <= attackRange && AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.visibilityLayer, new Godot.Collections.Array() {this}) && AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.tilesLayer, new Godot.Collections.Array() {this}), "attack_target");
            },
            // path_to_last_pos -> follow_target
            () => {
                // If player is visible and no tiles block path
                Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
                return new AIBehaviour.TransitionTestResult(AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.visibilityLayer, new Godot.Collections.Array() {this}) && AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.tilesLayer, new Godot.Collections.Array() {this}), "follow_target");
            },
        }));

        behaviors.Add("attack_target", new MeleeAttackBehaviour(aIManager, attackSpeed, attackRange, new Func<AIBehaviour.TransitionTestResult>[] {
            // attack_target -> follow_target
            () => {
                return new AIBehaviour.TransitionTestResult(GlobalPosition.DistanceTo(aIManager.lastTarget.GlobalPosition) > attackRange, "follow_target");
            },
            // attack_target -> path_to_last_pos
            () => {
                // If target is not visible
                Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
                return new AIBehaviour.TransitionTestResult(!AIManager.TraceToTarget(GlobalPosition, aIManager.lastTarget, spaceState, AIManager.visibilityLayer, new Godot.Collections.Array() {this}), "path_to_last_pos");
            },
        }));

        behaviors.Add("dead", new NoBehaviour(aIManager, new Func<AIBehaviour.TransitionTestResult>[] { }));

        aIManager.Behaviours = behaviors;
        aIManager.SetCurrentBehaviour("idle");

        if (initPos != Vector2.Zero)
        {
            GlobalPosition = initPos;
        }

        if (hasTarget)
            aIManager.TryTransition();
    }

    public override Vector2 GetInputAxis(float delta)
    {
        return aIManager.Steer();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        aIManager.Process(delta);

        debugLabel.Text = aIManager.CurrentBehaviour;
    }

    public override void _ExitTree()
    {
        // Stop AIManager
        aIManager.StopTryTransitionLoop();
        aIManager.Dispose();
        tween.StopAll();
        base._ExitTree();
    }

    private Physics2DShapeQueryParameters GetDetectionShapeQuery()
    {
        shapeQuery.Transform = new Transform2D(0.0f, this.GlobalPosition);

        return shapeQuery;
    }

    public override void Die()
    {
        base.Die();

        if (deathParticles.Texture != null)
        {
            deathParticles.Emitting = true;
        }

        if (_animDeath.Empty())
        {
            charSprite.Visible = false;
            shadowSprite.Visible = false;
        }

        GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);

        aIManager.SetCurrentBehaviour("dead");

        EmitSignal(nameof(Died), this);
    }

    public void DetectionAlert()
    {
        tween.InterpolateProperty(detectionNotifier, "scale", new Vector2(0, 0), new Vector2(1.5f, 1.5f), 0.1f, Tween.TransitionType.Cubic);
        tween.InterpolateProperty(detectionNotifier, "modulate", Colors.White, new Color(1.0f, 0.537255f, 0.537255f), 0.1f);
        tween.InterpolateProperty(detectionNotifier, "scale", new Vector2(1.5f, 1.5f), new Vector2(1, 1), 0.1f, Tween.TransitionType.Cubic, delay: 0.1f);
        tween.InterpolateProperty(detectionNotifier, "modulate", new Color(1.0f, 0.537255f, 0.537255f), Colors.White, 0.1f, delay: 0.1f);
        tween.InterpolateProperty(detectionNotifier, "modulate", Colors.White, Colors.Transparent, 0.75f, delay: 1.35f);

        tween.Start();

        //play sound
    }

    public void TargetPlayer(Player player)
    {
        hasTarget = true;
        aIManager.lastTarget = player;

        aIManager.TryTransition();
    }

    Vector2 ICastsSpells.GetSpellDirection()
    {
        return GetSpellDirection();
    }

    protected virtual Vector2 GetSpellDirection()
    {
        return dir;
    }

    Vector2 ICastsSpells.GetSpellSpawnPos()
    {
        return GetSpellSpawnPos();
    }

    public virtual Vector2 GetSpellSpawnPos()
    {
        return GlobalPosition;
    }

    int ICastsSpells.GetSpellDamage(int baseDamge)
    {
        return GetSpellDamage(baseDamge);
    }

    public virtual int GetSpellDamage(int baseDamge)
    {
        return baseDamge;
    }

    float ICastsSpells.GetSpellRange(float baseRange)
    {
        return GetSpellRange(baseRange);
    }

    public virtual float GetSpellRange(float baseRange)
    {
        return baseRange;
    }

    float ICastsSpells.GetSpellKnockback(float baseKnockback)
    {
        return GetSpellKnockback(baseKnockback);
    }

    public virtual float GetSpellKnockback(float baseKnockback)
    {
        return baseKnockback;
    }

    float ICastsSpells.GetSpellSpeed(float baseSpeed)
    {
        return GetSpellSpeed(baseSpeed);
    }

    public virtual float GetSpellSpeed(float baseSpeed)
    {
        return baseSpeed;
    }

    Color ICastsSpells.GetSpellColour(Color baseColour)
    {
        return GetSpellColour(baseColour);
    }

    public virtual Color GetSpellColour(Color baseColour)
    {
        return baseColour;
    }

    void IIntersectsPlayerHitArea.PlayerHit(Player player)
    {
        PlayerHit(player);
    }

    public virtual void PlayerHit(Player player)
    {
        EmitSignal(nameof(PlayerIntersected), player);
    }
}
