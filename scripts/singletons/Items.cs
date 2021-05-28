using Godot;
using System.Collections.Generic;
using Stats;

public class Items : Node
{
    public enum LootPool
    {
        GENERAL,
        ENEMY,
        WOOD_CHEST
    }

    public RandomNumberGenerator Rng = new RandomNumberGenerator();
    public Dictionary<LootPool, List<Artefact>> artefactPools = new Dictionary<LootPool, List<Artefact>>() { { LootPool.GENERAL, new List<Artefact>() }, { LootPool.ENEMY, new List<Artefact>() }, { LootPool.WOOD_CHEST, new List<Artefact>() } };
    public Dictionary<LootPool, List<PackedScene>> pickupPools = new Dictionary<LootPool, List<PackedScene>>() { { LootPool.GENERAL, new List<PackedScene>() }, { LootPool.ENEMY, new List<PackedScene>() }, { LootPool.WOOD_CHEST, new List<PackedScene>() } };
    public List<Potion> potions = new List<Potion>();

    private PackedScene potionScene;
    private Artefact defaultArtefact = new Artefact("Vital Elixir", "you feel more vital!", GD.Load<Texture>("res://textures/health_potion.png"),
    (Player p) =>
    {
        p.AdjustMaxHealth(4, true);
    }, Artefact.emptyTexSet);

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

    public void RegisterPotion(Potion potion)
    {
        potions.Add(potion);
    }

    public Artefact GetRandomArtefact(LootPool lootPool)
    {
        Artefact artefactOut;

        if (artefactPools[lootPool].Count <= 0)
        {
            artefactOut = defaultArtefact;
        }
        else
        {
            artefactOut = artefactPools[lootPool][Rng.RandiRange(0, artefactPools[lootPool].Count - 1)];

            RemoveArtefactFromPools(artefactOut);
        }

        return artefactOut;
    }

    public BasePickup GetRandomPickup(LootPool lootPool)
    {
        return pickupPools[lootPool][Rng.RandiRange(0, pickupPools[lootPool].Count - 1)].Instance<BasePickup>();
    }

    public PotionPickup GetRandomPotionPickup()
    {
        PotionPickup randPotionPickup = potionScene.Instance<PotionPickup>();
        randPotionPickup.potion = (potions[Rng.RandiRange(0, potions.Count - 1)]);

        return randPotionPickup;
    }

    private void RemoveArtefactFromPools(Artefact artefact)
    {
        foreach (LootPool pool in artefactPools.Keys)
        {
            artefactPools[pool].Remove(artefact);
        }
    }

    public Items()
    {
        Rng.Randomize();
        potionScene = GD.Load<PackedScene>("res://scenes/Potion.tscn");

        RegisterArtefacts();
        RegisterPickups();
        RegisterPotions();
    }

    private void RegisterArtefacts()
    {
        RegisterArtefact(defaultArtefact, new LootPool[] { LootPool.GENERAL });

        RegisterArtefact(new Artefact("Black Bile of a Long Dead God", "raw godly power", GD.Load<Texture>("res://textures/dark_potion.png"),
        (Player p) =>
        {
            p.ApplyBuff(Buffs.CreateBuff("Black Bile of a Long Dead God",
        new List<(Stat stat, float amount)>() { (Stat.DamageMultiplier, 2.0f), (Stat.KnockbackMultiplier, -1.0f) }, 0));
        }, new Artefact.ArtefactTextureSet(new Vector2(-2, -17), null, GD.Load<Texture>("res://textures/black_eyes_down.png"), GD.Load<Texture>("res://textures/black_eyes_leftright.png"))),
        new LootPool[] { LootPool.GENERAL });

        RegisterArtefact(new Artefact("Amanita Muscaria", "this time without deer piss", GD.Load<Texture>("res://textures/amanita_muscaria.png"),
        (Player p) =>
        {
            p.ApplyBuff(Buffs.CreateBuff("Amanita Muscaria",
        new List<(Stat stat, float amount)>() { (Stat.DamageFlat, 1.0f), (Stat.RangeMultiplier, 1.25f), (Stat.MagykaCostMultiplier, 0.75f) }, 0));
        }, Artefact.emptyTexSet), new LootPool[] { LootPool.GENERAL });

        RegisterArtefact(new Artefact("Grunty's Hat", "although she's dim, she attended Fat Hag High!", GD.Load<Texture>("res://textures/witch_hat.png"),
        (Player p) =>
        {
            p.ApplyBuff(Buffs.CreateBuff("Grunty's Hat",
        new List<(Stat stat, float amount)>() { (Stat.SpellSpeedMultiplier, 1.5f), (Stat.RangeMultiplier, 2.0f), (Stat.MagykaCostMultiplier, 0.5f), (Stat.CooldownMultplier, 0.5f) }, 0));
        }, new Artefact.ArtefactTextureSet(new Vector2(-3, -22), GD.Load<Texture>("res://textures/witch_hat_equip_updown.png"), GD.Load<Texture>("res://textures/witch_hat_equip_updown.png"), GD.Load<Texture>("res://textures/witch_hat_equip_leftright.png"))
        ), new LootPool[] { LootPool.GENERAL });
    }

    private void RegisterPickups()
    {
        RegisterPickup(GD.Load<PackedScene>("res://scenes/HealthPickup.tscn"), new LootPool[] { LootPool.GENERAL, LootPool.ENEMY, LootPool.WOOD_CHEST });
    }

    private void RegisterPotions()
    {
        RegisterPotion(new Potion(new (Stat stat, float amount)[] { (Stat.ResistDamageFlat, 1.0f), (Stat.MagykaCostFlat, -5.0f) }, 10.0f, "Humble Huckleberry", Color.Color8(37, 27, 147), Color.Color8(105, 10, 130), Color.Color8(95, 103, 146)));

        RegisterPotion(new Potion(new (Stat stat, float amount)[] { (Stat.ReflectDamageFlat, 2.0f) }, 10.0f, "Beverage of Briars", Color.Color8(82, 71, 36), Color.Color8(38, 71, 36), Color.Color8(56, 36, 41)));
    }

    public override void _Ready()
    {
        base._Ready();
    }
}