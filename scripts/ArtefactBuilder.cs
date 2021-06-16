using Godot;
using System;

namespace Oubliette
{
    public class ArtefactBuilder : Reference
    {
        private string name;
        private string description;
        private Texture icon;
        private Action<Player> playerPickupAction;
        private Action<Player> onPlayerDamaged;
        private Artefact.ArtefactTextureSet textureSet = Artefact.EmptyTexSet;
        private float rarityWeight;

        public ArtefactBuilder SetName(string name)
        {
            this.name = name;

            return this;
        }

        public ArtefactBuilder SetDescription(string description)
        {
            this.description = description;

            return this;
        }

        public ArtefactBuilder SetIcon(Texture icon)
        {
            this.icon = icon;

            return this;
        }

        public ArtefactBuilder SetPlayerPickUpAction(Action<Player> playerPickupAction)
        {
            this.playerPickupAction = playerPickupAction;

            return this;
        }

        public ArtefactBuilder SetOnPlayerDamaged(Action<Player> onPlayerDamaged)
        {
            this.onPlayerDamaged = onPlayerDamaged;

            return this;
        }

        public ArtefactBuilder SetTextureSet(Artefact.ArtefactTextureSet textureSet)
        {
            this.textureSet = textureSet;

            return this;
        }

        public ArtefactBuilder SetRarityWeight(float rarityWeight)
        {
            this.rarityWeight = rarityWeight;

            return this;
        }

        public Artefact Build()
        {
            Artefact build = new Artefact(name, description, rarityWeight, icon, playerPickupAction, textureSet);
            build.OnPlayerDamaged = onPlayerDamaged;

            return build;
        }
    }
}