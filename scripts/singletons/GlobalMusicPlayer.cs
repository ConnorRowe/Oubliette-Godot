using Godot;
using System.Collections.Generic;

namespace Oubliette
{
    public class GlobalMusicPlayer : AudioStreamPlayer
    {
        private static List<AudioStreamOGGVorbis> musicTracks = new List<AudioStreamOGGVorbis>();
        private static RandomNumberGenerator rng = new RandomNumberGenerator();

        static GlobalMusicPlayer()
        {
            LevelGen.LevelGenerator.LoadFromDirectory<AudioStreamOGGVorbis>("res://sound/music/", musicTracks);
            rng.Randomize();
        }

        public override void _Ready()
        {
            base._Ready();

            Connect("finished", this, nameof(Finished));

            PlayRandomTrack();
        }

        private void Finished()
        {
            PlayRandomTrack();
        }

        private void PlayRandomTrack()
        {
            Stream = musicTracks[rng.RandiRange(0, musicTracks.Count - 1)];

            Play();
        }
    }
}