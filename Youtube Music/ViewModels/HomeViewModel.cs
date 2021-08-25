using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Youtube_Music.Models;
using Youtube_Music.Commands;

namespace Youtube_Music.ViewModels
{
    public class HomeViewModel : BindableBase
    {
        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        private ObservableCollection<SearchShelf> _shelfs;
        public ObservableCollection<SearchShelf> Shelfs
        {
            get => _shelfs;
            set => Set(ref _shelfs, value);
        }

        public DelegateCommand ReloadCommand { get; set; }

        public HomeViewModel()
        {
            ReloadCommand = new DelegateCommand(async () => await Browse());
            Task.Run(Browse);
        }

        public async Task Browse()
        {
            if (Shelfs != null) Shelfs.Clear();
            IsLoading = true;
            SearchShelf quickPicksShelf = new() { Title = "Quick Picks" };
            var recentlyPlayedShelf = await RecentlyPlayed();
            if (recentlyPlayedShelf != null)
            {
                var random = new Random().Next(0, recentlyPlayedShelf.Videos.Length);
                quickPicksShelf.Videos = await App.MusicApi.UpNext(recentlyPlayedShelf.Videos[random].VideoId, true);
            }
            else
            {
                var bID = await App.MusicApi.Browse("FEmusic_home", "browseId", 1, 3);
                var videoId = await App.MusicApi.Browse(bID, "videoId", 0, 1);
                var songs = await App.MusicApi.UpNext(videoId);
                quickPicksShelf.Videos = songs;
            }
            Shelfs = new ObservableCollection<SearchShelf>();
            if (recentlyPlayedShelf != null) Shelfs.Add(recentlyPlayedShelf);
            Shelfs.Add(quickPicksShelf);
            IsLoading = false;
        }

        public static async Task<SearchShelf> RecentlyPlayed()
        {
            try
            {
                var localFile = App.CacheService.RecentlyPlayedPath;
                if (File.Exists(localFile))
                {
                    var listVideos = System.Text.Json.JsonSerializer.Deserialize<YoutubeVideoInfo[]>(await File.ReadAllTextAsync(localFile));
                    if (listVideos != null && listVideos.Length > 0)
                    {
                        SearchShelf recentlyPlayedShelf = new()
                        {
                            Title = "Recently Played",
                            Videos = listVideos.Reverse().ToArray()
                        };
                        return recentlyPlayedShelf;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return null;
        }
    }
}
