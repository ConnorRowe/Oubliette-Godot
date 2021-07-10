using Godot;
using System.Drawing;
using System.Collections.Generic;
using Oubliette.Stats;

namespace Oubliette
{
    public enum Direction : byte
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
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
        private Dictionary<Stat, float> baseStats = Stats.Buffs.DefaultStats();
        public Dictionary<Stat, float> currentStats = Stats.Buffs.DefaultStats();

        public float MaxSpeed { get; set; } = 64f;
        protected float speed = 0f;
        protected float acceleration = 5f;
        protected float friction = 8f;
        protected float elevation = 0f;
        protected float jumpVelocity = 0f;
        public float CurrentHealth { get; set; } = 0f;
        public bool IsDead { get; set; } = false;
        public bool CheckSlideCollisions { get; set; } = false;
        private float CheckSlideMaxCD = 0.15f;
        private float checkSlideCD = 0.0f;
        private SceneTreeTimer spillageDmgTimer;
        private int spillageCount = 0;

        public float Elevation { get { return elevation; } }

        public HashSet<Buff> TimedBuffs { get; set; } = new HashSet<Buff>();
        protected string lastDamagedBy = "";

        public AnimatedSprite CharSprite { get { return charSprite; } }

        public virtual float GetMaxSpeed()
        {
            return MaxSpeed * currentStats[Stat.MoveSpeedMultiplier];
        }

        public virtual float GetAcceleration() { return acceleration; }

        // Directions must be normalised
        public Vector2 Dir { get; set; } = new Vector2();
        public Vector2 InputDir { get; set; } = new Vector2();
        public Vector2 MovementVelocity { get; set; } = new Vector2();
        public Vector2 ExternalVelocity { get; set; } = new Vector2();


        // Nodes
        protected AnimatedSprite charSprite;
        protected Sprite shadowSprite;
        protected AnimationPlayer animPlayer;
        protected KinematicBody2D hitbox;
        private CollisionPolygon2D upDownCollider;
        private CollisionPolygon2D rightLeftCollider;
        protected Area2D feetArea;
        private SpillageHazard lastSpillage;

        // Export
        [Export]
        public string DamageSourceName { get; set; } = "default";
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
        private NodePath _feetAreaPath;
        [Export]
        public float MaxHealth { get; set; } = 12f;
        [Export]
        private bool renderElevation = false;
        [Export]
        private bool tracksSpillages = true;
        [Export]
        public bool IsOnEnemyTeam { get; set; } = true;

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
        public delegate void HealthChanged(float currentHealth, float maxHealth);
        [Signal]
        public delegate void SlideCollision(KinematicCollision2D collision);

        public abstract Vector2 GetInputAxis(float delta);

        public override void _Ready()
        {
            charSprite = GetNode<AnimatedSprite>(_charSpritePath);
            shadowSprite = GetNode<Sprite>(_shadowSpritePath);
            animPlayer = GetNode<AnimationPlayer>(_animationPlayerPath);
            hitbox = GetNode<KinematicBody2D>(_hitboxPath);
            upDownCollider = GetNode<CollisionPolygon2D>(_upDownColliderPath);
            rightLeftCollider = GetNode<CollisionPolygon2D>(_rightLeftColliderPath);
            feetArea = GetNode<Area2D>(_feetAreaPath);
            feetArea.Connect("area_entered", this, nameof(OnFeetAreaEntered));
            feetArea.Connect("area_exited", this, nameof(OnFeetAreaExited));


            CurrentHealth = MaxHealth;

            EmitSignal(nameof(HealthChanged), CurrentHealth, MaxHealth);
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            // Get input direction
            InputDir = GetInputAxis(delta);

            // Remember last input direction (Lets character sprite remain facing last direction moved)
            if (InputDir.Length() > 0)
                Dir = InputDir;

            // Render elevation
            if (renderElevation)
            {
                charSprite.Position = new Vector2(0, -elevation);
                float shadowScale = Mathf.Lerp(1f, 0f, Mathf.Clamp(elevation / 32f, 0f, 0.9f));
                shadowSprite.Scale = new Vector2(shadowScale, shadowScale);
            }

            if (IsDead)
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
            bool isUpDownColliderDisabled = true;
            bool isRightLeftColliderDisabled = true;

            switch (GetFacingDirection())
            {
                case Direction.Up:
                    {
                        isUpDownColliderDisabled = false;
                        break;
                    }
                case Direction.Right:
                    {
                        isRightLeftColliderDisabled = false;
                        rightLeftCollider.Scale = new Vector2(1, 1);
                        break;
                    }
                case Direction.Down:
                    {
                        isUpDownColliderDisabled = false;
                        break;
                    }
                case Direction.Left:
                    {
                        isRightLeftColliderDisabled = false;
                        rightLeftCollider.Scale = new Vector2(-1, 1);
                        break;
                    }
            }

            upDownCollider.SetDeferred("disabled", isUpDownColliderDisabled);
            rightLeftCollider.SetDeferred("disabled", isRightLeftColliderDisabled);

            // Remove expired buffs
            uint time = OS.GetTicksMsec();
            foreach (Buff buff in TimedBuffs)
            {
                if (buff.duration > 0 && buff.startTime + buff.duration < time)
                {
                    buff.notifyExpired?.BuffExpired();
                }
            }
            int removedBuffCount = TimedBuffs.RemoveWhere((Buff buff) => { return buff.duration > 0 && buff.startTime + buff.duration < time; });
            if (removedBuffCount > 0)
            {
                RecalcStats();
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            if (InputDir.Length() > 0)
            {
                if (speed < GetMaxSpeed())
                {
                    speed += GetAcceleration();

                }
                if (speed > GetMaxSpeed())
                {
                    speed = GetMaxSpeed();
                }

                // Cant move as fast in the air
                MovementVelocity += Dir * speed * (IsAirborne() ? 0.025f : 1f);
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

                MovementVelocity = MovementVelocity.Normalized() * speed;
            }

            if (MovementVelocity.Length() > GetMaxSpeed())
            {
                MovementVelocity = MovementVelocity.Normalized() * GetMaxSpeed();
            }

            Vector2 finalVelocity = MovementVelocity + ExternalVelocity;

            // Apply movement velocity
            Vector2 slideVelocity = MoveAndSlide(finalVelocity * delta * 100.0f, infiniteInertia: false);

            // Report slide collision
            if (CheckSlideCollisions && checkSlideCD <= 0.0f)
            {
                checkSlideCD = CheckSlideMaxCD;

                int slideCount = GetSlideCount();

                for (int i = 0; i < slideCount; i++)
                    OnSlideCollision(GetSlideCollision(i), slideVelocity);
            }

            if (checkSlideCD > 0.0f)
            {
                checkSlideCD -= delta;
            }

            // Dampen external velocity over time
            ExternalVelocity -= ExternalVelocity.Normalized() * ExternalVelocity.Length() * 0.9f * (delta * 8.0f);


            // Apply jump stuff
            if (Mathf.Abs(jumpVelocity) > 0)
            {
                elevation += jumpVelocity * delta;

                jumpVelocity -= 9.8f;

                if (elevation < 0)
                    elevation = 0;
            }
        }

        public virtual void OnSlideCollision(KinematicCollision2D kinematicCollision, Vector2 slideVelocity)
        {
            if (kinematicCollision.Collider is RigidBody2D rigidBody)
            {
                rigidBody.ApplyCentralImpulse(-kinematicCollision.Normal * GetMaxSpeed());
            }

            EmitSignal(nameof(SlideCollision), kinematicCollision);
        }

        public virtual void OnFeetAreaEntered(Area2D area)
        {
            if (tracksSpillages && area is SpillageHazard spillage && spillage.EnemyOwned != IsOnEnemyTeam)
            {
                spillageCount++;

                if (spillageCount == 1)
                {
                    if (spillageDmgTimer != null && spillageDmgTimer.TimeLeft > 0.0f)
                        return;

                    lastSpillage = spillage;
                    SpillageDamage();
                }

            }
        }

        public virtual void OnFeetAreaExited(Area2D area)
        {
            if (tracksSpillages && area is SpillageHazard spillage && spillage.EnemyOwned != IsOnEnemyTeam)
            {
                spillageCount--;
            }
        }

        private void SpillageDamage()
        {
            if (spillageCount > 0)
            {
                if (!lastSpillage.Active)
                {
                    spillageCount--;

                    if (spillageCount <= 0)
                        return;
                }

                TakeDamage(sourceName: lastSpillage.DmgSourceName);

                spillageDmgTimer = GetTree().CreateTimer(1.0f, false);
                spillageDmgTimer.Connect("timeout", this, nameof(SpillageDamage));
            }
        }

        public bool IsAirborne()
        {
            return this.elevation > 0;
        }

        public bool IsMoving()
        {
            return MovementVelocity.Length() > 0;
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
            return GetDirection(Dir);
        }

        // Tuple(animation name, FlipH, freeze frame if >= 0)
        public virtual (string name, bool flipH, int freezeFrame) GetSpriteAnimation()
        {
            if (IsDead)
                return (_animDeath, false, -1);

            (string name, bool flipH, int freezeFrame) anim = ("", false, -1);

            switch (GetFacingDirection())
            {
                case Direction.Up:
                    {
                        anim.name = _animUp;
                        anim.flipH = false;
                        break;
                    }
                case Direction.Right:
                    {
                        anim.name = _animLeftRight;
                        anim.flipH = false;
                        break;
                    }
                case Direction.Down:
                    {
                        anim.name = _animDown;
                        anim.flipH = false;
                        break;
                    }
                case Direction.Left:
                    {
                        anim.name = _animLeftRight;
                        anim.flipH = true;
                        break;
                    }
            }

            if (MovementVelocity.Length() <= 0.001f)
            {
                anim.freezeFrame = 0;
            }

            return anim;
        }

        public void UpdateSpriteDirection()
        {
            var anim = GetSpriteAnimation();

            charSprite.Animation = anim.name;
            charSprite.FlipH = anim.flipH;

            if (anim.Item3 >= 0)
            {
                charSprite.Frame = anim.Item3;
                charSprite.Playing = false;
            }
            else
                charSprite.Playing = true;
        }

        public virtual void TakeDamage(float damage = 1f, Character source = null, string sourceName = "")
        {
            if (IsDead)
                return;

            lastDamagedBy = sourceName;

            // Resist damage
            damage = damage - Mathf.RoundToInt(currentStats[Stat.ResistDamageFlat]);

            // Reflect damage
            if (source != null && source != this && currentStats[Stat.ReflectDamageFlat] > 0.0f)
                source.TakeDamage(currentStats[Stat.ReflectDamageFlat], null);

            CurrentHealth -= damage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

            animPlayer.CurrentAnimation = _animTakeDmg;

            if (animPlayer.IsPlaying())
            {
                animPlayer.Seek(0.0f);
            }
            else
            {
                animPlayer.Play();
            }

            if (damage > 0) { EmitSignal(nameof(HealthChanged), CurrentHealth, MaxHealth); }

            if (CurrentHealth <= 0f)
            {
                Die();
            }
        }

        public virtual void Heal(float healing)
        {
            CurrentHealth += healing;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

            EmitSignal(nameof(HealthChanged), CurrentHealth, MaxHealth);
        }

        public void AdjustMaxHealth(float adjustment, bool affectCurrentHealth)
        {
            MaxHealth += adjustment;

            if (affectCurrentHealth)
                CurrentHealth += adjustment;

            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

            EmitSignal(nameof(HealthChanged), CurrentHealth, MaxHealth);
        }

        public virtual void Die()
        {
            if (!_animDeath.Empty())
                charSprite.Play(_animDeath);

            hitbox.Layers = 0;

            IsDead = true;
        }

        public virtual void ApplyKnockBack(Vector2 vel)
        {
            ExternalVelocity += vel;
        }

        public void ApplyTimedBuff(Buff buff)
        {
            bool containsBuff = false;
            foreach (Buff b in TimedBuffs)
            {
                if (b.name == buff.name)
                {
                    containsBuff = true;
                    break;
                }
            }

            if (containsBuff)
            {
                TimedBuffs.RemoveWhere((b) => { return b.name == buff.name; });
            }

            TimedBuffs.Add(buff);

            RecalcStats();
        }

        public void ApplyTimedBuffs(Buff[] buffs)
        {
            foreach (Buff buff in buffs)
            {
                ApplyTimedBuff(buff);
            }

            RecalcStats();
        }

        public virtual void RecalcStats()
        {
            currentStats = new Dictionary<Stat, float>(baseStats);
            foreach (Buff buff in TimedBuffs)
            {
                foreach ((Stat stat, float amount) in buff.stats)
                {
                    currentStats[stat] = currentStats[stat] + amount;
                }
            }
        }

        public float GetStatValue(Stat stat)
        {
            return currentStats[stat];
        }
    }
}