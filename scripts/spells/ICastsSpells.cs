using Godot;

namespace Oubliette.Spells
{
    public interface ICastsSpells
    {
        int GetSpellDamage(int baseDamage);
        float GetSpellRange(float baseRange);
        float GetSpellKnockback(float baseKnockback);
        float GetSpellSpeed(float baseSpeed);
        Vector2 GetSpellDirection();
        Vector2 GetSpellSpawnPos();
        Color GetSpellColour(Color baseColour);
    }
}