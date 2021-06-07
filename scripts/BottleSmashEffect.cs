using Godot;
using System;

public class BottleSmashEffect : Node2D
{
    private Vector2 velocity = Vector2.Zero;
    private float rotationVelocity = 0.0f;
    private float dampening = 3.0f;
    private Sprite bottle;
    private AudioStreamRandomPitch randStream = new AudioStreamRandomPitch();
    private AudioStreamSample smashStream;
    private AudioStreamSample throwStream;
    private AudioStreamPlayer2D audioPlayer;

    public override void _Ready()
    {
        smashStream = GD.Load<AudioStreamSample>("res://sound/sfx/glass_smash_mixdown.wav");
        throwStream = GD.Load<AudioStreamSample>("res://sound/sfx/wind_gust.wav");

        randStream.AudioStream = throwStream;

        bottle = GetNode<Sprite>("Bottle");
        audioPlayer = GetNode<AudioStreamPlayer2D>("AudioPlayer");
        audioPlayer.Stream = randStream;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        velocity = velocity.LinearInterpolate(Vector2.Zero, dampening * delta);

        Position += velocity;


        if (!Mathf.IsEqualApprox(rotationVelocity, 0, 0.1f))
        {
            float dampening = rotationVelocity * 0.9f * delta;
            rotationVelocity -= dampening;
        }
        else
        {
            rotationVelocity = 0.0f;
        }

        bottle.RotationDegrees += (rotationVelocity * delta);

    }

    public void Start(Vector2 velocity, float rotationVelocity)
    {
        this.velocity = velocity;

        this.rotationVelocity = rotationVelocity * Math.Sign(velocity.x);

        AnimationPlayer animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        animPlayer.Connect("animation_finished", this, nameof(AnimationFinished));
        animPlayer.Play("Smash");
    }

    public void PlayThrowSound()
    {
        randStream.AudioStream = throwStream;
        audioPlayer.Play(0);
    }

    public void PlaySmashSound()
    {
        randStream.AudioStream = smashStream;
        audioPlayer.Play(0);
    }

    private void AnimationFinished(string animationName)
    {
        rotationVelocity = 0.0f;
        bottle.RotationDegrees = 0.0f;

        GetTree().CreateTimer(GetNode<Particles2D>("Bottle/SmashParticles").Lifetime).Connect("timeout", this, nameof(ParticlesFinished));
    }

    private void ParticlesFinished()
    {
        QueueFree();
    }
}
