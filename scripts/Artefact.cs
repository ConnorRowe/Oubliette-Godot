using Godot;
using System;

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

    public static ArtefactTextureSet emptyTexSet = new ArtefactTextureSet(Vector2.Zero, null, null, null);

    public string Name { get; set; }
    public Texture Texture { get; set; }
    private Action<Player> playerPickUpAction { get; set; }
    public ArtefactTextureSet TextureSet;

    public Artefact(string name, Texture texture, Action<Player> playerPickUpAction, ArtefactTextureSet textureSet)
    {
        Name = name;
        Texture = texture;
        this.playerPickUpAction = playerPickUpAction;
        TextureSet = textureSet;
    }

    public void PlayerPickUp(Player player)
    {
        playerPickUpAction(player);

        if (TextureSet.IsValid())
            player.artefactTextureSets.Add(TextureSet);
    }
}
