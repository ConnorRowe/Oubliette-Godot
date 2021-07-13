using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oubliette
{
    public class ClothSim : Node2D
    {
        private struct PointData
        {
            public int pointIndex;
            public int[] boundPoints;
        }

        private const float SPRING_DAMPENING = 0.95f;
        private const float MOVEMENT_DAMPENING = 0.95f;
        private const float SPRING_TARGET_LENGTH = 1f;
        private const float SPRING_STIFFNESS = 6.0f;
        private const float GRAVITY = 98f / 8;
        private static readonly Vector2[] uvs = new Vector2[4] { Vector2.Zero, Vector2.Right, Vector2.Down, Vector2.One };

        private static bool debug_drawPoints = false;
        private static readonly Color debug_pointColour = new Color(1f, .2f, .8f);

        private ClothSimPoints[] points;
        private HashSet<PointData> pointsData = new HashSet<PointData>();
        private bool isSetUp = false;
        private Vector2[] defaultStaticPointPositions;

        public List<int[]> QuadIndices { get; } = new List<int[]>();
        public Vector2 SystemMovementVelocity { get; set; } = Vector2.Zero;
        public int MaxStaticPoint { get; set; } = 5;
        public Vector2 StaticPointsOffset { get; set; }
        public Color[] ClothColours { get; set; } = new Color[] { Colors.White };

        [Export]
        public bool KeepDefaultPoints { get; set; } = false;
        [Export]
        public NodePath PointsParentPath { get; set; } = "";

        public void SetUp()
        {
            if (KeepDefaultPoints)
            {
                QuadIndices.Add(new int[] { 0, 1, 6, 5 });
                QuadIndices.Add(new int[] { 1, 2, 7, 6 });
                QuadIndices.Add(new int[] { 2, 3, 8, 7 });
                QuadIndices.Add(new int[] { 3, 4, 9, 8 });
                QuadIndices.Add(new int[] { 5, 6, 11, 10 });
                QuadIndices.Add(new int[] { 6, 7, 12, 11 });
                QuadIndices.Add(new int[] { 7, 8, 13, 12 });
                QuadIndices.Add(new int[] { 8, 9, 14, 13 });
                QuadIndices.Add(new int[] { 10, 11, 16, 15 });
                QuadIndices.Add(new int[] { 11, 12, 17, 16 });
                QuadIndices.Add(new int[] { 12, 13, 18, 17 });
                QuadIndices.Add(new int[] { 13, 14, 19, 18 });
                QuadIndices.Add(new int[] { 15, 16, 21, 20 });
                QuadIndices.Add(new int[] { 16, 17, 22, 21 });
                QuadIndices.Add(new int[] { 17, 18, 23, 22 });
                QuadIndices.Add(new int[] { 18, 19, 24, 23 });
            }
            else
            {
                GetNode("Points").QueueFree();
            }

            // Generate data from points nodes

            Node pointsParent;
            if (PointsParentPath.IsEmpty())
            {
                pointsParent = GetNode("Points");
            }
            else
            {
                pointsParent = GetNode(PointsParentPath);
            }

            var pointsGD = pointsParent.GetChildren();
            points = new ClothSimPoints[pointsGD.Count];

            defaultStaticPointPositions = new Vector2[MaxStaticPoint];

            // Make standard array from the Godot.Collections.Array
            // Also cache the initial positions of the static points
            for (int i = 0; i < pointsGD.Count; i++)
            {
                ClothSimPoints p = (ClothSimPoints)pointsGD[i];
                points[i] = p;

                p.Index = i;

                if (i < MaxStaticPoint)
                {
                    defaultStaticPointPositions[i] = p.Position;
                }
            }

            // Generate struct data
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];

                PointData data = new PointData()
                {
                    pointIndex = i,
                    boundPoints = new int[p.bound_points.Count],
                };

                if (p.bound_points.Count > 0)
                {
                    int j = 0;
                    foreach (var nodePath in p.bound_points)
                    {
                        data.boundPoints[j++] = p.GetNode<ClothSimPoints>(nodePath).Index;
                    }
                }

                pointsData.Add(data);
            }

            isSetUp = true;
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            if (!isSetUp)
                return;

            Parallel.ForEach(pointsData, data =>
            {
                if (data.pointIndex >= MaxStaticPoint)
                {
                    Vector2 vel = Vector2.Zero;
                    vel.y += GRAVITY;
                    vel += SystemMovementVelocity;

                    Vector2 position = points[data.pointIndex].Position;

                    foreach (int i in data.boundPoints)
                    {
                        var other = points[i];

                        float length = position.DistanceTo(other.Position);

                        float springForce = (length - SPRING_TARGET_LENGTH) * -SPRING_STIFFNESS;

                        vel += other.Position.DirectionTo(position) * springForce;
                    }

                    vel *= SPRING_DAMPENING;

                    points[data.pointIndex].Position = position + (vel * delta);
                }
                else
                {
                    points[data.pointIndex].Position = defaultStaticPointPositions[data.pointIndex] + StaticPointsOffset;
                }
            });

            SystemMovementVelocity *= MOVEMENT_DAMPENING;

            Update();
        }

        public override void _Draw()
        {
            base._Draw();

            if (!isSetUp)
                return;

            foreach (var quad in BuildQuads())
            {
                DrawPrimitive(quad, ClothColours, uvs);
            }

            if (debug_drawPoints)
            {
                foreach (var point in points)
                {
                    DrawCircle(point.Position, SPRING_TARGET_LENGTH / 2, debug_pointColour);
                }
            }
        }

        private Vector2[][] BuildQuads()
        {
            Vector2[][] quads = new Vector2[QuadIndices.Count][];

            for (int x = 0; x < QuadIndices.Count; x++)
            {
                quads[x] = new Vector2[4];

                for (int y = 0; y < 4; y++)
                {
                    var t = QuadIndices[x][y];
                    quads[x][y] = points[t].Position;
                }
            }

            return quads;
        }
    }
}
