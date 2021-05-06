using Godot;
using System;

public class Projectile : KinematicBody2D
{
    private int damage = 0;
    private float range = 0;
    private float knockback = 0;
    private float speed = 0;

    private float rangeCounter = 0;
    private bool deactivated = false;
    public Vector2 direction = new Vector2(0, 0);
    public Character source = null;

    public Light2D light;
    public Particles2D particles;
    private Tween tween;

    [Export]
    private ParticlesMaterial explodeParticleMaterial;

    public override void _Ready()
    {
        base._Ready();

        light = GetNode<Light2D>("Light2D");
        particles = GetNode<Particles2D>("Particles2D");
        tween = GetNode<Tween>("Tween");
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

        if (node.Owner is Character)
        {
            (node.Owner as Character).TakeDamage(damage, source);
            (node.Owner as Character).ApplyKnockBack(direction * knockback);
        }

        Explode();
    }

    private void Explode()
    {
        deactivated = true;
        SetPhysicsProcess(false);

        float hueCache = ((ParticlesMaterial)particles.ProcessMaterial).HueVariation;

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

    public void SetSpellStats(int damage, float range, float knockback, float speed)
    {
        this.damage = damage;
        this.range = range;
        this.knockback = knockback;
        this.speed = speed;
    }
}
