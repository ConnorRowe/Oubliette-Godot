using Godot;
using System;

namespace Oubliette.Spells
{
    public abstract class BaseSpell : Reference
    {
        public string Name { get; set; }
        public int Damage { get; set; }
        public float Range { get; set; }
        public float Knockback { get; set; }
        public float Speed { get; set; }
        public float MajykaCost { get; set; }
        public Color BaseColour { get; set; }
        public Texture Icon { get; set; }
        public Action<Character> HitCharEvent { get; set; }

        public BaseSpell() { }

        public BaseSpell(string name, int damage, float range, float knockback, float speed, float majykaCost, Color baseColour, Texture icon, Action<Character> hitCharEvt)
        {
            SetBasicStats(name, damage, range, knockback, speed, majykaCost, baseColour, icon, hitCharEvt);
        }

        public void SetBasicStats(string name, int damage, float range, float knockback, float speed, float majykaCost, Color baseColour, Texture icon, Action<Character> hitCharEvt)
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
    }
}