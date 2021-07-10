using Godot;
using System;

namespace Oubliette.Spells
{
    public class ProjectileSpellBuilder : Reference, ISpellBuilder<ProjectileSpellBuilder, ProjectileSpell>
    {
        private string name;
        private float damage;
        private float range;
        private float knockback;
        private float speed;
        private float majykaCost;
        private Color baseColour;
        private Texture icon;
        private Action<Character> hitCharEvent;
        private Action<Character> onPickupEvent;
        private Action<Character> onDropEvent;
        private Curve curveX;
        private Curve curveY;
        private float curveInterpSpeed;
        private float curveMoveSpeed;
        private PackedScene projectileScene;
        private Vector2 gravity = Vector2.Zero;
        private Vector2 directionVarianceRange = Vector2.Zero;

        public ProjectileSpellBuilder SetName(string name)
        {
            this.name = name;

            return this;
        }

        public ProjectileSpellBuilder SetDamage(float damage)
        {
            this.damage = damage;

            return this;
        }

        public ProjectileSpellBuilder SetRange(float range)
        {
            this.range = range;

            return this;
        }

        public ProjectileSpellBuilder SetKnockback(float knockback)
        {
            this.knockback = knockback;

            return this;
        }

        public ProjectileSpellBuilder SetSpeed(float speed)
        {
            this.speed = speed;

            return this;
        }

        public ProjectileSpellBuilder SetMajykaCost(float majykaCost)
        {
            this.majykaCost = majykaCost;

            return this;
        }

        public ProjectileSpellBuilder SetBaseColour(Color baseColour)
        {
            this.baseColour = baseColour;

            return this;
        }

        public ProjectileSpellBuilder SetIcon(Texture icon)
        {
            this.icon = icon;

            return this;
        }

        public ProjectileSpellBuilder SetHitCharEvent(Action<Character> hitCharEvent)
        {
            this.hitCharEvent = hitCharEvent;

            return this;
        }

        public ProjectileSpellBuilder SetOnPickupEvent(Action<Character> onPickupEvent)
        {
            this.onPickupEvent = onPickupEvent;

            return this;
        }

        public ProjectileSpellBuilder SetOnDropEvent(Action<Character> onDropEvent)
        {
            this.onDropEvent = onDropEvent;

            return this;
        }

        public ProjectileSpellBuilder SetProjectileScene(PackedScene projectileScene)
        {
            this.projectileScene = projectileScene;

            return this;
        }

        public ProjectileSpellBuilder SetCurves(Curve curveX, Curve curveY, float curveInterpSpeed, float curveMoveSpeed)
        {
            this.curveX = curveX;
            this.curveY = curveY;
            this.curveInterpSpeed = curveInterpSpeed;
            this.curveMoveSpeed = curveMoveSpeed;

            return this;
        }

        public ProjectileSpellBuilder SetGravity(Vector2 gravity)
        {
            this.gravity = gravity;

            return this;
        }

        public ProjectileSpellBuilder SetDirectionVarianceRange(Vector2 directionVarianceRange)
        {
            this.directionVarianceRange = directionVarianceRange;

            return this;
        }

        public ProjectileSpell Build()
        {
            ProjectileSpell build = new ProjectileSpell();
            build.SetBasicStats(name, damage, range, knockback, speed, majykaCost, baseColour, icon, hitCharEvent);
            build.CurveX = curveX;
            build.CurveY = curveY;
            build.CurveInterpSpeed = curveInterpSpeed;
            build.CurveMoveSpeed = curveMoveSpeed;
            build.ProjectileScene = projectileScene;
            build.OnPickupEvent = onPickupEvent;
            build.OnDropEvent = onDropEvent;
            build.Gravity = gravity;
            build.DirectionVarianceRange = directionVarianceRange;

            return build;
        }
    }
}