using Godot;

namespace Oubliette.Spells
{
    public class RepeatCastChannelSpell : ChannelSpell
    {
        public BaseSpell RepeatCastSpell { get; set; }

        public RepeatCastChannelSpell() { }

        public RepeatCastChannelSpell(string name, BaseSpell repeatCastSpell, Color baseColour, float majykaCost, Texture icon) : base(name, 0, 0, 0, 0, majykaCost, baseColour, icon, null)
        {
            RepeatCastSpell = repeatCastSpell;
        }

        public override void ChannelTick()
        {
            RepeatCastSpell.Cast(sourceCache);

            base.ChannelTick();
        }
    }
}