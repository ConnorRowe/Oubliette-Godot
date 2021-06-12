using Godot;

namespace Oubliette.Spells
{
    public class MultiSpreadProjectileSpell : ProjectileSpell
    {
        private int projectileCount;
        private float spread;

        public MultiSpreadProjectileSpell() : base() { }

        public MultiSpreadProjectileSpell(int projectileCount, float spread, string name, ProjectileSpell baseProjectileSpell) : base(name, baseProjectileSpell.Damage, baseProjectileSpell.Range, baseProjectileSpell.Knockback, baseProjectileSpell.Speed, baseProjectileSpell.MajykaCost, baseProjectileSpell.BaseColour, baseProjectileSpell.Icon, baseProjectileSpell.HitCharEvent, baseProjectileSpell.ProjectileScene)
        {
            this.projectileCount = projectileCount;
            this.spread = spread;
            this.CurveX = baseProjectileSpell.CurveX;
            this.CurveY = baseProjectileSpell.CurveY;
            this.CurveInterpSpeed = baseProjectileSpell.CurveInterpSpeed;
            this.CurveMoveSpeed = baseProjectileSpell.CurveMoveSpeed;
        }

        public override void Cast(ICastsSpells source)
        {
            Vector2 baseDir = source.GetSpellDirection();

            for (int i = 1; i <= projectileCount; ++i)
            {
                float angleFactor = (((((float)i / (float)projectileCount) - 0.5f) * (float)projectileCount) - 0.5f) * spread;

                Vector2 adjustDir = baseDir.Rotated(angleFactor);

                Projectile proj = CastAndReturn(source);
                proj.SetDirection(adjustDir);
            }
        }
    }
}