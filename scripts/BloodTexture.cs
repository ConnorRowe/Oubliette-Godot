using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oubliette.LevelGen
{
    public class BloodTexture : Sprite
    {
        private static readonly HashSet<Vector2> plusPixels = new HashSet<Vector2>() { Vector2.Zero, Vector2.Left, Vector2.Up, Vector2.Right, Vector2.Down };

        private ImageTexture drawImageTexture;
        private Image image;
        private HashSet<(Vector2 dst, Color colour)> pixelsToDraw = new HashSet<(Vector2 dst, Color colour)>();
        private HashSet<(Image img, Rect2 srcRect, Vector2 dst)> imagesToDraw = new HashSet<(Image img, Rect2 srcRect, Vector2 dst)>();
        private bool isActive = false;
    
        public Image.Format ImageFormat { get; set; } = Image.Format.Rgbah;
        public Vector2 BloodCheckPos { get; set; } = Vector2.Zero;
        public Color BloodCheckColour { get; set; } = Colors.Transparent;
        public float CheckAlpha { get { return BloodCheckColour.a; } }
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                isActive = value;
                SetProcess(isActive);
            }
        }

        [Export]
        public Vector2 ImageSize { get; set; } = new Vector2(288, 224);

        public override void _Ready()
        {
            ResetImage();

            SetProcess(isActive);
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            image.Lock();

            // Draw all pixels
            Parallel.ForEach<(Vector2 dst, Color colour)>(pixelsToDraw, point =>
            {
                if (point.dst.x < ImageSize.x && point.dst.y < ImageSize.y && point.dst.x >= 0 && point.dst.y >= 0)
                {
                    Color baseColour = image.GetPixelv(point.dst.Round());
                    Color blend = point.colour.LinearInterpolate(baseColour, 0.5f * baseColour.a);

                    blend.a = System.Math.Max(baseColour.a, point.colour.a);

                    image.SetPixelv(point.dst.Round(), blend);
                }
            });

            // Process all texture blits
            Parallel.ForEach<(Image img, Rect2 srcRect, Vector2 dst)>(imagesToDraw, img =>
            {
                image.BlitRectMask(img.img, img.img, img.srcRect, img.dst);
                img.img.Dispose();
            });

            // Check player position for drawing their blood trail (can only read pixel when data is locked)
            Vector2 checkPos = (BloodCheckPos - GlobalPosition).Round();
            if (checkPos.x < ImageSize.x && checkPos.y < ImageSize.y && checkPos.x > 0 && checkPos.y > 0)
            {
                BloodCheckColour = image.GetPixelv(checkPos);
            }

            image.Unlock();
            pixelsToDraw.Clear();
            imagesToDraw.Clear();
            drawImageTexture.CreateFromImage(image, 1);
        }

        // Queue a single pixel to draw
        public void AddPixel(Vector2 point, Color colour)
        {
            pixelsToDraw.Add((point, colour));
        }

        // Queue a set of pixels to draw
        private void AddPixels(HashSet<(Vector2 dst, Color colour)> points)
        {
            pixelsToDraw.UnionWith(points);
        }

        // Queue a plus (+) shape of pixels to draw - removePixel arg is used to keep the pixel under the player empty if needed
        public void AddPlus(Vector2 origin, Vector2 removePixel, Color colour)
        {
            AddPixels((MakePlus(origin, removePixel, colour)));
        }

        // Make a set of pixels in the shape of a plus
        private HashSet<(Vector2 dst, Color colour)> MakePlus(Vector2 origin, Vector2 removePixel, Color colour)
        {
            HashSet<(Vector2 dst, Color colour)> newPlus = new HashSet<(Vector2 dst, Color colour)>();
            foreach (Vector2 pixel in plusPixels)
            {
                newPlus.Add((pixel + origin, colour));
            }

            newPlus.Remove((removePixel - GlobalPosition, colour));

            return newPlus;
        }

        // Sweep a defined number of pixels between two points
        private HashSet<(Vector2 dst, Color colour)> SweepPixels(Vector2 start, Vector2 end, int maxPoints, Color colour)
        {
            var diff_X = end.x - start.x;
            var diff_Y = end.y - start.y;

            var interval_X = diff_X / (maxPoints + 1);
            var interval_Y = diff_Y / (maxPoints + 1);

            HashSet<(Vector2 dst, Color colour)> pixels = new HashSet<(Vector2 dst, Color colour)>();
            for (int i = 1; i <= maxPoints; i++)
            {
                pixels.Add((new Vector2(start.x + interval_X * i, end.y + interval_Y * i).Round(), colour));
            }

            return pixels;
        }

        // Queue a set of sweeped pixels between two points to draw
        public void AddSweepedPixels(Vector2 start, Vector2 end, int maxPoints, Vector2 removePixel, Color colour)
        {
            HashSet<(Vector2 dst, Color colour)> pointSet = SweepPixels(start - GlobalPosition, end - GlobalPosition, maxPoints, colour);
            pointSet.Remove((removePixel.Round(), colour));

            AddPixels(pointSet);
        }

        // Queue a set of sweeped plus shapes between two points to draw
        public void AddSweepedPlus(Vector2 start, Vector2 end, int maxPoints, Vector2 removePixel, Color colour)
        {

            HashSet<(Vector2 dst, Color colour)> pointSet = SweepPixels(start - GlobalPosition, end - GlobalPosition, maxPoints, colour);
            pointSet.Remove((removePixel.Round(), colour));

            HashSet<(Vector2 dst, Color colour)> plusSet = new HashSet<(Vector2 dst, Color colour)>();

            foreach ((Vector2 dst, Color colour) p in pointSet)
            {
                plusSet.UnionWith(MakePlus(p.dst, removePixel, p.colour));
            }

            AddPixels(plusSet);
        }

        // Queue an image to blit
        public void BlitImage(Image img, Rect2 srcRect, Vector2 dst)
        {
            imagesToDraw.Add((img, srcRect, dst - GlobalPosition));
        }

        // Generate base image data
        public void ResetImage()
        {
            image = new Image();
            image.Create(Mathf.RoundToInt(ImageSize.x), Mathf.RoundToInt(ImageSize.y), false, ImageFormat);
            drawImageTexture = new ImageTexture();
            drawImageTexture.CreateFromImage(image, 1);
            Texture = drawImageTexture;
        }

        // Dispose image / texture when freed
        public override void _ExitTree()
        {
            base._ExitTree();

            drawImageTexture.Dispose();
            image.Dispose();
        }
    }
}