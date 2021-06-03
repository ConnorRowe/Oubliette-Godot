using Godot;
using System;

public class MainMenuButton : Node2D
{
    [Signal]
    public delegate void Clicked();

    [Export]
    private Texture mouseOverText;
    [Export]
    private Texture normalText;
    [Export]
    private Texture borderOverride;
    [Export]
    private NodePath _cameraPath;


    private Sprite bgAndText;
    private Sprite border;
    private Camera2D camera;

    private ShaderMaterial bgAndTextMat;
    private bool isMouseOver = false;

    private Vector2 bgMaxOffset = new Vector2(2.0f, 2.0f);
    private Vector2 fgMaxOffset = new Vector2(3.0f, 3.0f);
    private Vector2 parallaxMaxDistance = new Vector2(96.0f, 64f);

    private Vector2 bgOffsetGoal = Vector2.Zero;
    private Vector2 fgOffsetGoal = Vector2.Zero;

    private Vector2 defaultBgClipMin;
    private Vector2 defaultBgClipMax;

    public bool Active = true;

    public override void _Ready()
    {
        bgAndText = GetNode<Sprite>("BgAndText");
        border = GetNode<Sprite>("Border");
        camera = GetNode<Camera2D>(_cameraPath);

        bgAndTextMat = bgAndText.Material as ShaderMaterial;
        defaultBgClipMin = (Vector2)bgAndTextMat.GetShaderParam("clip_rect_min");
        defaultBgClipMax = (Vector2)bgAndTextMat.GetShaderParam("clip_rect_max");

        if(borderOverride != null)
        {
            border.Texture = borderOverride;
        }

        Connect("mouse_entered", this, nameof(MouseOver), new Godot.Collections.Array() { true });
        Connect("mouse_exited", this, nameof(MouseOver), new Godot.Collections.Array() { false });

        MouseOver(false);
    }

    public override void _Input(InputEvent evt)
    {
        base._Input(evt);

        if(!Active)
            return;

        if (evt is InputEventMouseButton emb)
        {
            if (isMouseOver && emb.ButtonIndex == (int)ButtonList.Left)
            {
                if (!emb.IsPressed())
                {
                    GetTree().SetInputAsHandled();

                    EmitSignal(nameof(Clicked));
                }
            }
        }

        if (evt is InputEventMouseMotion emm)
        {
            UpdateParallax();
        }
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if(!Active)
            return;

        bgAndText.Offset = bgAndText.Offset.LinearInterpolate(bgOffsetGoal, delta * 4);
        border.Offset = border.Offset.LinearInterpolate(fgOffsetGoal, delta * 8);

        bgAndTextMat.SetShaderParam("clip_rect_min", defaultBgClipMin + border.Offset);
        bgAndTextMat.SetShaderParam("clip_rect_max", defaultBgClipMax + border.Offset);
    }

    private void MouseOver(bool isOver)
    {
        isMouseOver = isOver;

        bgAndTextMat.SetShaderParam("text_sampler", isOver ? mouseOverText : normalText);
    }

    private void UpdateParallax()
    {
        Vector2 offset = GetLocalMousePosition();

        offset = new Vector2(Mathf.Clamp(offset.x, -parallaxMaxDistance.x, parallaxMaxDistance.x), Mathf.Clamp(offset.y, -parallaxMaxDistance.y, parallaxMaxDistance.y));

        offset /= parallaxMaxDistance;

        bgOffsetGoal = -offset * bgMaxOffset;
        fgOffsetGoal = offset * fgMaxOffset;
    }
}
