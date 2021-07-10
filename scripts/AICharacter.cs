using Godot;
using System;

namespace Oubliette.AI
{
    public class AICharacter : Character, Spells.ICastsSpells, IIntersectsPlayerHitArea
    {
        public AIManager AIManager { get; set; }
        protected Particles2D deathParticles;
        private CircleShape2D detectionShape = new CircleShape2D();
        protected Label debugLabel;
        protected Tween tween;
        protected Sprite detectionNotifier;
        public bool HasTarget { get; set; } = false;
        private PackedScene bloodSplatScene;

        public Physics2DShapeQueryParameters ShapeQuery { get; set; }
        public World World { get; set; }
        public float OgMaxSpeed { get; set; }
        public IProvidesNav NavProvider { get; set; }
        public Vector2 InitPos { get; set; } = Vector2.Zero;

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
        [Export]
        private Color bloodColour = new Color(0.760784f, 0, 0.101961f);
        [Export]
        private Vector2 minMaxBloodSplats = Vector2.Zero;
        [Export]
        private bool spawnBloodPoolOnDeath = false;

        // Signals
        [Signal]
        public delegate void Died(AICharacter aICharacter);
        [Signal]
        public delegate void PlayerIntersected(Player player);


        public override void _Ready()
        {
            base._Ready();

            this.MaxSpeed = maxMovementSpeed;
            OgMaxSpeed = this.MaxSpeed;

            if (GetParent() is World)
                World = GetParent<World>();
            else if (GetParent().GetParent() is World)
                World = GetParent().GetParent<World>();

            debugLabel = GetNode<Label>("debugLabel");
            deathParticles = GetNode<Particles2D>(_deathParticlesPath);
            tween = GetNode<Tween>(_tweenPath);
            detectionNotifier = GetNode<Sprite>(_detectionNotifierPath);
            detectionShape.Radius = detectionRadius;

            charSprite.Connect("animation_finished", this, nameof(CharAnimationFinished));

            ShapeQuery = new Physics2DShapeQueryParameters()
            {
                CollisionLayer = 512,
                Transform = new Transform2D(0.0f, this.GlobalPosition),
                ShapeRid = detectionShape.GetRid(),
                Exclude = new Godot.Collections.Array { this },
                CollideWithAreas = true,
                CollideWithBodies = true,
            };

            ShapeQuery.SetShape(detectionShape);

            AIManager = new AIManager(this, World);

            Godot.Collections.Dictionary<string, AIBehaviour> behaviors = new Godot.Collections.Dictionary<string, AIBehaviour>();

            behaviors.Add("idle", new NoBehaviour(AIManager, new Func<AIBehaviour.TransitionTestResult>[] {
            // idle -> attack_target
            () => {
                // If has target, target is within range, target is visible and no tiles block path
                Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
                return new AIBehaviour.TransitionTestResult(HasTarget && GlobalPosition.DistanceTo(AIManager.LastTarget.GlobalPosition) <= attackRange && AIManager.TraceToTarget(GlobalPosition, AIManager.LastTarget, spaceState, AIManager.VisibilityLayer, new Godot.Collections.Array() {this}) && AIManager.TraceToTarget(GlobalPosition, AIManager.LastTarget, spaceState, AIManager.TilesLayer, new Godot.Collections.Array() {this}), "attack_target");
            },
            // idle -> follow_target
            () => {
                return new AIBehaviour.TransitionTestResult(HasTarget, "follow_target");
            }
        }));

            behaviors.Add("follow_target", new FollowTargetBehaviour(AIManager, new Func<AIBehaviour.TransitionTestResult>[] {
            // follow_target -> attack_target
            () => {
                // If target is within range, target is visible and no tiles block path
                Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
                return new AIBehaviour.TransitionTestResult(GlobalPosition.DistanceTo(AIManager.LastTarget.GlobalPosition) <= attackRange && AIManager.TraceToTarget(GlobalPosition, AIManager.LastTarget, spaceState, AIManager.VisibilityLayer, new Godot.Collections.Array() {this}) && AIManager.TraceToTarget(GlobalPosition, AIManager.LastTarget, spaceState, AIManager.TilesLayer, new Godot.Collections.Array() {this}), "attack_target");
            },
            // follow_target -> path_to_last_pos
            () => {
                // If target is not visible
                Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
                return new AIBehaviour.TransitionTestResult(!AIManager.TraceToTarget(GlobalPosition, AIManager.LastTarget, spaceState, AIManager.VisibilityLayer, new Godot.Collections.Array() {this}), "path_to_last_pos");
            },
        }));

            behaviors.Add("path_to_last_pos", new FollowPathBehaviour(AIManager, () =>
            {
                return AIManager.GetNavPathGlobal(GlobalPosition, AIManager.TargetPosCache, NavProvider.GetNavigation());
            }, new Func<AIBehaviour.TransitionTestResult>[] {
            // path_to_last_pos -> attack_target
            () => {
                // If target is within range, target is visible and no tiles block path
                Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
                return new AIBehaviour.TransitionTestResult(GlobalPosition.DistanceTo(AIManager.LastTarget.GlobalPosition) <= attackRange && AIManager.TraceToTarget(GlobalPosition, AIManager.LastTarget, spaceState, AIManager.VisibilityLayer, new Godot.Collections.Array() {this}) && AIManager.TraceToTarget(GlobalPosition, AIManager.LastTarget, spaceState, AIManager.TilesLayer, new Godot.Collections.Array() {this}), "attack_target");
            },
            // path_to_last_pos -> follow_target
            () => {
                // If player is visible and no tiles block path
                Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
                return new AIBehaviour.TransitionTestResult(AIManager.TraceToTarget(GlobalPosition, AIManager.LastTarget, spaceState, AIManager.VisibilityLayer, new Godot.Collections.Array() {this}) && AIManager.TraceToTarget(GlobalPosition, AIManager.LastTarget, spaceState, AIManager.TilesLayer, new Godot.Collections.Array() {this}), "follow_target");
            },
            }));

            behaviors.Add("attack_target", new MeleeAttackBehaviour(AIManager, attackSpeed, attackRange, new Func<AIBehaviour.TransitionTestResult>[] {
            // attack_target -> follow_target
            () => {
                return new AIBehaviour.TransitionTestResult(GlobalPosition.DistanceTo(AIManager.LastTarget.GlobalPosition) > attackRange, "follow_target");
            },
            // attack_target -> path_to_last_pos
            () => {
                // If target is not visible
                Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
                return new AIBehaviour.TransitionTestResult(!AIManager.TraceToTarget(GlobalPosition, AIManager.LastTarget, spaceState, AIManager.VisibilityLayer, new Godot.Collections.Array() {this}), "path_to_last_pos");
            },
        }));

            behaviors.Add("dead", new NoBehaviour(AIManager, new Func<AIBehaviour.TransitionTestResult>[] { }));

            AIManager.Behaviours = behaviors;
            AIManager.SetCurrentBehaviour("idle");

            if (InitPos != Vector2.Zero)
            {
                GlobalPosition = InitPos;
            }

            if (HasTarget)
                AIManager.TryTransition();
        }

        public override Vector2 GetInputAxis(float delta)
        {
            return AIManager.Steer();
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            AIManager.Process(delta);
        }

        public override void _ExitTree()
        {
            // Stop AIManager
            AIManager.StopTryTransitionLoop();
            AIManager.Dispose();
            tween.StopAll();
            base._ExitTree();
        }

        private Physics2DShapeQueryParameters GetDetectionShapeQuery()
        {
            ShapeQuery.Transform = new Transform2D(0.0f, this.GlobalPosition);

            return ShapeQuery;
        }

        protected virtual void CharAnimationFinished()
        {
            if (spawnBloodPoolOnDeath && charSprite.Animation == _animDeath)
            {
                World.SpawnBloodPool(GlobalPosition, bloodColour);
            }
        }

        public override void Die()
        {
            base.Die();

            int bloodSplats = World.rng.RandiRange(Mathf.RoundToInt(minMaxBloodSplats.x), Mathf.RoundToInt(minMaxBloodSplats.y));
            for (int i = 0; i < bloodSplats; ++i)
            {
                World.SpawnBloodSplat(GlobalPosition, bloodColour);
            }

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

            AIManager.SetCurrentBehaviour("dead");

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
            HasTarget = true;
            AIManager.LastTarget = player;

            AIManager.TryTransition();
        }

        Vector2 Spells.ICastsSpells.GetSpellDirection()
        {
            return GetSpellDirection();
        }

        protected virtual Vector2 GetSpellDirection()
        {
            return Dir;
        }

        Vector2 Spells.ICastsSpells.GetSpellSpawnPos()
        {
            return GetSpellSpawnPos();
        }

        public virtual Vector2 GetSpellSpawnPos()
        {
            return GlobalPosition;
        }

        float Spells.ICastsSpells.GetSpellDamage(float baseDamge)
        {
            return GetSpellDamage(baseDamge);
        }

        public virtual float GetSpellDamage(float baseDamge)
        {
            return baseDamge;
        }

        float Spells.ICastsSpells.GetSpellRange(float baseRange)
        {
            return GetSpellRange(baseRange);
        }

        public virtual float GetSpellRange(float baseRange)
        {
            return baseRange;
        }

        float Spells.ICastsSpells.GetSpellKnockback(float baseKnockback)
        {
            return GetSpellKnockback(baseKnockback);
        }

        public virtual float GetSpellKnockback(float baseKnockback)
        {
            return baseKnockback;
        }

        float Spells.ICastsSpells.GetSpellSpeed(float baseSpeed)
        {
            return GetSpellSpeed(baseSpeed);
        }

        public virtual float GetSpellSpeed(float baseSpeed)
        {
            return baseSpeed;
        }

        Color Spells.ICastsSpells.GetSpellColour(Color baseColour)
        {
            return GetSpellColour(baseColour);
        }

        public virtual Color GetSpellColour(Color baseColour)
        {
            return baseColour;
        }

        NodePath Spells.ICastsSpells.AddSpellEffectNode(Node2D node)
        {
            return AddSpellEffectsNode(node);
        }

        public virtual NodePath AddSpellEffectsNode(Node2D node)
        {
            charSprite.AddChild(node);

            return node.GetPath();
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
}