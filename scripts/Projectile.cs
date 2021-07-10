using Godot;
using System;

namespace Oubliette
{
    public class Projectile : KinematicBody2D
    {
        private float damage = 0;
        private float range = 0;
        private float knockback = 0;
        private float speed = 0;
        private Color spellColour = Colors.Transparent;
        private float rangeCounter = 0;
        private bool deactivated = false;
        private float projCurveCount = 0.0f;

        private Vector2 direction = new Vector2(0, 0);
        private float directionRadians = 0.0f;

        public Character Source { get; set; } = null;
        public Light2D Light { get; set; }
        public Particles2D Particles { get; set; }
        public Curve CurveX { get; set; }
        public Curve CurveY { get; set; }
        public float CurveInterpSpeed { get; set; }
        public float CurveMoveSpeed { get; set; }
        public Vector2 Gravity { get; set; } = Vector2.Zero;
        public float Speed { get { return speed; } set { speed = value; } }

        private Tween tween;
        private AudioStreamPlayer2D explodePlayer;
        private AudioStreamRandomPitch explodeSound = new AudioStreamRandomPitch();
        private Action<Character> hitCharEvent = null;

        [Export]
        private ParticlesMaterial explodeParticleMaterial;
        [Export]
        public string dmgSourceName = "projectile";
        [Export]
        private AudioStreamSample baseExplodeSound;

        public override void _Ready()
        {
            base._Ready();

            Light = GetNode<Light2D>("Light2D");
            Particles = GetNodeOrNull<Particles2D>("Particles2D");
            tween = GetNode<Tween>("Tween");
            explodePlayer = GetNode<AudioStreamPlayer2D>("ExplodePlayer");

            if (baseExplodeSound != null)
            {
                explodeSound.AudioStream = baseExplodeSound;
                explodeSound.RandomPitch = 1.2f;
                explodePlayer.Stream = explodeSound;
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            Vector2 curveAdjust = Vector2.Zero;

            if (CurveInterpSpeed > 0.0f)
            {
                if (CurveX != null)
                    curveAdjust.x = CurveX.InterpolateBaked(projCurveCount);
                if (CurveY != null)
                    curveAdjust.y = CurveY.InterpolateBaked(projCurveCount);

                curveAdjust = curveAdjust.Rotated(directionRadians);
                curveAdjust *= CurveMoveSpeed;

                projCurveCount += (delta * CurveInterpSpeed);

                if (projCurveCount >= 1.0f)
                    projCurveCount -= 1.0f;
            }

            Vector2 gravity = Gravity * Mathf.Abs(direction.x);

            Vector2 move = ((direction * speed) + curveAdjust + gravity) * delta;

            KinematicCollision2D collide = MoveAndCollide(move);

            rangeCounter += move.Length();

            if (collide != null)
            {
                Hit(collide.Collider as Node);
            }
            else if (rangeCounter >= range)
            {
                Explode();
            }
        }

        private void Hit(Node node)
        {
            if (deactivated || node.Owner == Source || node is Projectile)
                return;

            if (node.Owner is Character character)
            {
                character.TakeDamage(damage, Source, dmgSourceName);
                character.ApplyKnockBack(direction * knockback);
                hitCharEvent?.Invoke(character);
            }

            Explode();
        }

        private void Explode()
        {
            if (baseExplodeSound != null)
            {
                explodePlayer.Play();
            }

            deactivated = true;
            SetPhysicsProcess(false);

            float hueCache = 0.0f;
            if (Particles.ProcessMaterial != null && Particles.ProcessMaterial is ParticlesMaterial pMat)
                hueCache = pMat.HueVariation;

            direction = Vector2.Zero;
            Particles.OneShot = true;
            Particles.Explosiveness = 1.0f;
            Particles.ProcessMaterial = explodeParticleMaterial;
            ((ParticlesMaterial)Particles.ProcessMaterial).HueVariation = hueCache;
            Particles.Restart();

            tween.InterpolateProperty(Light, "energy", Light.Energy, 0.0f, 0.5f, Tween.TransitionType.Quad);
            tween.InterpolateCallback(this, 0.5f, nameof(Die));
            tween.Start();

            GetTree().CreateTimer(0.5f).Connect("timeout", this, nameof(Die));
        }

        private void Die()
        {
            QueueFree();
        }

        public void SetSpellStats(float damage, float range, float knockback, float speed, string baseDmgSourceName)
        {
            this.damage = damage;
            this.range = range;
            this.knockback = knockback;
            this.speed = speed;
            this.dmgSourceName = baseDmgSourceName;
        }

        public void SetHitCharEvent(Action<Character> hitCharEvent)
        {
            this.hitCharEvent = hitCharEvent;
        }

        public void SetProjectileColour(Color newColour)
        {
            spellColour = newColour;

            if (Particles.ProcessMaterial != null)
            {
                GradientTexture particleColourRamp = (GradientTexture)((ParticlesMaterial)Particles.ProcessMaterial).ColorRamp;
                particleColourRamp.Gradient.SetColor(0, spellColour);
                explodeParticleMaterial.ColorRamp = particleColourRamp;
            }
        }

        public void SetDirection(Vector2 direction)
        {
            this.direction = direction;
            directionRadians = Mathf.Atan2(direction.x, direction.y);
        }
    }
}