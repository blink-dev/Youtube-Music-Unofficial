using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Youtube_Music.Models;

namespace Youtube_Music.ViewModels
{
    public class NowPlayingViewModel : BindableBase
    {
        private bool _isLoading = false;

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        private ObservableCollection<YoutubeVideoInfo> _songs;

        public ObservableCollection<YoutubeVideoInfo> Songs
        {
            get => _songs;
            set => Set(ref _songs, value);
        }

        private YoutubeVideoInfo _selectedSong;

        public YoutubeVideoInfo SelectedSong
        {
            get => _selectedSong;
            set => Set(ref _selectedSong, value);
        }

        public async Task UpNext(YoutubeVideoInfo song)
        {
            if (Songs != null) Songs.Clear();
            IsLoading = true;
            var next = await App.MusicApi.UpNext(song.VideoId);
            Songs = new ObservableCollection<YoutubeVideoInfo>(next);
            IsLoading = false;
        }

        public async Task UpNextAdd(YoutubeVideoInfo song)
        {
            var next = await App.MusicApi.UpNext(song);
            if (next is null || next.Length == 0)
            {
                next = await App.MusicApi.UpNext(song.VideoId);
                next = next.Skip(1).ToArray();
            }
            foreach (var n in next) Songs.Add(n);
        }
    }
}
