using Godot;
using System.Collections.Generic;


// This is used to draw an overlay of characters when they are obscured behind walls
public class OverlayRender : Node2D
{
    private HashSet<Character> characters = new HashSet<Character>();

    public World world;

    public override void _Ready()
    {
        base._Ready();

        world = GetParent().GetParent<World>();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        Update();
    }

    public override void _Draw()
    {
        base._Draw();

        foreach (Character c in characters)
        {
            SpriteFrames spriteFrames = c.CharSprite.Frames;
            AtlasTexture tex = (AtlasTexture)spriteFrames.GetFrame(c.CharSprite.Animation, c.CharSprite.Frame);

            // Position is centre of texture + the difference between the char's global pos and the player's pos, offset by the half texture size to centre it
            Vector2 pos = new Vector2(512, 300 + c.Elevation) + (c.GlobalPosition - world.Player.Position) - (tex.Region.Size * 0.5f) + c.CharSprite.Offset + c.CharSprite.Position;
            float flipH = c.CharSprite.FlipH ? -1.0f : 1.0f;

            DrawTextureRect(tex, new Rect2(pos, tex.Region.Size * new Vector2(flipH, 1.0f)), false);
        }
    }

    public void AddCharacter(Character character)
    {
        characters.Add(character);
    }

    public void RemoveCharacter(Character character)
    {
        characters.Remove(character);
    }

    public void AddCharacters(HashSet<Character> characters)
    {
        this.characters.UnionWith(characters);
    }

    public void ClearCharacters()
    {
        characters.Clear();
    }
}
