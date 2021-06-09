using Godot;
using System.Collections.Generic;

namespace Oubliette.LevelGen
{
    public class Chest : RigidBody2D, IIntersectsPlayerHitArea
    {
        private AnimationPlayer animPlayer;
        private Sprite artefactSprite;
        private bool itemTaken = false;
        private RandomNumberGenerator rng;
        public Artefact artefact = null;
        public List<BasePickup> pickups = new List<BasePickup>();

        [Export]
        public bool isOpen = false;

        public override void _Ready()
        {
            animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            artefactSprite = GetNode<Sprite>("ArtefactSprite");

            Items items = GetNode<Items>("/root/Items");
            rng = items.Rng;

            if (rng.Randf() <= 0.25f)
            {
                // artefact = items.GetRandomArtefact(Items.LootPool.GENERAL);
                // artefactSprite.Texture = artefact.Texture;
            }

            int maxPickups = rng.RandiRange(0, 2);
            for (int i = 0; i < maxPickups; i++)
            {
                if (rng.Randf() <= 0.3f)
                    pickups.Add(items.GetRandomPotionPickup());
                else
                    pickups.Add(items.GetRandomPickup(Items.LootPool.WOOD_CHEST));
            }

            foreach (BasePickup pickup in pickups)
            {
                GetParent().GetParent().CallDeferred("add_child", pickup);
            }
        }

        void IIntersectsPlayerHitArea.PlayerHit(Player player)
        {
            if (isOpen)
            {
                if (!itemTaken && artefact != null)
                {
                    artefact.PlayerPickUp(player);
                    itemTaken = true;
                    artefactSprite.Visible = false;
                }
            }
            else if (!animPlayer.IsPlaying())
            {
                animPlayer.Play("OpenChest");
            }
        }

        public void SpawnPickups()
        {
            foreach (BasePickup pickup in pickups)
            {
                pickup.Position = Position + (new Vector2(rng.Randi(), rng.Randi()).Normalized() * 3.0f);
                GetParent().GetParent().RemoveChild(pickup);
                GetParent().AddChild(pickup);
            }

            pickups.Clear();
        }
    }
}