using Godot;
using System;

namespace Oubliette.AI
{
    public class WanderBehaviour : MovementBehaviour
    {
        protected Godot.Collections.Array<Vector2> path = new Godot.Collections.Array<Vector2>() { };
        private SceneTreeTimer timer;

        public WanderBehaviour(AIManager manager, Func<TransitionTestResult>[] transitions) : base(manager, transitions) { }

        public override void OnBehaviourStart()
        {
            timer = mgr.owner.GetTree().CreateTimer(0.5f);
            timer.Connect("timeout", this, nameof(PickRandomPoint));

            mgr.owner.maxSpeed = 8.0f;
        }

        public override void Process(float delta) { }
        public override void OnBehaviourEnd()
        {
            timer.Disconnect("timeout", this, nameof(PickRandomPoint));

            mgr.owner.maxSpeed = (mgr.owner as AICharacter).ogMaxSpeed;
            (mgr.owner as AICharacter).DetectionAlert();
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

            do
            {
                newPath = (mgr.owner as AICharacter).world.GetNavPath(mgr.owner.GlobalPosition, mgr.owner.GlobalPosition + new Vector2(mgr.rng.RandfRange(-20, 20), mgr.rng.RandfRange(-20, 20)));

                --tries;
            } while (tries > 0 && newPath.Length > 0);

            if (newPath.Length > 0)
            {
                SetPath(newPath);
            }

            timer = mgr.owner.GetTree().CreateTimer(mgr.rng.RandfRange(1.5f, 4.0f));
            timer.Connect("timeout", this, nameof(PickRandomPoint));
        }

        // public void IsPlayerInRange()
        // {
        //     Player player = CheckForPlayer((mgr.owner as NewAITest).shapeQuery, mgr.owner.GetWorld2d().DirectSpaceState);

        //     mgr.owner.GetTree().CreateTimer(0.5f).Connect("timeout", this, nameof(IsPlayerInRange));
        // }
    }
}