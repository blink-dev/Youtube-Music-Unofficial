using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Youtube_Music.Models;

namespace Youtube_Music.Views
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class Search : Page
    {
        public Search()
        {
            InitializeComponent();
        }

        private void SearchBar_SuggestionChosen(object sender, RoutedEventArgs e)
        {
            AllFilter.IsChecked = true;
        }

        private async void Video_Click(object sender, MouseButtonEventArgs e)
        {
            var v = sender as FrameworkElement;
            YoutubeVideoInfo yvi = v.DataContext as YoutubeVideoInfo;
            await App.ViewModel.OpenMedia(yvi, true);
        }

    }
}
