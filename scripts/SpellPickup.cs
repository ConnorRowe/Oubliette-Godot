using Oubliette.Spells;

namespace Oubliette
{
    public class SpellPickup : BasePickup
    {
        private BaseSpell spell;

        public override void _Ready()
        {
            base._Ready();

            IsActive = true;
        }

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