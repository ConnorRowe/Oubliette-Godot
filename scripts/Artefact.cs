using Godot;
using System;

namespace Oubliette
{
    public class Artefact : Reference
    {
        public struct ArtefactTextureSet
        {
            public readonly Vector2 Offset;
            public readonly Texture Up;
            public readonly Texture Down;
            public readonly Texture LeftRight;

            public ArtefactTextureSet(Vector2 offset, Texture up, Texture down, Texture leftRight)
            {
                this.Offset = offset;
                this.Up = up;
                this.Down = down;
                this.LeftRight = leftRight;
            }

            public Texture TextureFromDirection(Direction direction)
            {
                switch (direction)
                {
                    case Direction.Up:
                        return Up;
                    case Direction.Left:
                    case Direction.Right:
                        return LeftRight;
                    case Direction.Down:
                    default:
                        return Down;
                }
            }

            public bool IsValid()
            {
                return Up != null || Down != null || LeftRight != null;
            }
        }

        public static ArtefactTextureSet EmptyTexSet { get; } = new ArtefactTextureSet(Vector2.Zero, null, null, null);

        public string Name { get; set; }
        public string Description { get; set; }
        public Texture Icon { get; set; }
        public Action<Player> PlayerPickUpAction { get; set; }
        public Action<Player> OnPlayerDamaged { get; set; }
        public ArtefactTextureSet TextureSet { get; set; } = EmptyTexSet;
        public float RarityWeight { get; set; }

        private Player player;

        public Artefact(string name, string desc, float rarityWeight, Texture texture, Action<Player> playerPickUpAction, ArtefactTextureSet textureSet)
        {
            Name = name;
            Description = desc;
            Icon = texture;
            this.PlayerPickUpAction = playerPickUpAction;
            TextureSet = textureSet;
            RarityWeight = rarityWeight;
        }

        public void PlayerPickUp(Player player)
        {
            this.player = player;

            PlayerPickUpAction?.Invoke(player);

            if (TextureSet.IsValid())
                player.artefactTextureSets.Add(TextureSet);

            player.PickedUpArtefact(this);

            player.Connect(nameof(Player.PlayerDamaged), this, nameof(PlayerDamaged));
        }

        private void PlayerDamaged(int damage)
        {
            OnPlayerDamaged?.Invoke(player);
        }
    }
}