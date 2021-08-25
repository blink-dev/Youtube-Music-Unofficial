using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Youtube_Music.Models;
namespace Youtube_Music.Views
{
    public partial class Home : Page
    {
        public Home()
        {
            InitializeComponent();
        }

        private async void Video_Click(object sender, MouseButtonEventArgs e)
        {
            var v = sender as FrameworkElement;
            YoutubeVideoInfo yvi = v.DataContext as YoutubeVideoInfo;
            await App.ViewModel.OpenMedia(yvi, true);
        }
    }
}
