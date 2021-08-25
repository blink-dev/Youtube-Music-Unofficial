using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Youtube_Music.Models;
using Youtube_Music.Commands;
using YoutubeExplode.Videos.Streams;

namespace Youtube_Music.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private Geometry _playPauseIcon;
        private Geometry _muteIcon;
        private Geometry _likeUnlikeIcon;
        private bool _isMuted;
        private YoutubeVideoInfo _currentSongInfo;

        public MusicPlayer MusicPlayer { get; }

        public Geometry PlayPauseIcon
        {
            get => _playPauseIcon;
            set => Set(ref _playPauseIcon, value);
        }

        public Geometry MuteIcon
        {
            get => _muteIcon;
            set => Set(ref _muteIcon, value);
        }

        public Geometry LikeUnlikeIcon
        {
            get => _likeUnlikeIcon;
            set => Set(ref _likeUnlikeIcon, value);
        }

        public bool IsMuted
        {
            get => _isMuted;
            set => Set(ref _isMuted, value);
        }

        public YoutubeVideoInfo CurrentSongInfo
        {
            get => _currentSongInfo;
            set => Set(ref _currentSongInfo, value);
        }

        #region Viewmodels

        public NowPlayingViewModel NowPlaying { get; set; }
        public HomeViewModel Home { get; set; }
        public LikedSongsViewModel LikedSongs { get; set; }
        public DownloadsViewModel Downloads { get; set; }
        public SearchViewModel Search { get; set; }

        #endregion

        #region Commands

        public static DelegateCommand<object> NavigateCommand => new(obj => App.AppFrame.Navigate(obj));

        public DelegateCommand StartPlaybackCommand => new(delegate
        {
            if (MusicPlayer.PlaybackState != CSCore.SoundOut.PlaybackState.Playing) MusicPlayer.Play();
            else MusicPlayer.Pause();
        });

        public DelegateCommand PreviousSongCommand => new(async delegate
        {
            if (NowPlaying.SelectedSong is null) return;
            var currentIndex = NowPlaying.Songs.IndexOf(NowPlaying.SelectedSong) - 1;
            if (currentIndex.Equals(-1))
            {
                await OpenMedia(NowPlaying.SelectedSong);
                return;
            }
            NowPlaying.SelectedSong = NowPlaying.Songs[currentIndex];
            await OpenMedia(NowPlaying.SelectedSong);
        });

        public DelegateCommand NextSongCommand => new(async delegate
        {
            if (NowPlaying.SelectedSong is null) return;
            var currentIndex = NowPlaying.Songs.IndexOf(NowPlaying.SelectedSong) + 1;
            if (currentIndex >= NowPlaying.Songs.Count - 1)
            {
                await NowPlaying.UpNextAdd(NowPlaying.SelectedSong);
                NowPlaying.SelectedSong = NowPlaying.Songs[currentIndex + 1];
                await OpenMedia(NowPlaying.SelectedSong);
                return;
            }
            NowPlaying.SelectedSong = NowPlaying.Songs[currentIndex];
            await OpenMedia(NowPlaying.SelectedSong);
        });

        public DelegateCommand MuteCommand => new(delegate
        {
            if (MusicPlayer.IsMuted)
            {
                MusicPlayer.IsMuted = !MusicPlayer.IsMuted;
                MuteIcon = Application.Current.Resources["UnmuteIcon"] as Geometry;
            }
            else
            {
                MusicPlayer.IsMuted = !MusicPlayer.IsMuted;
                MuteIcon = Application.Current.Resources["MuteIcon"] as Geometry;
            }
        });

        public static DelegateCommand OpenMediaGridCommand => new(delegate
        {
            if (App.AppFrame.Height < 393)
            {
                var hide = App.Current.MainWindow.Resources["MediaGridHide"] as Storyboard;
                hide.Begin();
            }
            else
            {
                var show = App.Current.MainWindow.Resources["MediaGridShow"] as Storyboard;
                show.Begin();
            }
        });

        public DelegateCommand<object> DownloadCommand => new(sender =>
        {
            YoutubeVideoInfo song;
            if (sender is null)
                song = CurrentSongInfo;
            else
            {
                var item = Helpers.FindParent<ContextMenu>(sender as FrameworkElement).PlacementTarget as FrameworkElement;
                song = item.DataContext as YoutubeVideoInfo;
            }
            string fulltitle = $"{song.Artist} - {song.Title}";
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "Audio file (.mp3)|*.mp3",
                FileName = fulltitle,
                AddExtension = false
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                DownloadItem downloadItem = new(fulltitle, saveFileDialog.FileName, song.VideoId);
                Downloads.DownloadItems.Add(downloadItem);
            }
        });

        public static DelegateCommand<object> CopyLinkCommand => new(sender =>
        {
            var item = Helpers.FindParent<ContextMenu>(sender as FrameworkElement).PlacementTarget as FrameworkElement;
            var song = item.DataContext as YoutubeVideoInfo;
            Clipboard.SetText($"https://youtu.be/{song.VideoId}");
        });

        public DelegateCommand<object> GeneratePlaylistCommand => new(async sender =>
        {
            var item = Helpers.FindParent<ContextMenu>(sender as FrameworkElement).PlacementTarget as FrameworkElement;
            var song = item.DataContext as YoutubeVideoInfo;
            await OpenMedia(song, true);
        });

        #endregion


        public MainViewModel()
        {
            NowPlaying = new NowPlayingViewModel();
            Home = new HomeViewModel();
            LikedSongs = new LikedSongsViewModel();
            Downloads = new DownloadsViewModel();
            Search = new SearchViewModel();

            PlayPauseIcon = Geometry.Empty;
            IsMuted = false;
            MuteIcon = Application.Current.Resources["UnmuteIcon"] as Geometry;
            LikeUnlikeIcon = Application.Current.Resources["LikeIconOutlined"] as Geometry;
            MusicPlayer = new();
            MusicPlayer.PlaybackStateChanged += MusicPlayer_PlaybackStateChanged;
            MusicPlayer.MediaEnded += (s, e) => NextSongCommand.Execute();
        }

        #region Methods & Events

        public static async Task AddSongToRecentlyPlayed(YoutubeVideoInfo song)
        {
            var localFile = App.CacheService.RecentlyPlayedPath;

            if (System.IO.File.Exists(localFile))
            {
                var list = System.Text.Json.JsonSerializer.Deserialize<List<YoutubeVideoInfo>>(await System.IO.File.ReadAllTextAsync(localFile));
                if (list is null) list = new List<YoutubeVideoInfo>();
                if (list.Exists(s => s.VideoId == song.VideoId)) list.Remove(list.Where(s => s.VideoId == song.VideoId).First());
                list.Add(song);
                if (list.Count > 50)
                {
                    var rcount = list.Count - 50;
                    list.RemoveRange(0, rcount);
                }

                string json = System.Text.Json.JsonSerializer.Serialize(list);
                await System.IO.File.WriteAllTextAsync(localFile, json);
            }
        }

        private CancellationTokenSource cts;

        public async Task OpenMedia(YoutubeVideoInfo song, bool generatePlaylist = false, bool likedSongs = false)
        {
            if(cts is null)
            {
                cts = new();
                try
                {
                    await InitMedia(song, cts.Token, generatePlaylist, likedSongs);
                }
                catch (OperationCanceledException) { }
                return;
            } 
            else
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
                cts = new();
                try
                {
                    await InitMedia(song, cts.Token, generatePlaylist, likedSongs);
                }
                catch (OperationCanceledException) { }
            }
        }

        public async Task InitMedia(YoutubeVideoInfo song, CancellationToken token, bool generatePlaylist = false, bool likedSongs = false)
        {
            if (MusicPlayer.PlaybackState == CSCore.SoundOut.PlaybackState.Stopped)
            {
                CurrentSongInfo = song;
            }

            bool like = LikedSongs.Songs.Any(s => s.VideoId == song.VideoId);
            LikeUnlikeIcon = !like ? Application.Current.Resources["LikeIconOutlined"] as Geometry : Application.Current.Resources["LikeIcon"] as Geometry;

            App.WindowMain.PlayButtonLoading(true);
            App.WindowMain.ShowHideMediaGrid();

            YoutubeExplode.YoutubeClient youtubeClient = new();
            StreamManifest streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(song.VideoId, token);
            IStreamInfo streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            if (token.IsCancellationRequested) return;
            await MusicPlayer.Open(streamInfo.Url, token);

            CurrentSongInfo = song;

            App.WindowMain.ShowHideMediaGrid();

            App.WindowMain.PlayButtonLoading(false);

            await AddSongToRecentlyPlayed(song);

            if (generatePlaylist)
            {
                await NowPlaying.UpNext(song);
                NowPlaying.SelectedSong = NowPlaying.Songs[0];
            }

            if (likedSongs)
            {
                NowPlaying.Songs = new(LikedSongs.Songs);
                App.ViewModel.NowPlaying.SelectedSong = song;
                await Task.Delay(100, token);
                App.WindowMain.NowPlayingList.ScrollToPlayingSong();
            }

            cts.Dispose();
            cts = null;
        }

        private void MusicPlayer_PlaybackStateChanged(object sender, PlaybackStateChangedEventArgs e)
        {
            if (e.PlaybackState == CSCore.SoundOut.PlaybackState.Playing) PlayPauseIcon = Application.Current.Resources["PauseIcon"] as Geometry;
            else PlayPauseIcon = Application.Current.Resources["PlayIcon"] as Geometry;
        }

        public void ShiftVolume(double delta)
        {
            if (delta > 0)
                MusicPlayer.Volume += MusicPlayer.Volume < 0.05f ? 0.01f : 0.05f;
            else
                MusicPlayer.Volume -= MusicPlayer.Volume < 0.05f ? 0.01f : 0.05f;
        }

        #endregion

    }
}
