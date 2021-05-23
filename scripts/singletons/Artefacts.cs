using Godot;
using System.Collections.Generic;
using Stats;

public class Artefacts : Node
{
    public enum LootPool
    {
        GENERAL,
        ENEMY
    }

    public RandomNumberGenerator Rng = new RandomNumberGenerator();
    public Dictionary<LootPool, List<Artefact>> artefactPools = new Dictionary<LootPool, List<Artefact>>() { { LootPool.GENERAL, new List<Artefact>() }, { LootPool.ENEMY, new List<Artefact>() } };
    public Dictionary<LootPool, List<PackedScene>> pickupPools = new Dictionary<LootPool, List<PackedScene>>() { { LootPool.GENERAL, new List<PackedScene>() }, { LootPool.ENEMY, new List<PackedScene>() } };

    public void RegisterArtefact(Artefact artifact, LootPool[] lootPools)
    {
        foreach (LootPool pool in lootPools)
        {
            artefactPools[pool].Add(artifact);
        }
    }

    public void RegisterPickup(PackedScene pickup, LootPool[] lootPools)
    {
        foreach (LootPool pool in lootPools)
        {
            pickupPools[pool].Add(pickup);
        }
    }

    public Artefact GetRandomArtefact(LootPool lootPool)
    {
        return artefactPools[lootPool][Rng.RandiRange(0, artefactPools[lootPool].Count - 1)];
    }

    public BasePickup GetRandomPickup(LootPool lootPool)
    {
        return pickupPools[lootPool][Rng.RandiRange(0, pickupPools[lootPool].Count - 1)].Instance<BasePickup>();
    }

    public Artefacts()
    {
        Rng.Randomize();

        // Artefact registration

        RegisterArtefact(new Artefact("Black Bile of a Long Dead God", GD.Load<Texture>("res://textures/dark_potion.png"),
        (Player p) =>
        {
            p.ApplyBuff(Buffs.CreateBuff("Black Bile of a Long Dead God",
        new List<(Stat stat, float amount)>() { (Stat.DamageMultiplier, 2.0f), (Stat.KnockbackMultiplier, -1.0f) }, 0));
        }),
        new LootPool[] { LootPool.GENERAL });

        RegisterArtefact(new Artefact("Vital Elixir", GD.Load<Texture>("res://textures/health_potion.png"),
        (Player p) =>
        {
            p.AdjustMaxHealth(4, true);
        }), new LootPool[] { LootPool.GENERAL });

        RegisterArtefact(new Artefact("Amanita Muscaria", GD.Load<Texture>("res://textures/amanita_muscaria.png"),
        (Player p) =>
        {
            p.ApplyBuff(Buffs.CreateBuff("Amanita Muscaria",
        new List<(Stat stat, float amount)>() { (Stat.DamageFlat, 1.0f), (Stat.RangeMultiplier, 1.25f), (Stat.MagykaCostMultiplier, 0.75f) }, 0));
        }), new LootPool[] { LootPool.GENERAL });

        RegisterPickup(GD.Load<PackedScene>("res://scenes/ElementalFruit.tscn"), new LootPool[] { LootPool.GENERAL });
    }

    public override void _Ready()
    {
        base._Ready();
    }
}