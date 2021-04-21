using Godot;
using System;

public class HealthContainer : Node2D
{
    private Texture _healthTex;
    private readonly int[] healthYOffsets = new int[3] { 0, -10, 10 };
    public int maxHealth = 0;
    public int currentHealth = 0;

    public override void _Ready()
    {
        _healthTex = GD.Load<Texture>("res://textures/health_container.png");
    }

    public void SetHealth(int currentHealth, int maxHealth)
    {
        this.maxHealth = maxHealth;
        this.currentHealth = currentHealth;

        Update();
    }

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) { currentHealth = maxHealth; }
        if (currentHealth < 0) { currentHealth = 0; }

        Update();
    }

    public override void _Draw()
    {
        base._Draw();

        int drawHealth = currentHealth;
        int emptyContainers = (maxHealth - currentHealth) / 4;

        int drawnContainers = 0;
        int xOffset = 0;

        do
        {
            int yOffset = healthYOffsets[drawnContainers % 3];

            if (drawHealth >= 4)
            {
                DrawTextureRectRegion(_healthTex, new Rect2(Position + new Vector2(xOffset * Scale.x, yOffset * Scale.y), new Vector2(16.0f * Scale.x, 16.0f * Scale.y)), new Rect2(0, 0, 16, 16));
                drawHealth -= 4;
                ++drawnContainers;

            }
            else if (drawHealth > 0)
            {
                DrawTextureRectRegion(_healthTex, new Rect2(Position + new Vector2(xOffset * Scale.x, yOffset * Scale.y), new Vector2(16.0f * Scale.x, 16.0f * Scale.y)), new Rect2((4 - drawHealth) * 16, 0, 16, 16));
                drawHealth = 0;
                ++drawnContainers;
            }

            if (drawnContainers % 3 < 2)
            {
                xOffset += 10;
            }

        } while (drawHealth > 0);

        for (int i = 0; i < emptyContainers; ++i)
        {
            int yOffset = healthYOffsets[drawnContainers % 3];

            DrawTextureRectRegion(_healthTex, new Rect2(Position + new Vector2(xOffset * Scale.x, yOffset * Scale.y), new Vector2(16.0f * Scale.x, 16.0f * Scale.y)), new Rect2(64, 0, 16, 16));
            ++drawnContainers;

            if (drawnContainers % 3 < 2)
            {
                xOffset += 10;
            }
        }

    }
}