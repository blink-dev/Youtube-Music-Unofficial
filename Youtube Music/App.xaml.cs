using GKeys;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Youtube_Music.ViewModels;

namespace Youtube_Music
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex mutex;
        private bool firstInstance;

        public static new App Current => Application.Current as App;
        public static Frame AppFrame => (Application.Current.MainWindow as MainWindow).AppFrame;
        public static Services.CacheService CacheService { get; set; } = new();
        public static Services.YTMusicApi MusicApi { get; set; } = new();

        public static MainViewModel ViewModel => Current.Resources["MainViewModel"] as MainViewModel;
        public static MainWindow WindowMain => Application.Current.MainWindow as MainWindow;

        public static bool CloseToTray { get; set; }

        private HwndSource source;

        private TaskbarIcon notifyIcon;

        public App() : base()
        {
            SetupUnhandledExceptionHandling();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            mutex = new(true, "{f835ea0e-d5d8-460f-b793-7c3fcd652c3d}", out firstInstance);
            if (!firstInstance)
            {
                NativeMethods.SendMessage((IntPtr)NativeMethods.HWND_BROADCAST, NativeMethods.WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
                Application.Current.Shutdown();
                return;
            }


            CloseToTray = Youtube_Music.Properties.Settings.Default.CloseToTray;

            Application.Current.MainWindow = new MainWindow
            {
                Left = Youtube_Music.Properties.Settings.Default.PosX,
                Top = Youtube_Music.Properties.Settings.Default.PosY
            };

            ViewModel.MusicPlayer.Volume = Youtube_Music.Properties.Settings.Default.Volume;

            MainWindow.SourceInitialized += MainWindow_SourceInitialized;

            GlobalHotkeys.Init(MainWindow);

            Application.Current.MainWindow.Closed += MainWindow_Closed;
            Application.Current.MainWindow.Show();

            base.OnStartup(e);
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            source.RemoveHook(WndProc);
            mutex.Close();
            mutex.Dispose();
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            var helper = new WindowInteropHelper(Current.MainWindow);
            source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            static void playAction() => ViewModel.StartPlaybackCommand.Execute();
            static void nextAction() => ViewModel.NextSongCommand.Execute();
            static void previousAction() => ViewModel.PreviousSongCommand.Execute();
            static void volumeUpAction()
            {
                ViewModel.ShiftVolume(120);
            }
            static void volumeDownAction()
            {
                ViewModel.ShiftVolume(-120);
            }

            var keys = System.Text.Json.JsonSerializer.Deserialize<List<Hotkey>>(Youtube_Music.Properties.Settings.Default.GlobalHotkeys);
            //var keys = new List<Hotkey>();

            if (keys.Count < 5)
            {
                keys = new();
                Hotkey play = new("Play/Pause");
                Hotkey next = new("Next Song");
                Hotkey previous = new("Previous Song");
                Hotkey volumeUp = new("Volume Up");
                Hotkey volumeDown = new("Volume Down");

                keys.Add(play);
                keys.Add(next);
                keys.Add(previous);
                keys.Add(volumeUp);
                keys.Add(volumeDown);

                var json = System.Text.Json.JsonSerializer.Serialize(keys);
                Youtube_Music.Properties.Settings.Default.GlobalHotkeys = json;
                Youtube_Music.Properties.Settings.Default.Save();
            }

            foreach (var key in keys)
            {
                if (key.Label == "Play/Pause")
                    GlobalHotkeys.Add(key.Label, (ModifierKeys)key.Modifiers, KeyInterop.KeyFromVirtualKey(key.VirtualKey), playAction);
                else if (key.Label == "Next Song")
                    GlobalHotkeys.Add(key.Label, (ModifierKeys)key.Modifiers, KeyInterop.KeyFromVirtualKey(key.VirtualKey), nextAction);
                else if (key.Label == "Previous Song")
                    GlobalHotkeys.Add(key.Label, (ModifierKeys)key.Modifiers, KeyInterop.KeyFromVirtualKey(key.VirtualKey), previousAction);
                else if (key.Label == "Volume Up")
                    GlobalHotkeys.Add(key.Label, (ModifierKeys)key.Modifiers, KeyInterop.KeyFromVirtualKey(key.VirtualKey), volumeUpAction);
                else if (key.Label == "Volume Down")
                    GlobalHotkeys.Add(key.Label, (ModifierKeys)key.Modifiers, KeyInterop.KeyFromVirtualKey(key.VirtualKey), volumeDownAction);
            }
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_SHOWME)
            {
                NotifyIconViewModel.ShowWindowCommand.Execute();
                handled = true;
            }

            return IntPtr.Zero;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon?.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }

        private void SetupUnhandledExceptionHandling()
        {
            // Catch exceptions from all threads in the AppDomain.
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                ShowUnhandledException(args.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException", false);

            // Catch exceptions from each AppDomain that uses a task scheduler for async operations.
            TaskScheduler.UnobservedTaskException += (sender, args) =>
                ShowUnhandledException(args.Exception, "TaskScheduler.UnobservedTaskException", false);

            // Catch exceptions from a single specific UI dispatcher thread.
            Dispatcher.UnhandledException += (sender, args) =>
            {
                // If we are debugging, let Visual Studio handle the exception and take us to the code that threw it.
                if (!Debugger.IsAttached)
                {
                    args.Handled = true;
                    ShowUnhandledException(args.Exception, "Dispatcher.UnhandledException", true);
                }
            };
        }

        private static async void ShowUnhandledException(Exception e, string unhandledExceptionType, bool promptUserForShutdown)
        {
            var messageBoxTitle = $"Unexpected Error Occurred: {unhandledExceptionType}";
            var messageBoxMessage = $"The following exception occurred:\n\n{e}";
            var messageBoxButtons = MessageBoxButton.OK;

            string logpathdir = $"{System.IO.Path.GetTempPath()}YoutubeMusic";
            if (!System.IO.Directory.Exists(logpathdir)) System.IO.Directory.CreateDirectory(logpathdir);
            string logpath = $"{logpathdir}/log.txt";
            await System.IO.File.AppendAllTextAsync(logpath, $"--- {DateTime.Now} ---\n");
            await System.IO.File.AppendAllTextAsync(logpath, messageBoxTitle);
            await System.IO.File.AppendAllTextAsync(logpath, $"{messageBoxMessage}\n\n");

            if (promptUserForShutdown)
            {
                messageBoxMessage += "\n\nNormally the app would die now. Should we let it die?";
                messageBoxButtons = MessageBoxButton.YesNo;
            }

            // Let the user decide if the app should die or not (if applicable).
            if (MessageBox.Show(messageBoxMessage, messageBoxTitle, messageBoxButtons) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

    }
}
