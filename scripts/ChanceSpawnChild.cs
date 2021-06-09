using Godot;

namespace Oubliette
{
    public class ChanceSpawnChild : Node2D
    {
        [Export(PropertyHint.Range, "0.0,1.0")]
        public float SpawnChance { get; set; }
    }
}