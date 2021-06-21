using Godot;
using System;

namespace Oubliette.AI
{
    public class PlaceTrapBehaviour : AIBehaviour
    {
        private PackedScene trapScene;

        public PlaceTrapBehaviour(PackedScene trapScene, AIManager manager, Func<TransitionTestResult>[] transitions) : base(manager, transitions)
        {
            this.trapScene = trapScene;
        }

        public override void OnBehaviourStart()
        {
            SpawnTrap();

            mgr.TryTransition();
        }
        public override void Process(float delta) { }

        public override void OnBehaviourEnd() { }

        public override Vector2 Steer()
        {
            return Vector2.Zero;
        }

        private void SpawnTrap()
        {
            BearTrap trap = trapScene.Instance<BearTrap>();
            trap.SetWorld(mgr.World);
            mgr.Owner.GetParent().AddChild(trap);
            trap.Position = mgr.Owner.Position;
        }
    }
}