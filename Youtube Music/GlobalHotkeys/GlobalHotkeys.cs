using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace GKeys
{
    public class GlobalHotkeys
    {
        private static HwndSource source;
        private static WindowInteropHelper helper;
        private static int keyId = 9000;

        public static ObservableCollection<Hotkey> Hotkeys { get; set; } = new ObservableCollection<Hotkey>();

        public static void Init(Window window)
        {
            helper = new WindowInteropHelper(window);
            window.SourceInitialized += (s, e) =>
            {
                source = HwndSource.FromHwnd(helper.Handle);
                source.AddHook(new HwndSourceHook(WndProc));
            };
            window.Closed += (s, e) =>
            {
                UnregisterAll();
                source.RemoveHook(WndProc);
                source.Dispose();
            };
            Hotkeys.CollectionChanged += Hotkeys_CollectionChanged;
        }

        public static void Add(string label, ModifierKeys modifierKeys, Key virtualKey, Action hotkeyCommand)
        {
            Hotkey hotkey = new(label)
            {
                KeyID = keyId++,
                Modifiers = (int)modifierKeys,
                VirtualKey = KeyInterop.VirtualKeyFromKey(virtualKey),
                HotkeyCommand = hotkeyCommand
            };

            Hotkeys.Add(hotkey);
            RegisterHotKey(hotkey.KeyID, hotkey.Modifiers, hotkey.VirtualKey);
        }

        private static void Hotkeys_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (object item in e.NewItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged += KeyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (object item in e.OldItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged -= KeyChanged;
                }
            }
        }

        private static void KeyChanged(object sender, PropertyChangedEventArgs e)
        {
            var data = sender as Hotkey;
            if (data.VirtualKey == 0) return;
            if (Hotkeys.Any(h => h != data && h.VirtualKey == data.VirtualKey))
            {
                var rem = Hotkeys.Where(h => h != data && h.VirtualKey == data.VirtualKey).First();
                rem.Modifiers = 0;
                rem.VirtualKey = 0;
            }
        }

        public static void RegisterAll()
        {
            foreach (var hotkey in Hotkeys)
            {
                if (!hotkey.VirtualKey.Equals(0))
                    RegisterHotKey(hotkey.KeyID, hotkey.Modifiers, hotkey.VirtualKey);
            }
        }

        public static void UnregisterAll()
        {
            foreach (var hotkey in Hotkeys) UnregisterHotKey(hotkey.KeyID);
        }

        public static void RegisterHotKey(int id, int modifiers, int key)
        {
            NativeMethods.RegisterHotKey(helper.Handle, id, modifiers, key);
        }

        public static void UnregisterHotKey(int id)
        {
            NativeMethods.UnregisterHotKey(helper.Handle, id);
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_HOTKEY)
            {
                if (Hotkeys.Any(h => h.KeyID == wParam.ToInt32()))
                {
                    var hotkey = Hotkeys.Where(h => h.KeyID == wParam.ToInt32()).First();
                    hotkey.HotkeyCommand?.Invoke();
                }
                handled = true;
            }

            return IntPtr.Zero;
        }
    }
}
