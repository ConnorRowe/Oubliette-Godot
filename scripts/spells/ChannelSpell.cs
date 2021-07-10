using Godot;
using System;

namespace Oubliette.Spells
{
    public class ChannelSpell : BaseSpell
    {
        public PackedScene channelSpellNodeScene { get; set; } = null;

        // in seconds
        public float TickRate { get; set; }
        private bool isChanneling = false;
        protected ICastsSpells sourceCache = null;
        private SceneTreeTimer tickTimer = null;
        protected Particles2D lastEffectNode = null;
        public Material FXParticle { get; set; } = null;
        public Action<Particles2D> InitParticles { get; set; } = null;
        public Action<Particles2D, ICastsSpells, float> ProcessParticles { get; set; } = null;

        public ChannelSpell() { }

        public ChannelSpell(string name, int damage, float range, float knockback, float speed, float majykaCost, Color baseColour, Texture icon, Action<Character> hitCharEvt) : base(name, damage, range, knockback, speed, majykaCost, baseColour, icon, hitCharEvt)
        {

        }

        public override void Cast(ICastsSpells source)
        {
            if (!isChanneling)
            {
                Character c = (source as Character);

                if (FXParticle != null && InitParticles != null)
                {
                    lastEffectNode = new Particles2D()
                    {
                        ProcessMaterial = FXParticle,
                    };

                    InitParticles.Invoke(lastEffectNode);

                    source.AddSpellEffectNode(lastEffectNode);
                }

                // if (channelSpellNodeScene is ChannelSpellBeam beam)
                // {
                //     beam = channelSpellNodeScene.Instance<ChannelSpellBeam>();
                //     c.AddChild(beam);
                //     beam.Position = Vector2.Zero;

                //     beam.Start(source, TickRate, Range);
                // }

                isChanneling = true;

                sourceCache = source;

                tickTimer = c.GetTree().CreateTimer(TickRate);
                tickTimer.Connect("timeout", this, nameof(ChannelTick));
            }
        }

        public override void Release(ICastsSpells source)
        {
            lastEffectNode?.QueueFree();

            isChanneling = false;

            if (tickTimer != null && tickTimer.IsConnected("timeout", this, nameof(ChannelTick)))
                tickTimer.Disconnect("timeout", this, nameof(ChannelTick));
        }

        public virtual void ChannelTick()
        {
            if (isChanneling)
            {
                tickTimer = (sourceCache as Character).GetTree().CreateTimer(TickRate);
                tickTimer.Connect("timeout", this, nameof(ChannelTick));
            }
        }

        public override void Process(ICastsSpells source, float delta)
        {
            base.Process(source, delta);

            if (isChanneling && ProcessParticles != null && FXParticle != null && lastEffectNode != null)
            {
                ProcessParticles.Invoke(lastEffectNode, source, delta);
            }
        }
    }
}