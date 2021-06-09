
namespace Oubliette
{
    public class HealthPickup : BasePickup
    {
        public override void PlayerHit(Player player)
        {
            if (player.CurrentHealth < player.MaxHealth && !IsQueuedForDeletion())
            {
                player.Heal(2);
                player.PlayGulpSound();

                QueueFree();
            }
        }
    }
}