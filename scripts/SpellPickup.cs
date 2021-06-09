
namespace Oubliette
{
    public class SpellPickup : BasePickup
    {
        private BaseSpell spell;

        public void SetSpell(BaseSpell spell)
        {
            this.spell = spell;

            MainSprite.Texture = spell.Icon;
        }

        public override void PlayerHit(Player player)
        {
            if (!IsQueuedForDeletion())
            {
                player.PickUpPrimarySpell(spell);

                QueueFree();
            }
        }
    }
}