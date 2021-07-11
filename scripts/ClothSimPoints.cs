using Godot;

public class ClothSimPoints : Node2D
{
    [Export]
    public Godot.Collections.Array<NodePath> bound_points = new Godot.Collections.Array<NodePath>();

    public int Index { get; set; }
}
