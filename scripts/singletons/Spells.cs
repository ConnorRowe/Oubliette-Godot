using Godot;
using System.Collections.Generic;
using Stats;

public class Spells : Node
{
    public static Dictionary<string, BaseSpell> RegisteredSpells = new Dictionary<string, BaseSpell>();

    public static T RegisterSpell<T>(string registryName, T spell) where T : BaseSpell
    {
        RegisteredSpells.Add(registryName, spell);

        return spell;
    }

    public static readonly ProjectileSpell MagicMissile = RegisterSpell<ProjectileSpell>("magic_missile", new ProjectileSpell("Magic Missile", 1, 128.0f, 100.0f, 100.0f, 25.0f, new Color(0.901961f, 0.282353f, 0.968627f), GD.Load<Texture>("res://textures/tome_magic_missile.png"), null, "res://scenes/ProjectileSmall.tscn"));
    public static readonly ProjectileSpell Shadowbolt = RegisterSpell<ProjectileSpell>("shadowbolt", new ProjectileSpell("Shadowbolt", 1, 128.0f, 100.0f, 200.0f, 25.0f, new Color(0.640667f, 0.02f, 1), GD.Load<Texture>("res://textures/tome_shadowbolt.png"), null, "res://scenes/Projectile.tscn"));
    public static readonly ProjectileSpell IceSpike = RegisterSpell<ProjectileSpell>("ice_spike", new ProjectileSpell("Ice Spike", 1, 160.0f, 40.0f, 120.0f, 25.0f, new Color(0.164063f, 0.980408f, 1), GD.Load<Texture>("res://textures/tome_icespike.png"), (Character c) => { c.ApplyBuff(Buffs.CreateBuff("Ice Spike", new List<(Stat stat, float amount)>(1) { (Stat.MoveSpeedMultiplier, -0.5f) }, 1.5f)); }, "res://scenes/ProjectileSmall.tscn"));
    public static readonly BuffSpell IceSkin = RegisterSpell<BuffSpell>("ice_skin", new BuffSpell("Ice Skin", 5.0f, 25.0f, Colors.AliceBlue, null, null, new (Stat stat, float amount)[] { (Stat.ResistDamageFlat, 1.0f), (Stat.ReflectDamageFlat, 1.0f) }, GD.Load<Shader>("res://particle/Shedding.shader"), Colors.AliceBlue));
}
