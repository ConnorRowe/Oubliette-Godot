using Godot;
using System;

namespace Oubliette
{
    public class Projectile : KinematicBody2D
    {
        private int damage = 0;
        private float range = 0;
        private float knockback = 0;
        private float speed = 0;
        private Color spellColour = Colors.Transparent;

        private float rangeCounter = 0;
        private bool deactivated = false;
        public Vector2 direction = new Vector2(0, 0);
        public Character source = null;

        public Light2D light;
        public Particles2D particles;
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

            light = GetNode<Light2D>("Light2D");
            particles = GetNodeOrNull<Particles2D>("Particles2D");
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

            Vector2 move = direction * speed * delta;

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
            if (deactivated || node.Owner == source || node is Projectile)
                return;

            if (node.Owner is Character character)
            {
                character.TakeDamage(damage, source, dmgSourceName);
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
            if (particles.ProcessMaterial != null && particles.ProcessMaterial is ParticlesMaterial pMat)
                hueCache = pMat.HueVariation;

            direction = Vector2.Zero;
            particles.OneShot = true;
            particles.Explosiveness = 1.0f;
            particles.ProcessMaterial = explodeParticleMaterial;
            ((ParticlesMaterial)particles.ProcessMaterial).HueVariation = hueCache;
            particles.Restart();

            tween.InterpolateProperty(light, "energy", light.Energy, 0.0f, 0.5f, Tween.TransitionType.Quad);
            tween.InterpolateCallback(this, 0.5f, nameof(Die));
            tween.Start();

            GetTree().CreateTimer(0.5f).Connect("timeout", this, nameof(Die));
        }

        private void Die()
        {
            QueueFree();
        }

        public void SetSpellStats(int damage, float range, float knockback, float speed, string baseDmgSourceName)
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

            if (particles.ProcessMaterial != null)
            {
                GradientTexture particleColourRamp = (GradientTexture)((ParticlesMaterial)particles.ProcessMaterial).ColorRamp;
                particleColourRamp.Gradient.SetColor(0, spellColour);
                explodeParticleMaterial.ColorRamp = particleColourRamp;
            }
        }
    }
}