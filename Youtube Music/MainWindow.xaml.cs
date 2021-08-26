using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Youtube_Music.ViewModels;
using Youtube_Music.Views;

namespace Youtube_Music
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, (_, __) => { SystemCommands.CloseWindow(this); }));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, (_, __) => { SystemCommands.MinimizeWindow(this); }));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, (_, __) => { SystemCommands.MaximizeWindow(this); }));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, (_, __) => { SystemCommands.RestoreWindow(this); }));

            Loaded += (s, e) =>
            {
                Thumb thumb = (PositionSlider.Template.FindName("PART_Track", PositionSlider) as Track).Thumb;
                thumb.MouseEnter += (ss, ee) =>
                {
                    if (ee.LeftButton == MouseButtonState.Pressed && ee.MouseDevice.Captured == null)
                    {
                        MouseButtonEventArgs args = new(ee.MouseDevice, ee.Timestamp, MouseButton.Left)
                        {
                            RoutedEvent = MouseLeftButtonDownEvent
                        };
                        (ss as Thumb).RaiseEvent(args);
                    }
                };

                Thumb thumb2 = (VolumeSlider.Template.FindName("PART_Track", VolumeSlider) as Track).Thumb;
                thumb2.MouseEnter += (ss, ee) =>
                {
                    if (ee.LeftButton == MouseButtonState.Pressed && ee.MouseDevice.Captured == null)
                    {
                        MouseButtonEventArgs args = new(ee.MouseDevice, ee.Timestamp, MouseButton.Left)
                        {
                            RoutedEvent = MouseLeftButtonDownEvent
                        };
                        (ss as Thumb).RaiseEvent(args);
                    }
                };

                AppFrame.Navigate(Resources["Home"]);
            };
        }

        public void ShowHideMediaGrid()
        {
            if (AppFrame.ActualHeight > 393)
            {
                Storyboard anim = Resources["MediaGridHide"] as Storyboard;
                anim.Begin();
                return;
            }

            Storyboard showMediaGrid = Resources["MediaGridShow"] as Storyboard;
            showMediaGrid.Begin();
        }

        public void PlayButtonLoading(bool isLoading)
        {
            if (isLoading)
            {
                Styles.ButtonProgressAssist.SetIsIndicatorVisible(PlayBtn, true);
                PlayBtn.IsEnabled = false;
                PlayPauseIcon.Opacity = 0;
            }
            else
            {
                Styles.ButtonProgressAssist.SetIsIndicatorVisible(PlayBtn, false);
                PlayBtn.IsEnabled = true;
                PlayPauseIcon.Opacity = 1;
            }
        }

        private void WindowTitleZone_LeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            if (App.CloseToTray) Hide();
            else NotifyIconViewModel.ExitApplicationCommand.Execute();
        }

        private bool _allowDirectNavigation = false;
        private NavigatingCancelEventArgs _navArgs = null;

        private void AppFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                e.Cancel = true;
                return;
            }

            Type contentType = e.Content.GetType();

            if (contentType == typeof(Home)) HomeBtn.IsChecked = true;
            else if (contentType == typeof(LikedSongs)) LikedSongsBtn.IsChecked = true;
            else if (contentType == typeof(Downloads)) DownloadsBtn.IsChecked = true;
            else if (contentType == typeof(Search)) SearchBtn.IsChecked = true;
            else if (contentType == typeof(Settings)) SettingsBtn.IsChecked = true;
            else
            {
                HomeBtn.IsChecked = false;
                LikedSongsBtn.IsChecked = false;
                DownloadsBtn.IsChecked = false;
                SearchBtn.IsChecked = false;
                SettingsBtn.IsChecked = false;
            }

            if (e.Content != null && !_allowDirectNavigation)
            {
                e.Cancel = true;
                _navArgs = e;
                var anim = Resources["AppFrameNavigationAnimation0"] as Storyboard;
                anim.Begin();
            }
        }

        private void SlideCompleted(object sender, EventArgs e)
        {
            _allowDirectNavigation = true;
            switch (_navArgs.NavigationMode)
            {
                case NavigationMode.New:
                    if (_navArgs.Uri == null)
                        AppFrame.NavigationService.Navigate(_navArgs.Content);
                    else
                        AppFrame.NavigationService.Navigate(_navArgs.Uri);
                    break;
                case NavigationMode.Back:
                    if (AppFrame.NavigationService.CanGoBack) AppFrame.NavigationService.GoBack();
                    break;
                case NavigationMode.Forward:
                    if (AppFrame.NavigationService.CanGoForward) AppFrame.NavigationService.GoForward();
                    break;
                case NavigationMode.Refresh:
                    AppFrame.NavigationService.Refresh();
                    break;
            }

            var anim = Resources["AppFrameNavigationAnimation1"] as Storyboard;
            anim.Begin();

            _allowDirectNavigation = false;

            if (AppFrame.Height < 393)
            {
                var hide = Resources["MediaGridHide"] as Storyboard;
                hide.Begin();
            }
        }

        private void VolumeSlider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            App.ViewModel.ShiftVolume(e.Delta);
        }
    }
}
