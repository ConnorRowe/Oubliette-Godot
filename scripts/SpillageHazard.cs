using Godot;

namespace Oubliette
{
    public class SpillageHazard : Area2D
    {
        public float LifeTime { get; set; } = 2.0f;
        public string DmgSourceName { get; set; } = "Spillage";
        public bool EnemyOwned { get; set; } = true;
        public bool Active { get; set; } = true;

        private Particles2D _bubbles;
        protected Particles2D Bubbles
        {
            get
            {
                if (_bubbles == null)
                {
                    _bubbles = GetNode<Particles2D>("Bubbles");
                }

                return _bubbles;
            }
        }

        public override void _Ready()
        {
            base._Ready();

            GetTree().CreateTimer(0.1f).Connect("timeout", this, nameof(CheckOverlap));

            Monitoring = true;
        }

        private void CheckOverlap()
        {
            foreach (Area2D area in GetOverlappingAreas())
            {
                if (area.GetParent() is Character)
                {
                    area.EmitSignal("area_entered", this);
                }
            }

            SetDeferred("monitoring", false);
        }

        public override void _Process(float delta)
        {
            LifeTime -= delta;

            if (LifeTime <= 0.0f)
            {
                if (Modulate.a > 0)
                {
                    Modulate = new Color(Modulate, Mathf.Lerp(Modulate.a, 0.0f, delta * 2.0f));
                }

                if (Modulate.a < 0.05)
                {
                    QueueFree();
                }
            }
        }

        public virtual void SetColours(Color spillageColour, Color bubbleColor)
        {
            GetNode<Sprite>("Sprite").SelfModulate = spillageColour;
            Bubbles.SelfModulate = bubbleColor;
        }
    }
}