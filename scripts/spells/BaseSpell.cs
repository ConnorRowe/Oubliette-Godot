using Godot;
using System;

public abstract class BaseSpell : Reference
{
    protected readonly int damage;
    protected readonly float range;
    protected readonly float knockback;
    protected readonly float speed;
    public readonly float majykaCost;

    public BaseSpell() { }

    public BaseSpell(int damage, float range, float knockback, float speed, float majykaCost)
    {
        this.damage = damage;
        this.range = range;
        this.knockback = knockback;
        this.speed = speed;
        this.majykaCost = majykaCost;
    }

    public abstract void Cast(ICastsSpells source);
}
