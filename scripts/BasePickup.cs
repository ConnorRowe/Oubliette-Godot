using Godot;

namespace Oubliette
{
    public class BasePickup : RigidBody2D, IIntersectsPlayerHitArea
    {
        private Sprite _mainSprite;

        protected Sprite MainSprite
        {
            get
            {
                if (_mainSprite == null)
                    _mainSprite = GetNode<Sprite>("MainSprite");

                return _mainSprite;
            }
        }

        public bool IsActive { get; set; } = false;

        public override void _Ready()
        {
            base._Ready();
        }

        void IIntersectsPlayerHitArea.PlayerHit(Player player)
        {
            if (IsActive)
                PlayerHit(player);
        }

        public virtual void PlayerHit(Player player)
        {

        }
    }
}