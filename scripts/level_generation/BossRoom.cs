using Godot;

namespace Oubliette.LevelGen
{
    public class BossRoom : Room
    {
        private PackedScene pedestalScene;
        private PackedScene ladderScene;
        private bool ladderEntered = false;

        public override void _Ready()
        {
            base._Ready();

            pedestalScene = GD.Load<PackedScene>("res://scenes/Pedestal.tscn");
            ladderScene = GD.Load<PackedScene>("res://scenes/level_generation/Ladders.tscn");
        }

        public override void RoomCleared()
        {
            base.RoomCleared();

            Pedestal bossItem = pedestalScene.Instance<Pedestal>();
            AddChild(bossItem);
            bossItem.Position = new Vector2(177, 143);
            bossItem.GenerateItem();

            Area2D ladder = ladderScene.Instance<Area2D>();
            AddChild(ladder);
            ladder.Position = bossItem.Position + new Vector2(0, -64.0f);
            ladder.Connect("area_entered", this, nameof(LadderEntered));
        }

        private void LadderEntered(Area2D other)
        {
            if (!ladderEntered && other.Name == "FeetArea")
            {
                GetParent().GetNode<LevelGenerator>("LevelGenerator").RegenerateLevel();
                ladderEntered = true;
            }
        }
    }
}