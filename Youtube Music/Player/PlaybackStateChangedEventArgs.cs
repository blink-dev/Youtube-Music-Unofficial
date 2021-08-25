using CSCore.SoundOut;
using System;

namespace Youtube_Music
{
    public class PlaybackStateChangedEventArgs : EventArgs
    {
        public PlaybackState PlaybackState { get; set; }

        public PlaybackStateChangedEventArgs(PlaybackState playbackState)
        {
            PlaybackState = playbackState;
        }
    }
}
