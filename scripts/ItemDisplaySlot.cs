using Godot;
using System;

public class ItemDisplaySlot : Sprite
{
    private Label nameLabel;
    private string _itemName = "";

    public Label NameLabel
    {
        get
        {
            if (nameLabel == null)
                nameLabel = GetNode<Label>("Label");

            return nameLabel;
        }
    }

    public string ItemName
    {
        get
        {
            return _itemName;
        }
        set
        {
            if (_itemName != value)
            {
                _itemName = value;
                NameLabel.Text = _itemName;
            }
        }
    }

    public override void _Ready()
    {
        base._Ready();

        Area2D mouseOverArea = GetNode<Area2D>("MouseOverArea");

        mouseOverArea.Connect("mouse_entered", this, nameof(SetLabelVisibility), new Godot.Collections.Array() { true });
        mouseOverArea.Connect("mouse_exited", this, nameof(SetLabelVisibility), new Godot.Collections.Array() { false });

        SetLabelVisibility(false);
    }

    public void SetItemTexture(Texture texture)
    {
        Texture = texture;
    }

    public void SetItemMaterial(Material material)
    {
        Material = material;
    }

    private void SetLabelVisibility(bool isVisible)
    {
        if (isVisible)
        {
            NameLabel.Show();
        }
        else
        {
            NameLabel.Hide();
        }
    }
}
