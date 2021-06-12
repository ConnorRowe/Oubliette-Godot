using Godot;
using System;

namespace Oubliette.Spells
{
    public class ProjectileSpell : BaseSpell
    {
        public PackedScene ProjectileScene { get; set; }
        public Curve CurveX { get; set; }
        public Curve CurveY { get; set; }
        public float CurveInterpSpeed { get; set; }
        public float CurveMoveSpeed { get; set; }

        public ProjectileSpell() { }

        public ProjectileSpell(string name, int damage, float range, float knockback, float speed, float majykaCost, Color baseColour, Texture icon, Action<Character> hitCharEvt, PackedScene projectileScene) : base(name, damage, range, knockback, speed, majykaCost, baseColour, icon, hitCharEvt)
        {
            ProjectileScene = projectileScene;
        }

        public override void Cast(ICastsSpells source)
        {
            CastAndReturn(source);
        }

        public virtual Projectile CastAndReturn(ICastsSpells source)
        {
            Projectile proj = ProjectileScene.Instance<Projectile>();
            ((Node)source).GetParent().AddChild(proj);
            proj.GlobalPosition = source.GetSpellSpawnPos();
            proj.SetDirection(source.GetSpellDirection());
            proj.Source = (Character)source;
            proj.SetProjectileColour(source.GetSpellColour(BaseColour));
            proj.CurveX = CurveX;
            proj.CurveY = CurveY;
            proj.CurveInterpSpeed = CurveInterpSpeed;
            proj.CurveMoveSpeed = CurveMoveSpeed;
            proj.SetSpellStats(source.GetSpellDamage(Damage), source.GetSpellRange(Range), source.GetSpellKnockback(Knockback), source.GetSpellSpeed(Speed), $"{(source as Character).DamageSourceName}'s {Name}");
            proj.SetHitCharEvent(HitCharEvent);

            return proj;
        }
    }
}