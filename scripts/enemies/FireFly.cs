using Godot;

namespace Oubliette.AI
{
    public class FireFly : AICharacter
    {
        private float circleTime = 0.0f;
        private const float circleSpeed = 2.0f;
        private RandomNumberGenerator rng;
        private Curve flicker;
        private ShaderMaterial spriteMat;
        private ShaderMaterial deathBubblesMat;
        private static Spells.MultiSpreadProjectileSpell deathRattleSpell = new Spells.MultiSpreadProjectileSpell(3, Mathf.Tau / 3.0f, "Firebolt",
            new Spells.ProjectileSpellBuilder().SetDamage(1).SetRange(80f).SetSpeed(20f).SetKnockback(50f).SetBaseColour(new Color(0.921569f, 0.427451f, 0.098039f))
            .SetProjectileScene(GD.Load<PackedScene>("res://scenes/ProjectileSmall.tscn"))
            .SetCurves(GD.Load<Curve>("res://curve/ProjSpiral_x.tres"), GD.Load<Curve>("res://curve/ProjSpiral_y.tres"), 4f, 50f).Build());

        public override void _Ready()
        {
            base._Ready();

            flicker = GD.Load<Curve>("res://curve/torchFlicker.tres");
            spriteMat = (ShaderMaterial)charSprite.Material;
            deathBubblesMat = (ShaderMaterial)GetNode<Particles2D>("DeathBubbles").ProcessMaterial;
            rng = new RandomNumberGenerator();
            rng.Randomize();

            circleTime += rng.Randf();
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            circleTime += (delta * circleSpeed) * rng.Randf();
            if (circleTime > 1.0f)
                circleTime -= 1.0f;

            float angle = Mathf.Lerp(0.0f, Mathf.Tau, circleTime);
            float inOut = Mathf.Cos(circleTime);

            charSprite.Position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (0.5f + (inOut * 5.0f));
            hitbox.Position = charSprite.Position;

            spriteMat.SetShaderParam("intensity", Mathf.Lerp(3.0f, 5.0f, flicker.InterpolateBaked(circleTime)));
        }

        public override void Die()
        {
            GetNode<Particles2D>("DeathBubbles").Emitting = true;

            tween.InterpolateMethod(this, nameof(SetNumberBubblesShown), 0, 10, 1.6f, Tween.TransitionType.Cubic, Tween.EaseType.Out);
            tween.InterpolateMethod(this, nameof(SetNumberBubblesShown), 10, 0, 0.6f, Tween.TransitionType.Cubic, Tween.EaseType.OutIn, 1.6f);
            tween.Start();

            deathRattleSpell.Cast(this);

            base.Die();
        }

        private void SetNumberBubblesShown(int number)
        {
            deathBubblesMat.SetShaderParam("number_particles_shown", number);
        }

        public override Vector2 GetSpellSpawnPos()
        {
            return base.GetSpellSpawnPos() + new Vector2(0, -8);
        }

        protected override Vector2 GetSpellDirection()
        {
            return DirectionExt.AsVector(Direction.Down);
        }
    }
}