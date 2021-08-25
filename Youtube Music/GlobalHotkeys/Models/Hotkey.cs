using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace GKeys
{
    public class Hotkey : INotifyPropertyChanged
    {
        [JsonIgnore]
        public int KeyID { get; set; }

        public string Label { get; set; }

        public int Modifiers { get; set; }

        private int virtualKey;

        public int VirtualKey
        {
            get => virtualKey;
            set
            {
                virtualKey = value;
                if (!KeyID.Equals(0) && !virtualKey.Equals(0)) GlobalHotkeys.RegisterHotKey(KeyID, Modifiers, virtualKey);
                KeyString = GetKeyString();
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public Action HotkeyCommand { get; set; }

        private string keyString;

        [JsonIgnore]
        public string KeyString
        {
            get => keyString;
            set
            {
                keyString = value;
                OnPropertyChanged();
            }
        }

        public Hotkey(string label)
        {
            Label = label;
        }

        public string GetKeyString()
        {
            var str = new StringBuilder();
            var modkeys = (ModifierKeys)Modifiers;
            var key = KeyInterop.KeyFromVirtualKey(VirtualKey);

            if (modkeys.HasFlag(ModifierKeys.Control))
                str.Append("Ctrl + ");
            if (modkeys.HasFlag(ModifierKeys.Shift))
                str.Append("Shift + ");
            if (modkeys.HasFlag(ModifierKeys.Alt))
                str.Append("Alt + ");
            if (modkeys.HasFlag(ModifierKeys.Windows))
                str.Append("Win + ");

            str.Append(key);
            return str.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
