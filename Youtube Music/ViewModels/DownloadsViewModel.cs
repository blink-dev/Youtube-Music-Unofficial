using System.Collections.ObjectModel;
using Youtube_Music.Models;

namespace Youtube_Music.ViewModels
{
    public class DownloadsViewModel : BindableBase
    {
        private bool _isEmpty = true;
        private ObservableCollection<DownloadItem> _downloadItems;

        public bool IsEmpty
        {
            get => _isEmpty;
            set => Set(ref _isEmpty, value);
        }

        public ObservableCollection<DownloadItem> DownloadItems
        {
            get => _downloadItems;
            set => Set(ref _downloadItems, value);
        }

        public DownloadsViewModel()
        {
            DownloadItems = new ObservableCollection<DownloadItem>();
            DownloadItems.CollectionChanged += DownloadItems_CollectionChanged;
        }

        private void DownloadItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsEmpty = DownloadItems.Count == 0;
        }
    }
}
