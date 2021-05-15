using Godot;
using System.Collections.Generic;

namespace Stats
{
    public enum Stat
    {
        DamageFlat,
        DamageMultiplier,
        CooldownFlat,
        CooldownMultplier,
        RangeMultiplier,
        KnockbackMultiplier,
        SpellSpeedMultiplier,
        MoveSpeedMultiplier,
        MagykaCostFlat,
        MagykaCostMultiplier,
        ReflectDamageFlat,
        ResistDamageFlat
    }

    public struct Buff
    {
        public string name;
        public List<(Stat stat, float amount)> stats;
        public uint startTime;
        // Set duration 0 to be permanent
        public uint duration;
        public BuffSpell notifyExpired;
    }

    public static class Buffs
    {
        private static readonly Dictionary<Stat, float> defaultStats = new Dictionary<Stat, float>()
        {
            {Stat.DamageFlat, 0},
            {Stat.DamageMultiplier, 1},
            {Stat.CooldownFlat, 0},
            {Stat.CooldownMultplier, 1},
            {Stat.RangeMultiplier, 1},
            {Stat.KnockbackMultiplier, 1},
            {Stat.SpellSpeedMultiplier, 1},
            {Stat.MoveSpeedMultiplier, 1},
            {Stat.MagykaCostFlat, 0},
            {Stat.MagykaCostMultiplier, 1},
            {Stat.ReflectDamageFlat, 0},
            {Stat.ResistDamageFlat, 0},
        };

        // Duration is in seconds for ease of use
        public static Buff CreateBuff(string name, List<(Stat stat, float amount)> stats, float duration, BuffSpell notifyExpired = null)
        {
            return new Buff()
            {
                name = name,
                stats = stats,
                startTime = OS.GetTicksMsec(),
                duration = (uint)Mathf.RoundToInt(duration * 1000.0f),
                notifyExpired = notifyExpired,
            };
        }

        public static Dictionary<Stat, float> DefaultStats()
        {
            return new Dictionary<Stat, float>(defaultStats);
        }
    }
}
