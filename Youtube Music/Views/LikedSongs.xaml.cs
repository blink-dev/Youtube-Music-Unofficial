using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Youtube_Music.Models;

namespace Youtube_Music.Views
{
    public partial class LikedSongs : Page
    {
        public LikedSongs()
        {
            InitializeComponent();
        }

        private async void Video_Click(object sender, MouseButtonEventArgs e)
        {
            var v = sender as FrameworkElement;
            YoutubeVideoInfo yvi = v.DataContext as YoutubeVideoInfo;
            await App.ViewModel.OpenMedia(yvi, false, true);
        }

    }
}
