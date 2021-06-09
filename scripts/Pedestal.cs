using Godot;

namespace Oubliette.LevelGen
{
    public class Pedestal : StaticBody2D, IIntersectsPlayerHitArea
    {
        public Sprite ItemSprite { get; set; }
        public Artefact CurrentArtefact { get; set; }
        public BaseSpell CurrentSpell { get; set; }
        private bool hasGeneratedItem = false;
        private bool itemTaken = false;
        private bool isHoldingSpell = false;
        private AudioStream artefactPickupSound;

        public override void _Ready()
        {
            ItemSprite = GetNode<Sprite>("Sprite/ItemSprite");
            artefactPickupSound = GD.Load<AudioStream>("res://sound/sfx/item_pickup_mixdown.wav");
        }

        void IIntersectsPlayerHitArea.PlayerHit(Player player)
        {
            if (!itemTaken)
            {
                if (isHoldingSpell)
                    player.PickUpPrimarySpell(CurrentSpell);
                else
                {
                    player.world.PlayGlobalSoundEffect(artefactPickupSound);
                    CurrentArtefact.PlayerPickUp(player);
                }

                ItemSprite.Visible = false;
                itemTaken = true;
            }
        }

        public void GenerateItem()
        {
            if (!hasGeneratedItem)
            {
                Items items = GetNode<Items>("/root/Items");

                bool tryGetSpell = items.Rng.Randf() <= 0.25f;

                if (tryGetSpell)
                {
                    CurrentSpell = items.GetRandomSpell(Items.LootPool.GENERAL);
                    if (CurrentSpell != null)
                    {
                        isHoldingSpell = true;
                    }
                }

                if (!isHoldingSpell)
                    CurrentArtefact = items.GetRandomArtefact(Items.LootPool.GENERAL);

                ItemSprite.Texture = isHoldingSpell ? CurrentSpell.Icon : CurrentArtefact.Texture;

                hasGeneratedItem = true;
            }
        }
    }
}