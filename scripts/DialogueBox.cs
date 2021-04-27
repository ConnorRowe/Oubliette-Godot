using Godot;
using System;
using System.Collections.Generic;

public class DialogueBox : Control
{
    private Label nameLabel;
    private RichTextLabel dialogueLabel;
    private AudioStreamPlayer audioPlayer;
    private Tween tween;

    [Export]
    private NodePath _namePath;
    [Export]
    private NodePath _dialoguePath;
    [Export]
    private NodePath _audioPlayerPath;
    [Export]
    private NodePath _tweenPath;

    private float maxCharacters;
    private float currentCharacters = 0;

    // character name, dialogue
    public List<(string name, string dialogue)> queuedText = new List<(string name, string dialogue)>();

    // Sounds
    private AudioStreamSample ah;
    private AudioStreamSample buh;
    private AudioStreamSample duh;
    private AudioStreamSample eh;
    private AudioStreamSample fuh;
    private AudioStreamSample guh;
    private AudioStreamSample hih;
    private AudioStreamSample ih;
    private AudioStreamSample juh;
    private AudioStreamSample kuh;
    private AudioStreamSample nuh;
    private AudioStreamSample oh;
    private AudioStreamSample puh;
    private AudioStreamSample ruh;
    private AudioStreamSample suh;
    private AudioStreamSample yuh;

    public override void _Ready()
    {
        ah = GD.Load<AudioStreamSample>("res://sound/phonetic/ah.wav");
        buh = GD.Load<AudioStreamSample>("res://sound/phonetic/buh.wav");
        duh = GD.Load<AudioStreamSample>("res://sound/phonetic/duh.wav");
        eh = GD.Load<AudioStreamSample>("res://sound/phonetic/eh.wav");
        fuh = GD.Load<AudioStreamSample>("res://sound/phonetic/fuh.wav");
        guh = GD.Load<AudioStreamSample>("res://sound/phonetic/guh.wav");
        hih = GD.Load<AudioStreamSample>("res://sound/phonetic/hih.wav");
        ih = GD.Load<AudioStreamSample>("res://sound/phonetic/ih.wav");
        juh = GD.Load<AudioStreamSample>("res://sound/phonetic/juh.wav");
        kuh = GD.Load<AudioStreamSample>("res://sound/phonetic/kuh.wav");
        nuh = GD.Load<AudioStreamSample>("res://sound/phonetic/nuh.wav");
        oh = GD.Load<AudioStreamSample>("res://sound/phonetic/oh.wav");
        puh = GD.Load<AudioStreamSample>("res://sound/phonetic/puh.wav");
        ruh = GD.Load<AudioStreamSample>("res://sound/phonetic/ruh.wav");
        suh = GD.Load<AudioStreamSample>("res://sound/phonetic/suh.wav");
        yuh = GD.Load<AudioStreamSample>("res://sound/phonetic/yuh.wav");

        nameLabel = GetNode<Label>(_namePath);
        dialogueLabel = GetNode<RichTextLabel>(_dialoguePath);
        audioPlayer = GetNode<AudioStreamPlayer>(_audioPlayerPath);
        tween = GetNode<Tween>(_tweenPath);

        Visible = false;
        RectScale = new Vector2(0, 1);

        List<(string name, string dialogue)> testConvo = new List<(string name, string dialogue)> {
        ("Momo", "meow"),
        ("Jiji", "meep"),
        ("Momo", "i'm gonna find somethin to break"),
        ("Jiji", "cool, i'm gonna do a stinky poo"),
        ("Momo", "well, good to see we're both conducting important business. good day"),
        ("Jiji", "good day"),
        ("Momo", "[shake rate=10 level=8]*farts*[/shake]")};

        QueueText(testConvo);
        NextText();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        int startChars = dialogueLabel.VisibleCharacters;

        if (currentCharacters < maxCharacters)
        {
            currentCharacters += delta * 20.0f;
        }

        if (currentCharacters > maxCharacters)
        {
            currentCharacters = maxCharacters;
        }

        dialogueLabel.VisibleCharacters = Mathf.RoundToInt(currentCharacters);

        if (dialogueLabel.VisibleCharacters > startChars)
        {
            NewChar(dialogueLabel.Text[startChars]);
        }
    }

    public override void _Input(InputEvent evt)
    {
        base._Input(evt);

        if (evt is InputEventKey iek)
        {
            if (iek.Pressed && iek.Scancode == (uint)KeyList.Space)
            {
                if (currentCharacters < maxCharacters)
                {
                    currentCharacters = maxCharacters;
                    dialogueLabel.VisibleCharacters = Mathf.RoundToInt(currentCharacters);
                }
                else
                {
                    if (queuedText.Count > 0)
                    {
                        NextText();
                    }
                    else
                    {
                        ToggleDisplay(true);
                    }
                }
            }
        }
    }

    public void ToggleDisplay(bool hide)
    {
        tween.InterpolateProperty(this, "rect_scale", hide? new Vector2(1, 1) : new Vector2(0, 1), hide? new Vector2(0,1) : new Vector2(1, 1), 0.8f, Tween.TransitionType.Bounce);

        if(hide)
            tween.InterpolateProperty(this, "visible", true, false, 0.01f, delay: 0.8f);
        else
            Visible = true;

        tween.Start();
    }

    public void QueueText(List<(string name, string dialogue)> text)
    {
        foreach ((string name, string dialogue) p in text)
        {
            queuedText.Add(p);
        }

        ToggleDisplay(false);
    }

    public void NextText()
    {
        var t = queuedText[0];

        nameLabel.Text = t.name;
        dialogueLabel.BbcodeText = t.dialogue;

        CallDeferred(nameof(UpdateMaxCharacters));
        dialogueLabel.VisibleCharacters = 0;
        currentCharacters = 0;

        queuedText.RemoveAt(0);
    }

    private void UpdateMaxCharacters()
    {
        maxCharacters = dialogueLabel.GetTotalCharacterCount();
    }

    private void NewChar(char c)
    {
        AudioStreamSample charSound = GetCharSound(c);

        if (charSound != null)
        {
            audioPlayer.Stream = charSound;
            audioPlayer.Play(0);
        }
    }

    private AudioStreamSample GetCharSound(char c)
    {
        switch (Char.ToLower(c))
        {
            case 'a':
            case 'u':
                return ah;
            case 'b':
                return buh;
            case 'd':
            case 't':
                return duh;
            case 'e':
                return eh;
            case 'f':
                return fuh;
            case 'g':
                return guh;
            case 'h':
                return hih;
            case 'i':
                return ih;
            case 'j':
                return juh;
            case 'k':
            case 'c':
            case 'q':
            case 'x':
                return kuh;
            case 'n':
            case 'm':
                return nuh;
            case 'o':
                return oh;
            case 'p':
                return puh;
            case 'r':
            case 'w':
            case 'l':
            case 'v':
                return ruh;
            case 's':
            case 'z':
                return suh;
            case 'y':
                return yuh;
        }

        return null;
    }
}
