using Godot;
using System;

public class ProjectileSpell : BaseSpell
{
    private readonly PackedScene projectileScene;
    private readonly float hueAdjust;

    public ProjectileSpell() { }

    public ProjectileSpell(float hueAdjust, int damage, float range, float knockback, float speed, float majykaCost, NodePath projectilePath) : base(damage, range, knockback, speed, majykaCost)
    {
        projectileScene = GD.Load<PackedScene>(projectilePath);
        this.hueAdjust = hueAdjust;
    }

    public override void Cast(ICastsSpells source)
    {
        CastAndReturn(source);
    }

    public virtual Projectile CastAndReturn(ICastsSpells source)
    {
        Projectile proj = projectileScene.Instance<Projectile>();
        ((Node)source).GetParent().AddChild(proj);
        proj.GlobalPosition = source.GetSpellSpawnPos();
        proj.direction = source.GetSpellDirection();
        proj.source = (Character)source;

        if (proj.particles.ProcessMaterial != null)
            ((ParticlesMaterial)proj.particles.ProcessMaterial).HueVariation = hueAdjust;

        proj.SetSpellStats(source.GetSpellDamage(damage), source.GetSpellRange(range), source.GetSpellKnockback(knockback), source.GetSpellSpeed(speed));

        return proj;
    }
}
