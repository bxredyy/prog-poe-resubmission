// VoiceService.cs
// Plays the welcome WAV file when the app starts.
// Wrapped in try/catch so the app still opens even if the audio file is missing.
// POE Part 1: Voice Greeting.

using System;
using System.IO;
using System.Media;
using System.Windows;

namespace CyberSecurityBot.Services
{
    public class VoiceService
    {
        private SoundPlayer? _player;

        public void PlayGreeting()
        {
            try
            {
                var uri = new Uri("pack://application:,,,/Assets/greeting.wav", UriKind.Absolute);
                var info = Application.GetResourceStream(uri);
                if (info == null) return;

                using var ms = new MemoryStream();
                info.Stream.CopyTo(ms);
                ms.Position = 0;

                _player = new SoundPlayer(ms);
                _player.Play();
            }
            catch
            {
                // Audio is a nice-to-have; never crash the app over it.
            }
        }
    }
}
