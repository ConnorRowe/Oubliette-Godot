using Godot;
using System.Collections.Generic;
using Oubliette.Stats;

namespace Oubliette
{
    public class BuffTracker : CenterContainer
    {
        private int _charges = 0;
        public int Charges
        {
            get { return _charges; }
            set
            {
                SetCharges(value);
                _charges = value;
            }
        }

        public string sourceName = "";
        public HashSet<(Stat stat, float amount)> stats;
        public TextureRect ItemIcon { get; set; }
        private ShaderMaterial chargeGemsMat;

        public override void _Ready()
        {
            ItemIcon = GetNode<TextureRect>("ItemIcon");
            chargeGemsMat = (ShaderMaterial)(GetNode<TextureRect>("ChargeGems").Material);
        }

        public void Init(string sourceName, HashSet<(Stat stat, float amount)> stats, int duration)
        {
            ItemIcon = GetNode<TextureRect>("ItemIcon");
            chargeGemsMat = (ShaderMaterial)(GetNode<TextureRect>("ChargeGems").Material);


            this.sourceName = sourceName;
            this.stats = stats;
            Charges = duration;
        }

        private void SetCharges(int charges)
        {
            chargeGemsMat.SetShaderParam("charges_remaining", charges);
        }
    }
}