using Godot;
using System;

namespace Oubliette.Spells
{
    public class ProjectileSpellBuilder : Reference, ISpellBuilder<ProjectileSpellBuilder, ProjectileSpell>
    {
        private string name;
        private int damage;
        private float range;
        private float knockback;
        private float speed;
        private float majykaCost;
        private Color baseColour;
        private Texture icon;
        private Action<Character> hitCharEvent;
        private Curve curveX;
        private Curve curveY;
        private float curveInterpSpeed;
        private float curveMoveSpeed;
        private PackedScene projectileScene;

        public ProjectileSpellBuilder SetName(string name)
        {
            this.name = name;

            return this;
        }

        public ProjectileSpellBuilder SetDamage(int damage)
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

        public ProjectileSpell Build()
        {
            ProjectileSpell build = new ProjectileSpell();
            build.SetBasicStats(name, damage, range, knockback, speed, majykaCost, baseColour, icon, hitCharEvent);
            build.CurveX = curveX;
            build.CurveY = curveY;
            build.CurveInterpSpeed = curveInterpSpeed;
            build.CurveMoveSpeed = curveMoveSpeed;
            build.ProjectileScene = projectileScene;

            return build;
        }
    }
}