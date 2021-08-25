using System.Collections.Generic;
using System.Windows;
using Youtube_Music.Commands;

namespace Youtube_Music.ViewModels
{
    public class NotifyIconViewModel : BindableBase
    {
        private bool _closeToTray;
        private bool _playButtonsVisible;

        public bool CloseToTray
        {
            get => _closeToTray;
            set
            {
                Set(ref _closeToTray, value);
                App.CloseToTray = CloseToTray;
            }
        }

        public bool PlayButtonsVisible
        {
            get => _playButtonsVisible;
            set => Set(ref _playButtonsVisible, value);
        }

        public static DelegateCommand ShowWindowCommand => new(delegate
        {
            App.Current.MainWindow.Show();
            if (App.Current.MainWindow.WindowState != WindowState.Normal) App.Current.MainWindow.WindowState = WindowState.Normal;
            App.Current.MainWindow.Activate();
        });

        public static DelegateCommand HideWindowCommand => new(App.Current.MainWindow.Hide);

        public static DelegateCommand ExitApplicationCommand => new(delegate
        {
            var main = App.Current.MainWindow as MainWindow;

            var keys = new List<GKeys.Hotkey>();
            foreach (var h in GKeys.GlobalHotkeys.Hotkeys)
                keys.Add(new GKeys.Hotkey(h.Label) { Modifiers = h.Modifiers, VirtualKey = h.VirtualKey });
            var json = System.Text.Json.JsonSerializer.Serialize(keys);

            Properties.Settings.Default.GlobalHotkeys = json;

            Properties.Settings.Default.CloseToTray = App.CloseToTray;
            Properties.Settings.Default.Volume = App.ViewModel.MusicPlayer.Volume;
            Properties.Settings.Default.PosX = main.Left;
            Properties.Settings.Default.PosY = main.Top;
            Properties.Settings.Default.Save();
            App.ViewModel.MusicPlayer.Stop();
            Application.Current.Shutdown();
        });

        public NotifyIconViewModel()
        {
            CloseToTray = App.CloseToTray;
            _playButtonsVisible = false;
            App.ViewModel.MusicPlayer.MediaOpened += (s, e) => PlayButtonsVisible = true;
        }

    }
}