using Godot;
using System;

namespace Oubliette.Spells
{
    public abstract class BaseSpell : Reference
    {
        public string Name { get; set; }
        public float Damage { get; set; }
        public float Range { get; set; }
        public float Knockback { get; set; }
        public float Speed { get; set; }
        public float MajykaCost { get; set; }
        public Vector2 DirectionVarianceRange { get; set; } = Vector2.Zero;
        public Color BaseColour { get; set; }
        public Texture Icon { get; set; }
        public Action<Character> HitCharEvent { get; set; }
        public Action<Character> OnPickupEvent { get; set; }
        public Action<Character> OnDropEvent { get; set; }

        public BaseSpell() { }

        public BaseSpell(string name, float damage, float range, float knockback, float speed, float majykaCost, Color baseColour, Texture icon, Action<Character> hitCharEvt)
        {
            SetBasicStats(name, damage, range, knockback, speed, majykaCost, baseColour, icon, hitCharEvt);
        }

        public void SetBasicStats(string name, float damage, float range, float knockback, float speed, float majykaCost, Color baseColour, Texture icon, Action<Character> hitCharEvt)
        {
            Name = name;
            this.Damage = damage;
            this.Range = range;
            this.Knockback = knockback;
            this.Speed = speed;
            this.MajykaCost = majykaCost;
            this.BaseColour = baseColour;
            Icon = icon;
            HitCharEvent = hitCharEvt;
        }

        public abstract void Cast(ICastsSpells source);

        public abstract void Release(ICastsSpells source);

        public virtual void Process(ICastsSpells source, float delta) { }

        public void OnPickup(Character owner)
        {
            OnPickupEvent?.Invoke(owner);
        }

        public void OnDrop(Character owner)
        {
            OnDropEvent?.Invoke(owner);
        }
    }
}