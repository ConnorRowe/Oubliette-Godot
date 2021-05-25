using Godot;
using System;
using Stats;

public class Potion : Reference
{
    public readonly (Stat stat, float amount)[] buffs;
    public readonly float duration;
    public readonly string name;
    public readonly Color[] lerpColours = new Color[3] { Colors.White, Colors.White, Colors.White };

    public Potion((Stat stat, float amount)[] buffs, float duration, string name, Color colourA, Color colourB, Color colourC)
    {
        this.buffs = buffs;
        this.duration = duration;
        this.name = name;
        lerpColours[0] = colourA;
        lerpColours[1] = colourB;
        lerpColours[2] = colourC;
    }
}
