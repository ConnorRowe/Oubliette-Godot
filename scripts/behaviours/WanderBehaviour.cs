using Godot;
using System;

namespace Oubliette.AI
{
    public class WanderBehaviour : MovementBehaviour
    {
        private Godot.Collections.Array<Vector2> path = new Godot.Collections.Array<Vector2>() { };
        private SceneTreeTimer timer;
        private float wanderSpeed;
        private Vector2 timerRange;

        public WanderBehaviour(float wanderSpeed, Vector2 timerRange, AIManager manager, Func<TransitionTestResult>[] transitions) : base(manager, transitions)
        {
            this.wanderSpeed = wanderSpeed;
            this.timerRange = timerRange;
        }

        public override void OnBehaviourStart()
        {
            timer = mgr.Owner.GetTree().CreateTimer(0.5f);
            timer.Connect("timeout", this, nameof(PickRandomPoint));

            mgr.Owner.MaxSpeed = wanderSpeed;
        }

        public override void Process(float delta) { }
        public override void OnBehaviourEnd()
        {
            if (timer != null && timer.IsConnected("timeout", this, nameof(PickRandomPoint)))
                timer.Disconnect("timeout", this, nameof(PickRandomPoint));

            mgr.Owner.MaxSpeed = (mgr.Owner as AICharacter).OgMaxSpeed;
            (mgr.Owner as AICharacter).DetectionAlert();
        }

        public override Vector2 Steer()
        {
            return SteerToNextPoint(path);
        }

        public void SetPath(Vector2[] path)
        {
            this.path = new Godot.Collections.Array<Vector2>(path);

            if (path.Length > 0)
            {
                isOnPath = true;
            }
        }

        private void PickRandomPoint()
        {
            int tries = 4;
            Vector2[] newPath = { };
            Navigation2D nav = (mgr.Owner as AICharacter).NavProvider.GetNavigation();

            do
            {
                Vector2 roomPos = mgr.World.LevelGenerator.CurrentRoom.GlobalPosition;
                Vector2 endPos = nav.GetClosestPoint(nav.ToLocal(roomPos + new Vector2(mgr.rng.RandfRange(32f, 320f), mgr.rng.RandfRange(32f, 255f))));

                newPath = AIManager.GetNavPathGlobal(mgr.Owner.GlobalPosition, endPos, nav);

                if (newPath.Length > 0)
                    tries = 0;

                --tries;
            } while (tries > 0);

            if (newPath.Length > 0)
            {
                SetPath(newPath);
            }

            timer = mgr.Owner.GetTree().CreateTimer(mgr.rng.RandfRange(1.5f, 4.0f));
            timer.Connect("timeout", this, nameof(PickRandomPoint));
        }
    }
}