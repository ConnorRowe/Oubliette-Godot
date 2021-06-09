using Godot;
using System;

namespace Oubliette.AI
{
    public class CastSpellOnceBehaviour : AIBehaviour
    {
        private BaseSpell spell;

        public CastSpellOnceBehaviour() { }

        public CastSpellOnceBehaviour(AIManager manager, BaseSpell spell, Func<TransitionTestResult>[] transitions) : base(manager, transitions)
        {
            this.spell = spell;
        }

        public override void OnBehaviourStart()
        {
            CastSpell();
        }

        public override void Process(float delta)
        {
            mgr.targetPosCache = mgr.lastTarget.GlobalPosition;
        }

        public override Vector2 Steer()
        {
            return Vector2.Zero;
        }

        public override void OnBehaviourEnd()
        {
        }

        public virtual void CastSpell()
        {
            spell.Cast(mgr.owner as ICastsSpells);
        }
    }
}