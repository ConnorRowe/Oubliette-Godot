using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oubliette
{
    public class BloodTexture : Sprite
    {
        private ImageTexture drawImageTexture;
        private Image image;
        private HashSet<(Vector2 dst, float alpha)> pixelsToDraw = new HashSet<(Vector2 dst, float alpha)>();
        private HashSet<(Image img, Rect2 srcRect, Vector2 dst)> imagesToDraw = new HashSet<(Image img, Rect2 srcRect, Vector2 dst)>();
        private readonly HashSet<Vector2> plusPoints = new HashSet<Vector2>() { Vector2.Zero, Vector2.Left, Vector2.Up, Vector2.Right, Vector2.Down };

        public Vector2 BloodCheckPos = Vector2.Zero;
        private float checkAlpha = 0.0f;
        public float CheckAlpha { get { return checkAlpha; } }

        public override void _Ready()
        {
            ResetImage();
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            image.Lock();

            Parallel.ForEach<(Vector2 dst, float alpha)>(pixelsToDraw, point =>
            {
                if (point.dst.x <= 2816 - 1 && point.dst.y <= 2816 - 1 && point.dst.x >= 0 && point.dst.y >= 0)
                {
                    float alphaBlend = Mathf.Min(image.GetPixelv(point.dst.Round()).a + point.alpha, 1.0f);

                    image.SetPixelv(point.dst.Round(), new Color(1.0f, 1.0f, 1.0f, alphaBlend));
                }
            });

            Parallel.ForEach<(Image img, Rect2 srcRect, Vector2 dst)>(imagesToDraw, img =>
            {
                image.BlitRectMask(img.img, img.img, img.srcRect, img.dst);
            });

            checkAlpha = image.GetPixelv(BloodCheckPos.Round()).a;

            image.Unlock();
            pixelsToDraw.Clear();
            imagesToDraw.Clear();
            drawImageTexture.CreateFromImage(image, 1);
        }

        public void AddPoint(Vector2 point, float alpha = 1.0f)
        {
            pixelsToDraw.Add((point, alpha));
        }

        public void AddPoints(HashSet<(Vector2 dst, float alpha)> points)
        {
            pixelsToDraw.UnionWith(points);
        }

        public void AddPlus(Vector2 origin, Vector2 removePixel, float alpha = 1.0f)
        {
            AddPoints((MakePlus(origin, removePixel, alpha)));
        }

        private HashSet<(Vector2 dst, float alpha)> MakePlus(Vector2 origin, Vector2 removePixel, float alpha = 1.0f)
        {
            HashSet<(Vector2 dst, float alpha)> newPlus = new HashSet<(Vector2 dst, float alpha)>();
            foreach (Vector2 point in plusPoints)
            {
                newPlus.Add((point + origin, alpha));
            }

            newPlus.Remove((removePixel, alpha));

            return newPlus;
        }

        private HashSet<(Vector2 dst, float alpha)> SweepPoints(Vector2 start, Vector2 end, int maxPoints, float alpha = 1.0f)
        {
            var diff_X = end.x - start.x;
            var diff_Y = end.y - start.y;

            var interval_X = diff_X / (maxPoints + 1);
            var interval_Y = diff_Y / (maxPoints + 1);

            HashSet<(Vector2 dst, float alpha)> pointSet = new HashSet<(Vector2 dst, float alpha)>();
            for (int i = 1; i <= maxPoints; i++)
            {
                pointSet.Add((new Vector2(start.x + interval_X * i, end.y + interval_Y * i).Round(), alpha));
            }

            return pointSet;
        }

        public void AddSweepedPoints(Vector2 start, Vector2 end, int maxPoints, Vector2 removePixel, float alpha = 1.0f)
        {
            HashSet<(Vector2 dst, float alpha)> pointSet = SweepPoints(start, end, maxPoints, alpha);
            pointSet.Remove((removePixel.Round(), alpha));

            AddPoints(pointSet);
        }

        public void AddSweepedPlus(Vector2 start, Vector2 end, int maxPoints, Vector2 removePixel, float alpha = 1.0f)
        {

            HashSet<(Vector2 dst, float alpha)> pointSet = SweepPoints(start, end, maxPoints, alpha);
            pointSet.Remove((removePixel.Round(), alpha));

            HashSet<(Vector2 dst, float alpha)> plusSet = new HashSet<(Vector2 dst, float alpha)>();

            foreach ((Vector2 dst, float alpha) p in pointSet)
            {
                plusSet.UnionWith(MakePlus(p.dst, removePixel, p.alpha));
            }

            AddPoints(plusSet);
        }

        public void BlitImage(Image img, Rect2 srcRect, Vector2 dst, float alpha = 1.0f)
        {
            imagesToDraw.Add((img, srcRect, dst));
        }

        public void ResetImage()
        {
            image = new Image();
            image.Create(2816, 2816, false, Image.Format.La8);
            drawImageTexture = (ImageTexture)Texture;
            drawImageTexture.CreateFromImage(image, 1);
        }
    }
}