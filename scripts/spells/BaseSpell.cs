using Godot;
using System;

namespace Oubliette
{
    public abstract class BaseSpell : Reference
    {
        public readonly string Name;
        protected readonly int damage;
        protected readonly float range;
        protected readonly float knockback;
        protected readonly float speed;
        public readonly float majykaCost;
        public readonly Color baseColour;
        public readonly Texture Icon;
        public readonly Action<Character> HitCharEvent;

        public BaseSpell() { }

        public BaseSpell(string name, int damage, float range, float knockback, float speed, float majykaCost, Color baseColour, Texture icon, Action<Character> hitCharEvt)
        {
            Name = name;
            this.damage = damage;
            this.range = range;
            this.knockback = knockback;
            this.speed = speed;
            this.majykaCost = majykaCost;
            this.baseColour = baseColour;
            Icon = icon;
            HitCharEvent = hitCharEvt;
        }

        public abstract void Cast(ICastsSpells source);
    }
}