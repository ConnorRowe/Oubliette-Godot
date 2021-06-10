using Godot;
using System.Threading.Tasks;
using Oubliette.LevelGen;

namespace Oubliette
{
    public class BloodPool : AnimatedSprite
    {
        public Color BloodColour { get; set; }

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
            frameImg.Convert(bloodTexture.ImageFormat);
            frameImg.Lock();

            Parallel.For(0, frameImg.GetHeight(), (y) =>
            {
                for (int x = 0; x < frameImg.GetWidth(); ++x)
                {
                    Color p = frameImg.GetPixel(x, y);
                    if (p.a > 0.0f)
                    {
                        frameImg.SetPixel(x, y, p * BloodColour);
                    }
                }
            });

            frameImg.Lock();

            bloodTexture.BlitImage(frameImg, (Frames.GetFrame(Animation, Frame) as AtlasTexture).Region, Position + Offset);
        }
    }
}