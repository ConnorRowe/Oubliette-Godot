using Godot;

namespace Oubliette.Spells
{

    public class ChannelSpellBeam : Node2D
    {
        public float Range { get; set; }
        private Vector2 dir = Vector2.Zero;
        private bool isActive = false;
        private float tickRate;
        private ICastsSpells source;
        private Vector2 endPoint;
        private Line2D line;

        public override void _Ready()
        {
            line = GetNode<Line2D>("Line2D");

            SetProcess(false);
            SetPhysicsProcess(false);

        }

        public void Start(ICastsSpells source, float tickRate, float range)
        {
            this.tickRate = tickRate;
            this.source = source;
            this.Range = source.GetSpellRange(range);

            isActive = true;
            SetProcess(true);
            SetPhysicsProcess(true);

            endPoint = GlobalPosition + (dir * Range);

            GD.Print("channel node start");
        }

        public override void _Process(float delta)
        {
            Position = source.GetSpellSpawnPos() - (source as Character).GlobalPosition;

            if (source is Player player)
            {
                dir = player.facingDir;
            }
            else
            {
                dir = (source as Character).Dir;
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;

            float dist = Range;

            endPoint = GlobalPosition + (dir * Range);

            var result = spaceState.IntersectRay(GlobalPosition, endPoint, new Godot.Collections.Array() { this }, 8);

            if (result.Count > 0)
            {
                endPoint = (Vector2)result["position"];

                dist = GlobalPosition.DistanceTo(endPoint);
            }

            line.Points = new Vector2[2] { new Vector2(0, 0), dir * dist };
        }
    }
}