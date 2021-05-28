using Godot;
using System;

public class HealthPickup : BasePickup
{
    public override void PlayerHit(Player player)
    {
        if (player.currentHealth < player.maxHealth)
        {
            player.Heal(2);

            QueueFree();
        }
    }
}