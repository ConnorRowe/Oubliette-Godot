using Godot;
using System;

public class Pedestal : StaticBody2D, IIntersectsPlayerHitArea
{
    public Sprite ItemSprite { get; set; }
    public Artefact CurrentArtefact { get; set; }
    private bool itemTaken = false;

    public override void _Ready()
    {
        ItemSprite = GetNode<Sprite>("Sprite/ItemSprite");

        CurrentArtefact = GetNode<Items>("/root/Items").GetRandomArtefact(Items.LootPool.GENERAL);
        ItemSprite.Texture = CurrentArtefact.Texture;
    }

    void IIntersectsPlayerHitArea.PlayerHit(Player player)
    {
        if (!itemTaken)
        {
            CurrentArtefact.PlayerPickUp(player);
            ItemSprite.Visible = false;
            itemTaken = true;
        }
    }
}
