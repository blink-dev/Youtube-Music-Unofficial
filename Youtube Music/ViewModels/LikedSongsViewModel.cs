using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Youtube_Music.Models;
using Youtube_Music.Commands;

namespace Youtube_Music.ViewModels
{
    public class LikedSongsViewModel : BindableBase
    {
        private bool _isEmpty = false;
        private bool _isLoading = false;
        private ObservableCollection<YoutubeVideoInfo> _songs = new();

        public bool IsEmpty
        {
            get => _isEmpty;
            set => Set(ref _isEmpty, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        public ObservableCollection<YoutubeVideoInfo> Songs
        {
            get => _songs;
            set => Set(ref _songs, value);
        }

        #region Commands

        public DelegateCommand<object> LikeCommand => new(async delegate
        {
            if (App.ViewModel.CurrentSongInfo is null) return;
            var like = Songs.Any(s => s.VideoId == App.ViewModel.CurrentSongInfo.VideoId);

            if (!like)
            {
                App.ViewModel.LikeUnlikeIcon = App.Current.Resources["LikeIcon"] as Geometry;
                await AddSongToLikedSongs(App.ViewModel.CurrentSongInfo);
                await Browse();
            }
            else
            {
                App.ViewModel.LikeUnlikeIcon = App.Current.Resources["LikeIconOutlined"] as Geometry;
                await RemoveSongToLikedSongs(App.ViewModel.CurrentSongInfo);
                await Browse();
            }
        });

        public DelegateCommand<object> UnlikeCommand => new(async sender =>
        {
            var item = Helpers.FindParent<ContextMenu>(sender as FrameworkElement).PlacementTarget as FrameworkElement;
            var song = item.DataContext as YoutubeVideoInfo;

            if (App.ViewModel.CurrentSongInfo != null && App.ViewModel.CurrentSongInfo.VideoId == song.VideoId) App.ViewModel.LikeUnlikeIcon = Application.Current.Resources["LikeIconOutlined"] as Geometry;

            await RemoveSongToLikedSongs(song);
            await Browse();
        });

        #endregion

        public LikedSongsViewModel()
        {
            Task.Run(Browse);
        }

        #region Methods

        public async Task Browse()
        {
            IsLoading = true;
            IsEmpty = false;
            var likedSongs = await GetLikedSongs();
            if (likedSongs is null)
            {
                Songs = new ObservableCollection<YoutubeVideoInfo>();
                IsLoading = false;
                IsEmpty = true;
                return;
            }
            Songs = new ObservableCollection<YoutubeVideoInfo>(likedSongs);
            IsLoading = false;
        }

        public static async Task AddSongToLikedSongs(YoutubeVideoInfo song)
        {
            var localFile = App.CacheService.LikedSongsPath;

            if (File.Exists(localFile))
            {
                var list = System.Text.Json.JsonSerializer.Deserialize<List<YoutubeVideoInfo>>(await System.IO.File.ReadAllTextAsync(localFile));
                if (list is null) list = new List<YoutubeVideoInfo>();
                if (list.Exists(s => s.VideoId == song.VideoId)) list.Remove(list.Where(s => s.VideoId == song.VideoId).First());
                list.Add(song);

                string json = System.Text.Json.JsonSerializer.Serialize(list);
                await System.IO.File.WriteAllTextAsync(localFile, json);
            }
        }

        public static async Task RemoveSongToLikedSongs(YoutubeVideoInfo song)
        {
            var localFile = App.CacheService.LikedSongsPath;

            if (File.Exists(localFile))
            {
                var list = System.Text.Json.JsonSerializer.Deserialize<List<YoutubeVideoInfo>>(await System.IO.File.ReadAllTextAsync(localFile));
                if (list is null) list = new List<YoutubeVideoInfo>();
                if (list.Exists(s => s.VideoId == song.VideoId)) list.Remove(list.Where(s => s.VideoId == song.VideoId).First());

                string json = System.Text.Json.JsonSerializer.Serialize(list);
                await System.IO.File.WriteAllTextAsync(localFile, json);
            }
        }


        public static async Task<YoutubeVideoInfo[]> GetLikedSongs()
        {
            try
            {
                var localFile = App.CacheService.LikedSongsPath;
                if (File.Exists(localFile))
                {
                    var listVideos = JsonConvert.DeserializeObject<YoutubeVideoInfo[]>(await File.ReadAllTextAsync(localFile));
                    if (listVideos != null && listVideos.Length > 0)
                    {
                        return listVideos.Reverse().ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return null;
        }

        #endregion

    }
}
