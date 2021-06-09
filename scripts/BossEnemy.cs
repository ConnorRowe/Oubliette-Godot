using Godot;
using Oubliette.AI;

namespace Oubliette.AI
{
    public class BossEnemy : AICharacter
    {
        public override void ApplyKnockBack(Vector2 vel)
        {
            Vector2 kb = vel.Normalized();

            base.ApplyKnockBack(kb * (vel.Length() * 0.25f));
        }
    }
}