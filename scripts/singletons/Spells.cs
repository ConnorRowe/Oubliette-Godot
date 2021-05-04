using Godot;
using System.Collections.Generic;
using Stats;

public class Spells : Node
{
    public static Dictionary<string, BaseSpell> RegisteredSpells = new Dictionary<string, BaseSpell>();

    public static T RegisterSpell<T>(string name, T spell) where T : BaseSpell
    {
        RegisteredSpells.Add(name, spell);

        return spell;
    }

    public readonly ProjectileSpell FireBall = RegisterSpell<ProjectileSpell>("fireball", new ProjectileSpell(0.0f, 1, 128.0f, 100.0f, 100.0f, 25.0f));
    public readonly ProjectileSpell Shadowbolt = RegisterSpell<ProjectileSpell>("shadowbolt", new ProjectileSpell(0.81f, 1, 128.0f, 100.0f, 200.0f, 25.0f));
    public readonly BuffSpell IceSkin = RegisterSpell<BuffSpell>("ice_skin", new BuffSpell("ice_skin", 5.0f, 25.0f, new (Stat stat, float amount)[] { (Stat.ResistDamageFlat, 1.0f), (Stat.ReflectDamageFlat, 1.0f) }, GD.Load<Shader>("res://particle/Shedding.shader"), Colors.AliceBlue));
}
