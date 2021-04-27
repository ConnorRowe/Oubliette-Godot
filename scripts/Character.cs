using Godot;
using System.Drawing;

public enum Direction
{
    Up,
    Right,
    Down,
    Left
}
static class DirectionExt
{
    private static readonly Direction[] directions = new Direction[4] { Direction.Up, Direction.Right, Direction.Down, Direction.Left };
    public static Direction FromString(string str)
    {
        switch (str)
        {
            case "up":
                return Direction.Up;
            case "right":
                return Direction.Right;
            case "down":
                return Direction.Down;
            case "left":
                return Direction.Left;
        }

        return Direction.Down;
    }

    public static string AsString(this Direction d)
    {
        switch (d)
        {
            case Direction.Up:
                return "up";
            case Direction.Right:
                return "right";
            case Direction.Down:
                return "down";
            case Direction.Left:
                return "left";
        }

        return "error";
    }

    public static Vector2 AsVector(this Direction d)
    {
        switch (d)
        {
            case Direction.Up:
                return Vector2.Up;
            case Direction.Right:
                return Vector2.Right;
            case Direction.Down:
                return Vector2.Down;
            case Direction.Left:
                return Vector2.Left;
        }

        return Vector2.Zero;
    }

    public static Point AsPoint(this Direction d)
    {
        switch (d)
        {
            case Direction.Up:
                return new Point(0, -1);
            case Direction.Right:
                return new Point(1, 0);
            case Direction.Down:
                return new Point(0, 1);
            case Direction.Left:
                return new Point(-1, 0);
        }

        return Point.Empty;
    }

    public static Direction Opposite(this Direction d)
    {
        switch (d)
        {
            case Direction.Up:
                return Direction.Down;
            case Direction.Right:
                return Direction.Left;
            case Direction.Down:
                return Direction.Up;
            case Direction.Left:
                return Direction.Right;
        }

        return Direction.Down;
    }

    public static Direction FromVector(Vector2 v)
    {
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
        {
            if (v.x < 0)
            {
                return Direction.Left;

            }
            else
            {
                return Direction.Right;

            }
        }
        else if (Mathf.Abs(v.y) > 0)
        {
            if (v.y < 0)
            {
                return Direction.Up;
            }
            else
            {
                return Direction.Down;
            }
        }

        return Direction.Down;
    }

    public static Direction FromInt(int i)
    {
        switch (i)
        {
            case 0:
                return Direction.Down;
            case 1:
                return Direction.Left;
            case 2:
                return Direction.Up;
            case 3:
                return Direction.Right;
        }

        return Direction.Down;
    }

    public static Direction[] Directions()
    {
        return directions;
    }
}

public abstract class Character : KinematicBody2D
{
    public float maxSpeed = 64f;
    protected float speed = 0f;
    protected float acceleration = 5f;
    protected float friction = 8f;
    protected float elevation = 0f;
    protected float jumpVelocity = 0f;
    public int currentHealth = 0;
    public bool isDead = false;

    public virtual float GetMaxSpeed() { return maxSpeed; }

    // Directions must be normalised
    public Vector2 dir = new Vector2();
    public Vector2 inputDir = new Vector2();
    public Vector2 movementVelocity = new Vector2();
    public Vector2 externalVelocity = new Vector2();


    // Nodes
    protected AnimatedSprite charSprite;
    protected Sprite shadowSprite;
    protected AnimationPlayer animPlayer;
    private KinematicBody2D hitbox;
    private CollisionPolygon2D upDownCollider;
    private CollisionPolygon2D rightLeftCollider;

    // Export
    [Export]
    NodePath _hitboxPath;
    [Export]
    NodePath _upDownColliderPath;
    [Export]
    NodePath _rightLeftColliderPath;
    [Export]
    private NodePath _charSpritePath;
    [Export]
    private NodePath _shadowSpritePath;
    [Export]
    private NodePath _animationPlayerPath;
    [Export]
    public int maxHealth = 12;

    // Sprite animations
    [Export]
    protected string _animDown = "down";
    [Export]
    protected string _animLeftRight = "leftright";
    [Export]
    protected string _animUp = "up";
    [Export]
    protected string _animTakeDmg = "DamageFlash";
    [Export]
    protected string _animDeath = "death";

    // Signals
    [Signal]
    public delegate void HealthChanged(int currentHealth, int maxHealth);

    public abstract Vector2 GetInputAxis(float delta);

    public override void _Ready()
    {
        charSprite = GetNode<AnimatedSprite>(_charSpritePath);
        shadowSprite = GetNode<Sprite>(_shadowSpritePath);
        animPlayer = GetNode<AnimationPlayer>(_animationPlayerPath);
        hitbox = GetNode<KinematicBody2D>(_hitboxPath);
        upDownCollider = GetNode<CollisionPolygon2D>(_upDownColliderPath);
        rightLeftCollider = GetNode<CollisionPolygon2D>(_rightLeftColliderPath);

        currentHealth = maxHealth;

        EmitSignal(nameof(HealthChanged), currentHealth, maxHealth);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        // Get input direction
        inputDir = GetInputAxis(delta);

        // Remember last input direction (Lets character sprite remain facing last direction moved)
        if (inputDir.Length() > 0)
            dir = inputDir;

        // Render elevation
        charSprite.Position = new Vector2(0, -elevation);
        float shadowScale = Mathf.Lerp(1f, 0f, Mathf.Clamp(elevation / 32f, 0f, 0.9f));
        shadowSprite.Scale = new Vector2(shadowScale, shadowScale);

        if (isDead)
            return;

        //Set idle or moving
        if (IsMoving())
        {
            charSprite.Playing = true;
        }
        else
        {
            charSprite.Playing = false;
            charSprite.Frame = 0;
        }

        // Update char sprite direction
        UpdateSpriteDirection();

        // Update active hitbox collider
        upDownCollider.Disabled = true;
        rightLeftCollider.Disabled = true;

        switch (GetFacingDirection())
        {
            case Direction.Up:
                {
                    upDownCollider.Disabled = false;
                    break;
                }
            case Direction.Right:
                {
                    rightLeftCollider.Disabled = false;
                    rightLeftCollider.Scale = new Vector2(1, 1);
                    break;
                }
            case Direction.Down:
                {
                    upDownCollider.Disabled = false;
                    break;
                }
            case Direction.Left:
                {
                    rightLeftCollider.Disabled = false;
                    rightLeftCollider.Scale = new Vector2(-1, 1);
                    break;
                }
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        if (inputDir.Length() > 0)
        {
            if (speed < GetMaxSpeed())
            {
                speed += acceleration;

            }
            if (speed > GetMaxSpeed())
            {
                speed = GetMaxSpeed();
            }

            // Cant move as fast in the air
            movementVelocity += dir * speed * (IsAirborne() ? 0.025f : 1f);
        }
        else if (!IsAirborne()) // Friction only applied if not airborne
        {
            if (speed > 0)
            {
                speed -= friction;
            }
            if (speed < 0)
            {
                speed = 0;
            }

            movementVelocity = movementVelocity.Normalized() * speed;
        }

        if (movementVelocity.Length() > GetMaxSpeed())
        {
            movementVelocity = movementVelocity.Normalized() * GetMaxSpeed();
        }

        Vector2 finalVelocity = movementVelocity + externalVelocity;

        // Apply movement velocity
        MoveAndSlide(finalVelocity * delta * 100.0f);

        // Dampen external velocity over time
        externalVelocity -= externalVelocity.Normalized() * externalVelocity.Length() * 0.9f * (delta * 8.0f);


        // Apply jump stuff
        if (Mathf.Abs(jumpVelocity) > 0)
        {
            elevation += jumpVelocity * delta;

            jumpVelocity -= 9.8f;

            if (elevation < 0)
                elevation = 0;
        }

    }

    public bool IsAirborne()
    {
        return this.elevation > 0;
    }

    public bool IsMoving()
    {
        return movementVelocity.Length() > 0;
    }

    protected Direction GetDirection(Vector2 v)
    {
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
        {
            if (v.x < 0)
            {
                return Direction.Left;

            }
            else
            {
                return Direction.Right;

            }
        }
        else if (Mathf.Abs(v.y) > 0)
        {
            if (v.y < 0)
            {
                return Direction.Up;
            }
            else
            {
                return Direction.Down;
            }
        }

        return Direction.Down;
    }

    public virtual Direction GetFacingDirection()
    {
        return GetDirection(dir);
    }

    // Tuple(animation name, FlipH, freeze frame if >= 0)
    public virtual (string, bool, int) GetSpriteAnimation()
    {
        if (isDead)
            return (_animDeath, false, -1);

        (string, bool, int) anim = ("", false, -1);

        switch (GetFacingDirection())
        {
            case Direction.Up:
                {
                    anim.Item1 = _animUp;
                    anim.Item2 = false;
                    break;
                }
            case Direction.Right:
                {
                    anim.Item1 = _animLeftRight;
                    anim.Item2 = false;
                    break;
                }
            case Direction.Down:
                {
                    anim.Item1 = _animDown;
                    anim.Item2 = false;
                    break;
                }
            case Direction.Left:
                {
                    anim.Item1 = _animLeftRight;
                    anim.Item2 = true;
                    break;
                }
        }

        if (movementVelocity.Length() <= 0.001f)
        {
            anim.Item3 = 0;
        }

        return anim;
    }

    public void UpdateSpriteDirection()
    {
        var anim = GetSpriteAnimation();

        charSprite.Animation = anim.Item1;
        charSprite.FlipH = anim.Item2;

        if (anim.Item3 >= 0)
        {
            charSprite.Frame = anim.Item3;
            charSprite.Playing = false;
        }
        else
            charSprite.Playing = true;
    }

    public virtual void TakeDamage(int damage = 1, Character source = null)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        animPlayer.CurrentAnimation = _animTakeDmg;

        if (animPlayer.IsPlaying())
        {
            animPlayer.Seek(0.0f);
        }
        else
        {
            animPlayer.Play();
        }

        if (damage > 0) { EmitSignal(nameof(HealthChanged), currentHealth, maxHealth); }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        if (!_animDeath.Empty())
            charSprite.Play(_animDeath);

        hitbox.Layers = 0;

        isDead = true;
    }

    public void ApplyKnockBack(Vector2 vel)
    {
        externalVelocity += vel;
    }
}
