using Godot;
using System;

namespace Oubliette.Spells
{
    public interface ISpellBuilder<out R, out U>
        where U : BaseSpell
    {
        R SetName(string name);
        R SetDamage(float damage);
        R SetRange(float range);
        R SetKnockback(float knockback);
        R SetSpeed(float speed);
        R SetMajykaCost(float majykaCost);
        R SetBaseColour(Color baseColour);
        R SetIcon(Texture icon);
        R SetHitCharEvent(Action<Character> hitCharEvent);
        U Build();
    }
}