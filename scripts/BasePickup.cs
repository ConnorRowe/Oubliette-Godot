using Godot;

public class BasePickup : RigidBody2D, IIntersectsPlayerHitArea
{
    public override void _Ready()
    {
        base._Ready();

        Connect("body_entered", this, nameof(BodyEntered));
    }

    private void BodyEntered(Node body)
    {
        if (body is Player player)
        {
            PlayerHit(player);
        }
    }

    void IIntersectsPlayerHitArea.PlayerHit(Player player)
    {
        PlayerHit(player);
    }

    public virtual void PlayerHit(Player player)
    {

    }
}
