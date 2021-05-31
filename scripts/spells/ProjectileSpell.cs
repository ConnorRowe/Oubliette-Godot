using Godot;
using System;

public class ProjectileSpell : BaseSpell
{
    private readonly PackedScene projectileScene;

    public ProjectileSpell() { }

    public ProjectileSpell(string name, int damage, float range, float knockback, float speed, float majykaCost, Color baseColour, Texture icon, Action<Character> hitCharEvt, NodePath projectilePath) : base(name, damage, range, knockback, speed, majykaCost, baseColour, icon, hitCharEvt)
    {
        projectileScene = GD.Load<PackedScene>(projectilePath);
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
        proj.SetProjectileColour(source.GetSpellColour(baseColour));

        proj.SetSpellStats(source.GetSpellDamage(damage), source.GetSpellRange(range), source.GetSpellKnockback(knockback), source.GetSpellSpeed(speed));
        proj.SetHitCharEvent(HitCharEvent);

        return proj;
    }
}
