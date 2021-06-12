using Godot;

namespace Oubliette
{
    public class SpillageProjectile : Projectile
    {
        private Vector2 lastPosition;
        private Vector2 distanceMoved = Vector2.Zero;
        private readonly Vector2 distBetweenSpillages = new Vector2(10.0f, 4.5f);


        [Export]
        private PackedScene spillageScene;
        [Export]
        private Color spillageModulate;

        public override void _Ready()
        {
            base._Ready();

            lastPosition = Position;
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

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
            SpillageHazard newSpillage = spillageScene.Instance<SpillageHazard>();
            GetParent().AddChild(newSpillage);

            newSpillage.GlobalPosition = GlobalPosition + new Vector2(0, 7);
            newSpillage.SetColours(spillageModulate, new Color(0.933333f, 1, 0.878431f));
            newSpillage.DmgSourceName = $"{Source.DamageSourceName}'s Slime Ball's Acid Trail";
        }
    }
}