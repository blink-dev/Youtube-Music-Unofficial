using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Youtube_Music.ViewModels;

namespace Youtube_Music
{
    public class MusicPlayer : BindableBase, IDisposable
    {
        private float _volume;
        private float previousVolume;
        private bool _isMuted = false;
        private ISoundOut _soundOut;
        private IWaveSource _waveSource;

        private readonly DispatcherTimer _positionTimer = new() { Interval = TimeSpan.FromMilliseconds(20) };

        public event EventHandler<MediaOpenedEventArgs> MediaOpened;
        public event EventHandler<MediaEndedEventArgs> MediaEnded;
        public event EventHandler<PlaybackStateChangedEventArgs> PlaybackStateChanged;

        public PlaybackState PlaybackState
        {
            get => _soundOut != null ? _soundOut.PlaybackState : PlaybackState.Stopped;
        }

        public TimeSpan Position
        {
            get => _waveSource != null ? _waveSource.GetPosition() : TimeSpan.Zero;
            set
            {
                if (_waveSource != null)
                {
                    _waveSource.SetPosition(value);
                }
            }
        }

        public TimeSpan Length
        {
            get => _waveSource != null ? _waveSource.GetLength() : TimeSpan.Zero;
        }

        public float Volume
        {
            get => _volume;
            set
            {
                float vol = Math.Min(1.0f, Math.Max(value, 0f));
                if (_soundOut != null)
                    _soundOut.Volume = vol;
                Set(ref _volume, vol);
            }
        }

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                Set(ref _isMuted, value);
                if (_isMuted)
                {
                    previousVolume = _soundOut.Volume;
                    _soundOut.Volume = 0;
                }
                else _soundOut.Volume = previousVolume;
            }
        }

        public MusicPlayer()
        {
            _positionTimer.Tick += PositionTimer_Tick;
        }

        public async Task Open(string url, CancellationToken token = default)
        {
            CleanupPlayback();
            await Task.Run(() =>
            {
                _waveSource = CodecFactory.Instance.GetCodec(new Uri(url));
                if (token.IsCancellationRequested) return;
                //token.ThrowIfCancellationRequested();
                _soundOut = new DirectSoundOut(100, ThreadPriority.Lowest);
                _soundOut.Initialize(_waveSource);
                _soundOut.Volume = Volume;
                MediaOpened?.Invoke(this, new MediaOpenedEventArgs());
                OnPropertyChanged(nameof(Length));
                _positionTimer.Start();
                Play();
            }, token);
        }

        private void PositionTimer_Tick(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(Position));
            if (Length != TimeSpan.Zero && Position >= Length.Subtract(TimeSpan.FromMilliseconds(300)) && PlaybackState == PlaybackState.Stopped)
            {
                MediaEnded?.Invoke(this, new MediaEndedEventArgs());
                _positionTimer.Stop();
            }
        }

        public void Play()
        {
            if (_soundOut != null)
            {
                _soundOut.Play();
                PlaybackStateChanged?.Invoke(this, new PlaybackStateChangedEventArgs(_soundOut.PlaybackState));
            }
        }

        public void Pause()
        {
            if (_soundOut != null)
            {
                _soundOut.Pause();
                PlaybackStateChanged?.Invoke(this, new PlaybackStateChangedEventArgs(_soundOut.PlaybackState));
            }
        }

        public void Stop()
        {
            if (_soundOut != null)
            {
                _soundOut.Stop();
                PlaybackStateChanged?.Invoke(this, new PlaybackStateChangedEventArgs(_soundOut.PlaybackState));
            }
        }

        private void CleanupPlayback()
        {
            if (_soundOut != null)
            {
                _soundOut.Dispose();
                _soundOut = null;
            }
            if (_waveSource != null)
            {
                _waveSource.Dispose();
                _waveSource = null;
            }
        }

        public void Dispose()
        {
            CleanupPlayback();
            GC.SuppressFinalize(this);
        }
    }
}
