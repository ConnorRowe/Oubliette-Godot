using Godot;

namespace Oubliette
{
    public class MainMenu : Control
    {
        private MainMenuButton playButton;
        private MainMenuButton quitButton;

        [Export]
        private PackedScene nextScene;

        public override void _Ready()
        {
            playButton = GetNode<MainMenuButton>("PlayButton");
            quitButton = GetNode<MainMenuButton>("QuitButton");

            playButton.Connect(nameof(MainMenuButton.Clicked), this, nameof(Play));
            quitButton.Connect(nameof(MainMenuButton.Clicked), this, nameof(Quit));
        }

        private void Play()
        {
            GetTree().ChangeSceneTo(nextScene);

            QueueFree();
        }

        private void Quit()
        {
            GetTree().Notification(MainLoop.NotificationWmQuitRequest);
        }
    }
}