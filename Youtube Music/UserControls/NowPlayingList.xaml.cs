using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Youtube_Music.Controls;
using Youtube_Music.Models;

namespace Youtube_Music.UserControls
{
    public partial class NowPlayingList : UserControl
    {
        public NowPlayingList()
        {
            InitializeComponent();
        }

        private async void Video_Click(object sender, MouseButtonEventArgs e)
        {
            var v = sender as FrameworkElement;
            YoutubeVideoInfo yvi = v.DataContext as YoutubeVideoInfo;
            App.ViewModel.NowPlaying.SelectedSong = yvi;
            await App.ViewModel.OpenMedia(yvi);
            var currentIndex = App.ViewModel.NowPlaying.Songs.IndexOf(App.ViewModel.NowPlaying.SelectedSong);
            if (currentIndex == App.ViewModel.NowPlaying.Songs.Count - 1) await App.ViewModel.NowPlaying.UpNextAdd(yvi);
        }

        private void Songs_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (Songs.ItemContainerGenerator.ContainerFromIndex(Songs.SelectedIndex) is FrameworkElement song)
            {
                var viewer = Helpers.FindParent<AnimatedScrollViewer>(song);
                var transform = song.TransformToVisual(viewer);
                var position = transform.Transform(new Point(0, 0));
                var top = position.Y + viewer.VerticalOffset + song.ActualHeight;
                var bottom = position.Y + viewer.VerticalOffset - viewer.ViewportHeight;
                if (e.VerticalOffset < top && e.VerticalOffset > bottom)
                {
                    FadeScrollBtn(false);
                    return;
                }
                if (e.VerticalOffset > top) FadeScrollBtn(true, VerticalAlignment.Top);
                if (e.VerticalOffset < bottom) FadeScrollBtn(true, VerticalAlignment.Bottom);
            }
            else FadeScrollBtn(false);
        }

        private void ScrollToPlayingSong_Click(object sender, RoutedEventArgs e)
        {
            ScrollToPlayingSong();
        }

        public void ScrollToPlayingSong()
        {
            if (Songs.ItemContainerGenerator.ContainerFromIndex(Songs.SelectedIndex) is FrameworkElement song)
            {
                var viewer = Helpers.FindParent<AnimatedScrollViewer>(song);
                var transform = song.TransformToVisual(viewer);
                var position = transform.Transform(new Point(0, 0));
                position.Y += viewer.VerticalOffset - (viewer.ViewportHeight / 2 - song.ActualHeight / 2);
                viewer.TargetVerticalOffset = position.Y;
            }
        }

        public void FadeScrollBtn(bool visibility, VerticalAlignment direction = VerticalAlignment.Top)
        {
            if (visibility)
            {
                ScrollToPlayingSongBtn.VerticalAlignment = direction;
                if (direction == VerticalAlignment.Bottom)
                {
                    var icon = App.Current.Resources["DownArrow"] as Geometry;
                    ScrollBtnPathIcon.Data = icon;
                    var anim = Resources["FadeInFromBottomBtn"] as Storyboard;
                    anim.Begin();
                }
                else
                {
                    var icon = App.Current.Resources["UpArrow"] as Geometry;
                    ScrollBtnPathIcon.Data = icon;
                    var anim = Resources["FadeInFromTopBtn"] as Storyboard;
                    anim.Begin();
                }
            }
            else
            {
                if (direction == VerticalAlignment.Bottom)
                {
                    var anim = Resources["FadeOutFromBottomBtn"] as Storyboard;
                    anim.Begin();
                }
                else
                {
                    var anim = Resources["FadeOutFromTopBtn"] as Storyboard;
                    anim.Begin();
                }
            }
        }
    }
}
