using Godot;

namespace Oubliette
{
    public class BloodPool : AnimatedSprite
    {
        private BloodTexture bloodTexture;

        [Export]
        private Texture baseTex;

        public override void _Ready()
        {
            Connect("frame_changed", this, nameof(FrameChanged));
        }

        public void Start(BloodTexture bloodTexture)
        {
            this.bloodTexture = bloodTexture;
            RenderToBloodTexture();
            Playing = true;
        }

        private void FrameChanged()
        {
            RenderToBloodTexture();

            if (Frame == Frames.GetFrameCount(Animation) - 1)
            {
                QueueFree();
            }
        }

        private void RenderToBloodTexture()
        {
            Image frameImg = new Image();
            frameImg.CreateFromData(baseTex.GetWidth(), baseTex.GetHeight(), false, Image.Format.La8, baseTex.GetData().GetData());
            bloodTexture.BlitImage(frameImg, (Frames.GetFrame(Animation, Frame) as AtlasTexture).Region, Position + Offset);
        }
    }
}