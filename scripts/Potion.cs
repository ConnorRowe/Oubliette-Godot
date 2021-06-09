using Godot;
using Oubliette.Stats;

namespace Oubliette
{
    public class Potion : Reference
    {
        public readonly (Stat stat, float amount)[] stats;
        public readonly int duration;
        public readonly string name;
        public readonly string desc;
        public readonly Color[] lerpColours = new Color[3] { Colors.White, Colors.White, Colors.White };

        public Potion((Stat stat, float amount)[] stats, int durationRooms, string name, string desc, Color colourA, Color colourB, Color colourC)
        {
            this.stats = stats;
            this.duration = durationRooms;
            this.name = name;
            this.desc = desc;
            lerpColours[0] = colourA;
            lerpColours[1] = colourB;
            lerpColours[2] = colourC;
        }
    }
}