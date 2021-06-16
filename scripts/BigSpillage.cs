using Godot;

namespace Oubliette
{
    public class BigSpillage : SpillageHazard
    {
        public override void _Ready()
        {
            base._Ready();

            GetNode<AnimatedSprite>("AnimatedSprite").Connect("animation_finished", this, nameof(AnimationFinished));

            Active = false;

            Bubbles.Emitting = false;
        }

        public void AnimationFinished()
        {
            Active = true;
            Bubbles.Emitting = true;
        }

        public override void SetColours(Color spillageColour, Color bubbleColor)
        {
            base.SetColours(spillageColour, bubbleColor);

            GetNode<AnimatedSprite>("AnimatedSprite").Modulate = spillageColour;
        }
    }
}
