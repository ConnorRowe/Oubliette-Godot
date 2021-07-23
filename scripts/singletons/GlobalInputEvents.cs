using Godot;

namespace Oubliette
{
    public class GlobalInputEvents : Node
    {
        public override void _Input(InputEvent evt)
        {
            base._Input(evt);

            if (evt.IsActionReleased("toggle_fullscreen"))
            {
                OS.WindowFullscreen = !OS.WindowFullscreen;
            }
        }
    }
}