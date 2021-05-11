using Godot;
using Stats;

public class Potion : BasePickup
{
    private (Stat stat, float amount)[] buffs;
    private float duration;
    public string name;


    public void SetPotionEffects((Stat stat, float amount)[] buffs, float duration, string name, Color colourA, Color colourB, Color colourC)
    {
        this.buffs = buffs;
        this.duration = duration;
        this.name = name;

        ShaderMaterial shaderMat = (ShaderMaterial)MainSprite.Material;

        shaderMat.SetShaderParam("colour_lerp_a", colourA);
        shaderMat.SetShaderParam("colour_lerp_b", colourB);
        shaderMat.SetShaderParam("colour_lerp_c", colourC);
    }

    public override void PlayerHit(Player player)
    {
        player.ApplyBuff(Buffs.CreateBuff(name, new System.Collections.Generic.List<(Stat stat, float amount)>(buffs), duration));

        QueueFree();
    }
}
