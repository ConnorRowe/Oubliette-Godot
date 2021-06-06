using Godot;
using System;

public class BloodSplat : Node2D
{
    private AnimatedSprite droplet;
    private PackedScene bloodPoolScene;
    private BloodTexture bloodTexture;
    private Vector2 velocity = Vector2.Zero;
    private float dampening = 6.0f;

    public override void _Ready()
    {
        droplet = GetNode<AnimatedSprite>("Droplet");
        GetNode<AnimationPlayer>("AnimationPlayer").Connect("animation_finished", this, nameof(AnimationFinished));
        bloodPoolScene = GD.Load<PackedScene>("res://scenes/BloodPool.tscn");
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        velocity = velocity.LinearInterpolate(Vector2.Zero, dampening * delta);

        this.Position += velocity;
    }

    public void Init(BloodTexture bloodTexture, Vector2 velocity)
    {
        this.bloodTexture = bloodTexture;
        this.velocity = velocity;

        GetNode<AnimationPlayer>("AnimationPlayer").Play("Splat");
    }

    private void AnimationFinished(string animationName)
    {
        BloodPool bloodPool = bloodPoolScene.Instance<BloodPool>();
        GetParent().AddChild(bloodPool);
        bloodPool.Position = droplet.GlobalPosition;
        bloodPool.Start(bloodTexture);

        QueueFree();
    }
}
