using System.Collections.Generic;
using Stats;

public class ElementalFruit : BasePickup
{
    public override void PlayerHit(Player player)
    {
        if (player.currentHealth < player.maxHealth)
        {
            player.Heal(4);
            player.ApplyTimedBuff(Buffs.CreateBuff("elemental fruit", new List<(Stat stat, float amount)>() { (Stat.MagykaCostMultiplier, 0.5f), (Stat.MoveSpeedMultiplier, 1.5f) }, 10.0f));
            QueueFree();
        }
    }
}
