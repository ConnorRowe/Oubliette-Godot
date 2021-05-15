using Godot;
using System;

public class Artefact : Reference
{
    public string Name { get; set; }
    public Texture Texture { get; set; }
    public Action<Player> PlayerPickUpAction { get; set; }

    public Artefact(string name, Texture texture, Action<Player> playerPickUpAction)
    {
        Name = name;
        Texture = texture;
        PlayerPickUpAction = playerPickUpAction;
    }
}
