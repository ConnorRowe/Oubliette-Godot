using Godot;
using System.Collections.Generic;

namespace Oubliette
{
    public class BearTrap : Area2D
    {
        private AnimationPlayer animationPlayer;
        private World world;

        public override void _Ready()
        {
            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            animationPlayer.Connect("animation_finished", this, nameof(AnimationFinished));
            Connect("area_entered", this, nameof(AreaEntered));
        }

        private void AreaEntered(Area2D area)
        {
            if (area.Owner is Player player)
            {
                HitPlayer(player);

                Disconnect("area_entered", this, nameof(AreaEntered));
            }
        }

        private void AnimationFinished(string animName)
        {
            QueueFree();
        }

        private void HitPlayer(Player player)
        {
            player.TakeDamage(1, null, "a Bear Trap");
            player.ApplyTimedBuff(Stats.Buffs.CreateBuff("bear_trap", new List<(Stats.Stat stat, float amount)>() { (Stats.Stat.MoveSpeedMultiplier, -0.5f) }, 3.0f));

            animationPlayer.Play("HitPlayer");

            if (world != null)
            {
                world.SpawnBloodPool(GlobalPosition, Player.PlayerBloodColour).SpeedScale = 2.0f;
                for (int i = 0; i < world.rng.RandiRange(2, 3); ++i)
                {
                    world.SpawnBloodSplat(GlobalPosition, Player.PlayerBloodColour);
                }
            }
        }

        public void SetWorld(World world)
        {
            this.world = world;
        }
    }
}