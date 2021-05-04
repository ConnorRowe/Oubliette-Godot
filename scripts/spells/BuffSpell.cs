using Godot;
using System.Collections.Generic;
using Stats;

public class BuffSpell : BaseSpell
{
    private readonly (Stat stat, float amount)[] buffs;
    private readonly float duration;
    public readonly string name;
    public readonly Shader effectShader;
    public readonly Color outlineColour;

    public BuffSpell() { }

    public BuffSpell(string name, float duration, float majykaCost, (Stat stat, float amount)[] buffs, Shader effectShader, Color outlineColour) : base(0, 0, 0, 0, majykaCost)
    {
        this.name = name;
        this.duration = duration;
        this.buffs = buffs;
        this.effectShader = effectShader;
        this.outlineColour = outlineColour;
    }

    public override void Cast(ICastsSpells source)
    {
        (source as Character).ApplyBuff(Buffs.CreateBuff(name, new List<(Stat stat, float amount)>(buffs), duration, this));

        if (source is Player player)
        {
            player.UpdatePlayerSpellEffects(effectShader, outlineColour, duration);
        }
    }

    public virtual void BuffExpired() { }
}
