using Godot;
using System.Collections.Generic;
using Oubliette.Stats;

namespace Oubliette.Spells
{
    public class Spells : Node
    {
        public static Dictionary<string, BaseSpell> RegisteredSpells = new Dictionary<string, BaseSpell>();

        public static T RegisterSpell<T>(string registryName, T spell) where T : BaseSpell
        {
            RegisteredSpells.Add(registryName, spell);

            return spell;
        }

        public static readonly ProjectileSpell MagicMissile = RegisterSpell<ProjectileSpell>("magic_missile", new ProjectileSpellBuilder().SetName("Magic Missile").SetDamage(1).SetRange(128.0f).SetKnockback(100.0f).SetSpeed(100.0f).SetMajykaCost(25.0f).SetBaseColour(new Color(0.901961f, 0.282353f, 0.968627f)).SetIcon(GD.Load<Texture>("res://textures/tome_magic_missile.png")).SetProjectileScene(GD.Load<PackedScene>("res://scenes/ProjectileSmall.tscn")).Build());
        public static readonly ProjectileSpell Shadowbolt = RegisterSpell<ProjectileSpell>("shadowbolt", new ProjectileSpellBuilder().SetName("Shadowbolt").SetDamage(1).SetRange(128.0f).SetKnockback(100.0f).SetSpeed(200.0f).SetMajykaCost(25.0f).SetBaseColour(new Color(0.640667f, 0.02f, 1)).SetIcon(GD.Load<Texture>("res://textures/tome_shadowbolt.png")).SetProjectileScene(GD.Load<PackedScene>("res://scenes/Projectile.tscn")).SetCurves(GD.Load<Curve>("res://curve/InOut.tres"), null, 2f, 80.0f).Build());
        public static readonly ProjectileSpell IceSpike = RegisterSpell<ProjectileSpell>("ice_spike", new ProjectileSpell("Ice Spike", 1, 160.0f, 40.0f, 120.0f, 25.0f, new Color(0.164063f, 0.980408f, 1), GD.Load<Texture>("res://textures/tome_icespike.png"), (Character c) => { c.ApplyTimedBuff(Buffs.CreateBuff("Ice Spike", new List<(Stat stat, float amount)>(1) { (Stat.MoveSpeedMultiplier, -0.5f) }, 1.5f)); }, GD.Load<PackedScene>("res://scenes/ProjectileSmall.tscn")));
        public static readonly BuffSpell IceSkin = RegisterSpell<BuffSpell>("ice_skin", new BuffSpell("Ice Skin", 5.0f, 25.0f, Colors.AliceBlue, null, null, new (Stat stat, float amount)[] { (Stat.ResistDamageFlat, 1.0f), (Stat.ReflectDamageFlat, 1.0f) }, GD.Load<Shader>("res://particle/Shedding.shader"), Colors.AliceBlue));
    }
}