using Godot;

namespace Oubliette
{
    public class BigSpillage : SpillageHazard
    {
        public override void _Ready()
        {
            base._Ready();

            var animSprite = GetNode<AnimatedSprite>("AnimatedSprite");

            animSprite.Connect("animation_finished", this, nameof(AnimationFinished));
            animSprite.Play();

            Bubbles.Emitting = false;
        }

        public void AnimationFinished()
        {
            Bubbles.Emitting = true;
        }

        public override void SetColours(Color spillageColour, Color bubbleColor)
        {
            base.SetColours(spillageColour, bubbleColor);

            GetNode<AnimatedSprite>("AnimatedSprite").Modulate = spillageColour;
        }
    }
}
