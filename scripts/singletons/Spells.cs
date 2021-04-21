using Godot;
using System;

public class Spells : Node
{
    public delegate void OnSpellCast(Player player, Spell spell);
    // Returns damage to deal to player
    public delegate int PlayerTakesDamage(Player player, int damage, Character source);

    public struct Spell
    {
        public readonly string name;
        public readonly string description;
        public readonly int cost;
        public readonly int damage;
        public readonly float duration;
        public readonly Shader particleShader;
        public readonly OnSpellCast onCast;
        public readonly PlayerTakesDamage onTakeDamage;
        public readonly Color outlineColour;

        public Spell(string name, string description, int cost, int damage, float duration, Shader particleShader, OnSpellCast onCast, PlayerTakesDamage onTakeDamage, Color outlineColour)
        {
            this.name = name;
            this.description = description;
            this.cost = cost;
            this.damage = damage;
            this.duration = duration;
            this.particleShader = particleShader;
            this.onCast = onCast;
            this.onTakeDamage = onTakeDamage;
            this.outlineColour = outlineColour;
        }

        public bool IsValid()
        {
            return !name.Empty() && onCast != null;
        }
    }

    public Spell Ice_Skin = new Spell("Ice Skin", "", 10, 1, 5.0f, GD.Load<Shader>("res://particle/Shedding.shader"), (Player player, Spell spell) => { GD.Print("Casting " + spell.name); }, (Player player, int damage, Character source) => { source?.TakeDamage(1); player.ReduceSpellTimer(damage); return 0; }, Colors.AliceBlue);

    public override void _Ready()
    {

    }
}
