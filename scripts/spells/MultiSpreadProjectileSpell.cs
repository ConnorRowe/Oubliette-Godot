using Godot;
using System;

public class MultiSpreadProjectileSpell : ProjectileSpell
{
    private int projectileCount;
    private float spread;

    public MultiSpreadProjectileSpell() : base() { }

    public MultiSpreadProjectileSpell(int projectileCount, float spread, int damage, float range, float knockback, float speed, float majykaCost, Color baseColour, NodePath projectilePath) : base(damage, range, knockback, speed, majykaCost, baseColour, projectilePath)
    {
        this.projectileCount = projectileCount;
        this.spread = spread;
    }

    public override void Cast(ICastsSpells source)
    {
        Vector2 baseDir = source.GetSpellDirection();

        for (int i = 1; i <= projectileCount; ++i)
        {
            float angleFactor = (((((float)i / (float)projectileCount) - 0.5f) * (float)projectileCount) - 0.5f) * spread;

            Vector2 adjustDir = baseDir.Rotated(angleFactor);

            Projectile proj = CastAndReturn(source);
            proj.direction = adjustDir;
        }
    }
}
