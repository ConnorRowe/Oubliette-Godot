using Godot;
using System;

public class Projectile : KinematicBody2D
{
    private bool deactivated = false;
    public Vector2 velocity = new Vector2(100,0);
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

        KinematicCollision2D collide = MoveAndCollide(velocity*delta);
        if(collide != null)
        {
            Hit(collide.Collider as Node);
        }
    }

    private void Hit(Node node)
    {
        if(deactivated || node.Owner == source || node is Projectile)
            return;

        if(node.Owner is Character)
        {
            (node.Owner as Character).TakeDamage(source: source);
            (node.Owner as Character).ApplyKnockBack(velocity.Normalized() * 100.0f);
        }

        deactivated = true;

        velocity = Vector2.Zero;
        particles.OneShot = true;
        particles.Explosiveness = 1.0f;
        particles.ProcessMaterial = explodeParticleMaterial;
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
}
