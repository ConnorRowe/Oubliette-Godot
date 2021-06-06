using Godot;
using System;

public class ArtefactNamePopup : Node2D
{
    private Label _artefactNameLabel;
    private Label _descLabel;
    private AnimationPlayer _animPlayer;

    public override void _Ready()
    {
        _artefactNameLabel = GetNode<Label>("Background/ArtefactName");
        _descLabel = GetNode<Label>("Background/ArtefactName/Description");
        _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public void DisplayPopup(string name, string desc)
    {
        _artefactNameLabel.Text = name;
        _descLabel.Text = desc;

        if (_animPlayer.CurrentAnimationPosition > 0.0f)
            _animPlayer.Seek(0.0f);

        _animPlayer.Play("ShowHide");
    }
}