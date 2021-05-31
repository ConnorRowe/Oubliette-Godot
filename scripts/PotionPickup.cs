using Godot;
using Stats;

public class PotionPickup : BasePickup
{
    public Potion potion;

    public override void _Ready()
    {
        base._Ready();

        UpdateMaterial();
    }

    public void UpdateMaterial()
    {
        ShaderMaterial shaderMat = (ShaderMaterial)MainSprite.Material;

        shaderMat.SetShaderParam("colour_lerp_a", potion.lerpColours[0]);
        shaderMat.SetShaderParam("colour_lerp_b", potion.lerpColours[1]);
        shaderMat.SetShaderParam("colour_lerp_c", potion.lerpColours[2]);
    }

    public override void PlayerHit(Player player)
    {
        if(IsQueuedForDeletion())
            return;

        if (player.PickUpPotion(potion))
            QueueFree();
    }
}
