using GKeys;
using System.Windows.Controls;
using System.Windows.Input;

namespace Youtube_Music.Views
{
    public partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void HotkeyInputKey_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var btn = sender as Button;
            btn.Content = "...";
            GlobalHotkeys.UnregisterAll();
        }

        private void HotkeyInputKey_KeyUp(object sender, KeyEventArgs e)
        {
            var btn = sender as Button;
            if (btn.Content.Equals("..."))
            {
                Hotkey hotkey = btn.DataContext as Hotkey;

                var key = e.Key;

                // When Alt is pressed, SystemKey is used instead
                if (key == Key.System) key = e.SystemKey;

                // Pressing delete, backspace or escape without modifiers clears the current value
                if (Keyboard.Modifiers == ModifierKeys.None && (key == Key.Delete || key == Key.Back || key == Key.Escape))
                {
                    hotkey.Modifiers = 0;
                    hotkey.VirtualKey = 0;
                    btn.Content = hotkey.KeyString;
                    GlobalHotkeys.RegisterAll();
                    return;
                }

                // If no actual key was pressed - return
                if (key == Key.LeftCtrl ||
                    key == Key.RightCtrl ||
                    key == Key.LeftAlt ||
                    key == Key.RightAlt ||
                    key == Key.LeftShift ||
                    key == Key.RightShift ||
                    key == Key.LWin ||
                    key == Key.RWin ||
                    key == Key.Clear ||
                    key == Key.OemClear ||
                    key == Key.Apps)
                {
                    btn.Content = hotkey.GetKeyString();
                    GlobalHotkeys.RegisterAll();
                    return;
                }

                hotkey.Modifiers = (int)Keyboard.Modifiers;
                hotkey.VirtualKey = KeyInterop.VirtualKeyFromKey(key);

                btn.Content = hotkey.KeyString;
                GlobalHotkeys.RegisterAll();
            }
        }

        private void Button_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var btn = sender as Button;
            Hotkey hotkey = btn.DataContext as Hotkey;
            btn.Content = hotkey.GetKeyString();
            GlobalHotkeys.RegisterAll();
        }

        private void Button_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
    }
}
