using Godot;
using System;

public class PlayerGib : RigidBody2D
{
    private BloodTexture bloodTexture;
    private PackedScene bloodPoolScene;
    private Vector2 initialImpulse;
    private Tween tween;
    private Sprite sprite;
    public float Elevation = 0.0f;
    private float bloodAmount = 5.0f;
    private Vector2 lastPosition;
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    private float fakeRotationVelocity = 0.0f;
    private bool isBouncing = false;
    private bool canSpawnPool = true;
    private Particles2D bloodSpray;
    private bool fakeRotPositive = false;

    [Export]
    public bool isHead = false;

    public override void _Ready()
    {
        bloodPoolScene = GD.Load<PackedScene>("res://scenes/BloodPool.tscn");
        sprite = GetNode<Sprite>("Sprite");
        tween = GetNode<Tween>("Tween");
        bloodSpray = GetNode<Particles2D>("Sprite/Particles2D");
        rng.Randomize();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        sprite.Position = new Vector2(0, Elevation);

        if (isBouncing && canSpawnPool && Mathf.IsEqualApprox(Elevation, 0.0f, 0.25f))
        {
            SpawnFastBloodPool();

            canSpawnPool = false;

            GetTree().CreateTimer(1f).Connect("timeout", this, nameof(ResetCanSpawnPool));
        }

        if (Elevation > -0.01f)
        {
            Vector2 newPosition = bloodSpray.GlobalPosition + new Vector2(0, 4);

            if (bloodAmount > 0.0f)
            {
                float dist = lastPosition.DistanceTo(Position);
                Vector2 randOffset = new Vector2(rng.RandfRange(-2, 2), rng.RandfRange(-2, 2));
                if (dist > 1.0f)
                    if (bloodAmount > 0.5f)
                        bloodTexture.AddSweepedPlus(lastPosition + randOffset, newPosition, Mathf.Max(Mathf.RoundToInt(dist), 8), bloodSpray.GlobalPosition, 0.25f);
                    else
                        bloodTexture.AddSweepedPoints(lastPosition + randOffset, newPosition, Mathf.Max(Mathf.RoundToInt(dist), 8), bloodSpray.GlobalPosition, 0.25f);
                else
                {
                    if (bloodAmount > 0.5f)
                        bloodTexture.AddPlus(newPosition + randOffset, bloodSpray.GlobalPosition, 0.25f);
                    else
                        bloodTexture.AddPoint(newPosition + randOffset, 0.25f);
                }

                bloodAmount -= delta * 2.0f;
            }

            lastPosition = newPosition;
        }

        if (!Mathf.IsEqualApprox(fakeRotationVelocity, 0, 0.1f))
        {
            float velSign = Mathf.Sign(fakeRotationVelocity);
            float dampening = fakeRotationVelocity * 0.9f * delta;
            fakeRotationVelocity -= dampening;
        }
        else
        {
            fakeRotationVelocity = 0.0f;
        }

        sprite.RotationDegrees += (fakeRotationVelocity * delta);
    }

    public void Init(BloodTexture bloodTexture, Vector2 initialImpulse, float initialTorque)
    {
        this.bloodTexture = bloodTexture;
        this.initialImpulse = initialImpulse;

        // Have to delay applying forces because Godot doesn't like to do it properly when the object is first initialised
        GetTree().CreateTimer(0.1f).Connect("timeout", this, nameof(ApplyForces), new Godot.Collections.Array() { initialImpulse, initialTorque });
        GetTree().CreateTimer(rng.RandfRange(4.0f, 6.0f)).Connect("timeout", this, nameof(StopBloodSpray));
    }

    public void BounceTween(float height)
    {
        Elevation = height;
        SetIsBouncing(true);
        tween.InterpolateProperty(this, nameof(Elevation), height, 0.0f, 2.0f, Tween.TransitionType.Bounce, Tween.EaseType.Out);
        tween.InterpolateCallback(this, 2.0f, nameof(SetIsBouncing), false);

        tween.Start();
    }

    public void SetIsBouncing(bool isBouncing)
    {
        this.isBouncing = isBouncing;
    }

    public void ResetCanSpawnPool()
    {
        canSpawnPool = true;
    }

    public void StopBloodSpray()
    {
        bloodSpray.Emitting = false;
    }

    public void ApplyForces(Vector2 initialImpulse, float initialTorque)
    {
        ApplyImpulse(Vector2.Left, initialImpulse);

        fakeRotationVelocity = initialTorque;

        fakeRotPositive = fakeRotationVelocity > 0;
    }

    public BloodPool SpawnBloodPool()
    {
        BloodPool pool = bloodPoolScene.Instance<BloodPool>();
        GetParent().AddChild(pool);
        pool.Position = bloodSpray.GlobalPosition + new Vector2(0, 4);
        pool.Start(bloodTexture);

        return pool;
    }

    public void SpawnFastBloodPool()
    {
        SpawnBloodPool().SpeedScale = 2.0f;
    }
}
