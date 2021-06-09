using Godot;
using System.Collections.Generic;
using Oubliette.Stats;

namespace Oubliette
{
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
        public Dictionary<LootPool, List<(BaseSpell spell, float weight)>> spellPools = new Dictionary<LootPool, List<(BaseSpell spell, float weight)>>() { { LootPool.GENERAL, new List<(BaseSpell spell, float weight)>() }, { LootPool.ENEMY, new List<(BaseSpell spell, float weight)>() }, { LootPool.WOOD_CHEST, new List<(BaseSpell, float)>() } };
        public List<Potion> potions = new List<Potion>();

        private Dictionary<LootPool, float> artefactPoolWeightSum = new Dictionary<LootPool, float>() { { LootPool.GENERAL, 0.0f }, { LootPool.ENEMY, 0.0f }, { LootPool.WOOD_CHEST, 0.0f } };
        private Dictionary<LootPool, float> spellPoolWeightSum = new Dictionary<LootPool, float>() { { LootPool.GENERAL, 0.0f }, { LootPool.ENEMY, 0.0f }, { LootPool.WOOD_CHEST, 0.0f } };

        private PackedScene potionScene;
        private Artefact defaultArtefact = new Artefact("Vital Elixir", "you feel more vital!", 1.0f, GD.Load<Texture>("res://textures/health_potion.png"),
        (Player p) =>
        {
            p.AdjustMaxHealth(4, true);
        }, Artefact.emptyTexSet);
        private PackedScene spellPickupScene;

        public void RegisterArtefact(Artefact artifact, LootPool[] lootPools)
        {
            foreach (LootPool pool in lootPools)
            {
                artefactPools[pool].Add(artifact);

                artefactPoolWeightSum[pool] += artifact.RarityWeight;
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

        public void RegisterSpell(BaseSpell spell, float rarityWeight, LootPool[] lootPools)
        {
            foreach (LootPool pool in lootPools)
            {
                spellPools[pool].Add((spell, rarityWeight));
            }
        }

        public Artefact GetRandomArtefact(LootPool lootPool)
        {
            Artefact artefactOut = null;

            if (artefactPools[lootPool].Count <= 0)
            {
                artefactOut = defaultArtefact;
            }
            else
            {
                float rnd = Rng.RandfRange(0.0f, artefactPoolWeightSum[lootPool] - 0.001f);

                foreach (Artefact artefact in artefactPools[lootPool])
                {
                    if (rnd < artefact.RarityWeight)
                    {
                        artefactOut = artefact;
                        RemoveArtefactFromPools(artefact);
                        break;
                    }

                    rnd -= artefact.RarityWeight;
                }
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

        public BaseSpell GetRandomSpell(LootPool lootPool)
        {
            BaseSpell spellOut = null;

            if (spellPools[lootPool].Count > 0)
            {
                float rnd = Rng.RandfRange(0.0f, spellPoolWeightSum[lootPool] - 0.001f);

                foreach (var spell in spellPools[lootPool])
                {
                    if (rnd < spell.weight)
                    {
                        spellOut = spell.spell;
                        RemoveSpellFromPools(spell);
                        break;
                    }

                    rnd -= spell.weight;
                }
            }

            return spellOut;
        }

        public SpellPickup GetRandomSpellPickup(LootPool lootPool)
        {
            BaseSpell spell = GetRandomSpell(lootPool);

            if (spell == null)
                return null;

            SpellPickup spellPickup = spellPickupScene.Instance<SpellPickup>();
            spellPickup.SetSpell(spell);

            return spellPickup;
        }

        private void RemoveArtefactFromPools(Artefact artefact)
        {
            foreach (LootPool pool in artefactPools.Keys)
            {
                artefactPools[pool].Remove(artefact);

                artefactPoolWeightSum[pool] -= artefact.RarityWeight;
            }
        }

        public void RemoveSpellFromPools((BaseSpell spell, float weight) spell)
        {
            foreach (LootPool pool in spellPools.Keys)
            {
                spellPools[pool].Remove(spell);
            }
        }

        public (BaseSpell spell, float weight) FindSpellPoolEntry(BaseSpell spell, LootPool lootPool)
        {
            return spellPools[lootPool].Find(((BaseSpell s, float w) f) => { return f.s.Name == spell.Name; });
        }

        public Items()
        {
            Rng.Randomize();
            potionScene = GD.Load<PackedScene>("res://scenes/Potion.tscn");
            spellPickupScene = GD.Load<PackedScene>("res://scenes/SpellPickup.tscn");

            RegisterArtefacts();
            RegisterPickups();
            RegisterPotions();
            RegisterSpells();
        }

        private void RegisterArtefacts()
        {
            RegisterArtefact(defaultArtefact, new LootPool[] { LootPool.GENERAL });

            RegisterArtefact(new Artefact("Black Bile of a Long Dead God", "raw godly power", rarityWeight: 0.3f, GD.Load<Texture>("res://textures/dark_potion.png"),
            (Player p) =>
            {
                p.ApplyTimedBuff(Buffs.CreateBuff("Black Bile of a Long Dead God",
            new List<(Stat stat, float amount)>() { (Stat.DamageMultiplier, 2.0f), (Stat.KnockbackMultiplier, -1.0f) }, 0));
            }, new Artefact.ArtefactTextureSet(new Vector2(-2, -17), null, GD.Load<Texture>("res://textures/black_eyes_down.png"), GD.Load<Texture>("res://textures/black_eyes_leftright.png"))),
            new LootPool[] { LootPool.GENERAL });

            RegisterArtefact(new Artefact("Amanita Muscaria", "this time without deer piss", rarityWeight: 0.5f, GD.Load<Texture>("res://textures/amanita_muscaria.png"),
            (Player p) =>
            {
                p.ApplyTimedBuff(Buffs.CreateBuff("Amanita Muscaria",
            new List<(Stat stat, float amount)>() { (Stat.DamageFlat, 1.0f), (Stat.RangeMultiplier, 1.25f), (Stat.MagykaCostMultiplier, 0.75f) }, 0));
            }, Artefact.emptyTexSet), new LootPool[] { LootPool.GENERAL });

            RegisterArtefact(new Artefact("Grunty's Hat", "although she's dim, she attended Fat Hag High!", rarityWeight: 0.7f, GD.Load<Texture>("res://textures/witch_hat.png"),
            (Player p) =>
            {
                p.MixInSpellColour(new Color(0.546667f, 1, 0.15f), 1.0f);
                p.ApplyTimedBuff(Buffs.CreateBuff("Grunty's Hat",
            new List<(Stat stat, float amount)>() { (Stat.SpellSpeedMultiplier, 1.5f), (Stat.RangeMultiplier, 2.0f), (Stat.MagykaCostMultiplier, -0.5f), (Stat.CooldownMultplier, -0.5f) }, 0));
            }, new Artefact.ArtefactTextureSet(new Vector2(-3, -22), GD.Load<Texture>("res://textures/witch_hat_equip_updown.png"), GD.Load<Texture>("res://textures/witch_hat_equip_updown.png"), GD.Load<Texture>("res://textures/witch_hat_equip_leftright.png"))
            ), new LootPool[] { LootPool.GENERAL });
        }

        private void RegisterPickups()
        {
            RegisterPickup(GD.Load<PackedScene>("res://scenes/HealthPickup.tscn"), new LootPool[] { LootPool.GENERAL, LootPool.ENEMY, LootPool.WOOD_CHEST });
        }

        private void RegisterPotions()
        {
            RegisterPotion(new Potion(new (Stat stat, float amount)[] { (Stat.MagykaCostFlat, -10.0f) }, 3, "Humble Huckleberry", "you feel magical", Color.Color8(37, 27, 147), Color.Color8(105, 10, 130), Color.Color8(95, 103, 146)));

            RegisterPotion(new Potion(new (Stat stat, float amount)[] { (Stat.ReflectDamageFlat, 2.0f) }, 5, "Beverage of Briars", "makes you... thorny", Color.Color8(82, 71, 36), Color.Color8(38, 71, 36), Color.Color8(56, 36, 41)));
        }

        private void RegisterSpells()
        {
            RegisterSpell(Spells.MagicMissile, 1.0f, new LootPool[] { LootPool.GENERAL });
            RegisterSpell(Spells.Shadowbolt, 0.5f, new LootPool[] { LootPool.GENERAL });
            RegisterSpell(Spells.IceSpike, 1.0f, new LootPool[] { LootPool.GENERAL });
        }

        public void ResetItemPools()
        {
            artefactPools = new Dictionary<LootPool, List<Artefact>>() { { LootPool.GENERAL, new List<Artefact>() }, { LootPool.ENEMY, new List<Artefact>() }, { LootPool.WOOD_CHEST, new List<Artefact>() } };
            spellPools = new Dictionary<LootPool, List<(BaseSpell spell, float weight)>>() { { LootPool.GENERAL, new List<(BaseSpell spell, float weight)>() }, { LootPool.ENEMY, new List<(BaseSpell spell, float weight)>() }, { LootPool.WOOD_CHEST, new List<(BaseSpell, float)>() } };
            artefactPoolWeightSum = new Dictionary<LootPool, float>() { { LootPool.GENERAL, 0.0f }, { LootPool.ENEMY, 0.0f }, { LootPool.WOOD_CHEST, 0.0f } };
            spellPoolWeightSum = new Dictionary<LootPool, float>() { { LootPool.GENERAL, 0.0f }, { LootPool.ENEMY, 0.0f }, { LootPool.WOOD_CHEST, 0.0f } };

            RegisterArtefacts();
            RegisterSpells();
        }

        public override void _Ready()
        {
            base._Ready();
        }
    }
}