using Godot;

namespace Oubliette.AI
{
    public class Slime : AICharacter
    {
        private PackedScene spillageHazard;
        private Vector2 lastPosition;
        private Vector2 distanceMoved = Vector2.Zero;
        private readonly Vector2 distBetweenSpillages = new Vector2(10.0f, 5.5f);

        public override void _Ready()
        {
            base._Ready();

            spillageHazard = GD.Load<PackedScene>("res://scenes/SpillageHazard.tscn");
            lastPosition = Position;
        }

        public override void _PhysicsProcess(float delta)
        {
            distanceMoved += (Position - lastPosition).Abs();
            lastPosition = Position;

            if (distanceMoved.x >= distBetweenSpillages.x || distanceMoved.y >= distBetweenSpillages.y)
            {
                distanceMoved = Vector2.Zero;

                SpawnSpillage();
            }


            base._PhysicsProcess(delta);
        }

        private void SpawnSpillage()
        {
            SpillageHazard newSpillage = spillageHazard.Instance<SpillageHazard>();
            GetParent().AddChild(newSpillage);

            newSpillage.GlobalPosition = GlobalPosition + new Vector2(0, -7);
            newSpillage.SetColours(new Color(0.431373f, 1, 0), new Color(0.933333f, 1, 0.878431f));
            newSpillage.DmgSourceName = DamageSourceName + "'s Acid Trail";
        }
    }
}