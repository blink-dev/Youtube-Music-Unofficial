using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Youtube_Music.Models;
using Youtube_Music.Commands;

namespace Youtube_Music.ViewModels
{
    public class SearchViewModel : BindableBase
    {
        private bool _isLoading = false;

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        private ObservableCollection<string> _searchSuggestions;
        public ObservableCollection<string> SearchSuggestions
        {
            get => _searchSuggestions;
            set => Set(ref _searchSuggestions, value);
        }

        private ObservableCollection<YoutubeVideoInfo> _songs;
        public ObservableCollection<YoutubeVideoInfo> Songs
        {
            get => _songs;
            set => Set(ref _songs, value);
        }

        private ObservableCollection<SearchShelf> _shelfs = new();
        public ObservableCollection<SearchShelf> Shelfs
        {
            get => _shelfs;
            set => Set(ref _shelfs, value);
        }

        private string _query;
        public string Query
        {
            get => _query;
            set
            {
                Set(ref _query, value);
                App.Current.MainWindow.Dispatcher.InvokeAsync(SearchTextChanged);
            }
        }

        public DelegateCommand<string> QueryChangedCommand { get; set; }
        public DelegateCommand SuggestionChosenCommand { get; set; }
        public DelegateCommand AllCheckedCommand { get; set; }
        public DelegateCommand SongsCheckedCommand { get; set; }
        public DelegateCommand VideosCheckedCommand { get; set; }

        public SearchViewModel()
        {
            QueryChangedCommand = new DelegateCommand<string>(QueryChanged);
            SuggestionChosenCommand = new DelegateCommand(SuggestionChosen);
            AllCheckedCommand = new DelegateCommand(AllChecked);
            SongsCheckedCommand = new DelegateCommand(SongsChecked);
            VideosCheckedCommand = new DelegateCommand(VideosChecked);
        }

        private async void QueryChanged(string query)
        {
            Query = query;
            await LoadSearchSuggestionsAsync(Query);
            if (string.IsNullOrEmpty(Query)) SearchSuggestions = null;
        }

        private async void SuggestionChosen()
        {
            await SearchVideos(Query, Services.YTMusicApi.FilterType.All);
        }

        private async void SearchTextChanged()
        {
            await LoadSearchSuggestionsAsync(Query);
        }

        private async void AllChecked()
        {
            if (!string.IsNullOrEmpty(Query))
                await SearchVideos(Query, Services.YTMusicApi.FilterType.All);
        }

        private async void SongsChecked()
        {
            if (!string.IsNullOrEmpty(Query))
                await SearchVideos(Query, Services.YTMusicApi.FilterType.Songs);
        }

        private async void VideosChecked()
        {
            if (!string.IsNullOrEmpty(Query))
                await SearchVideos(Query, Services.YTMusicApi.FilterType.Videos);
        }

        public async Task LoadSearchSuggestionsAsync(string query)
        {
            var suggestions = await App.MusicApi.GetSearchSuggestions(System.Web.HttpUtility.UrlEncode(query));
            SearchSuggestions = new(suggestions);
        }

        public async Task SearchVideos(string query, Services.YTMusicApi.FilterType filter)
        {
            IsLoading = true;
            var search = await App.MusicApi.Search(query, filter);
            Shelfs = new ObservableCollection<SearchShelf>(search);
            IsLoading = false;
        }
    }
}
