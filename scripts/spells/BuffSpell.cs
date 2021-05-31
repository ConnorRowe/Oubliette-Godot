using Godot;
using System;
using System.Collections.Generic;
using Stats;

public class BuffSpell : BaseSpell
{
    private readonly (Stat stat, float amount)[] buffs;
    private readonly float duration;
    public readonly Shader effectShader;
    public readonly Color outlineColour;

    public BuffSpell() { }

    public BuffSpell(string name, float duration, float majykaCost, Color baseColour, Texture icon, Action<Character> hitCharEvt, (Stat stat, float amount)[] buffs, Shader effectShader, Color outlineColour) : base(name, 0, 0, 0, 0, majykaCost, baseColour, icon, hitCharEvt)
    {
        this.duration = duration;
        this.buffs = buffs;
        this.effectShader = effectShader;
        this.outlineColour = outlineColour;
    }

    public override void Cast(ICastsSpells source)
    {
        (source as Character).ApplyBuff(Buffs.CreateBuff(Name, new List<(Stat stat, float amount)>(buffs), duration, this));

        if (source is Player player)
        {
            player.UpdatePlayerSpellEffects(effectShader, outlineColour, duration);
        }
    }

    public virtual void BuffExpired() { }
}
