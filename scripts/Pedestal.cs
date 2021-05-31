using Godot;
using System;

public class Pedestal : StaticBody2D, IIntersectsPlayerHitArea
{
    public Sprite ItemSprite { get; set; }
    public Artefact CurrentArtefact { get; set; }
    public BaseSpell CurrentSpell { get; set; }
    private bool itemTaken = false;
    private bool isHoldingSpell = false;

    public override void _Ready()
    {
        ItemSprite = GetNode<Sprite>("Sprite/ItemSprite");

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
    }

    void IIntersectsPlayerHitArea.PlayerHit(Player player)
    {
        if (!itemTaken)
        {
            if (isHoldingSpell)
                player.PickUpPrimarySpell(CurrentSpell);
            else
                CurrentArtefact.PlayerPickUp(player);

            ItemSprite.Visible = false;
            itemTaken = true;
        }
    }
}
