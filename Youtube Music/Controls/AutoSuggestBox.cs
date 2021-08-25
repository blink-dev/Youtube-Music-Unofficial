using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Youtube_Music.Controls
{
    public class AutoSuggestBox : ItemsControl
    {
        public event TextChangedEventHandler TextChanged;
        public event RoutedEventHandler SuggestionChosen;

        TextBox PartEditor;
        ListView PartSelector;
        Button ClearBtn;

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Placeholder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(AutoSuggestBox), new PropertyMetadata("Search..."));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(AutoSuggestBox), new PropertyMetadata(""));

        public System.Collections.IEnumerable Suggestions
        {
            get { return (System.Collections.IEnumerable)GetValue(SuggestionsProperty); }
            set { SetValue(SuggestionsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Suggestions.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SuggestionsProperty =
            DependencyProperty.Register("Suggestions", typeof(System.Collections.IEnumerable), typeof(AutoSuggestBox), new PropertyMetadata(null, OnSuggestionsChanged));

        private static void OnSuggestionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (AutoSuggestBox)d;
            if (sender.closedByUser) return;
            sender.PartSelector.ItemsSource = sender.Suggestions;
            if (sender.PartSelector.Items.Count > 0)
            {
                sender.IsDropDownOpen = true;
            }
            else sender.IsDropDownOpen = false;
        }

        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDropDownOpen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(AutoSuggestBox), new PropertyMetadata(null));

        bool closedByUser = false;
        static AutoSuggestBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoSuggestBox), new FrameworkPropertyMetadata(typeof(AutoSuggestBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            PartEditor = Template.FindName("PART_Editor", this) as TextBox;
            PartSelector = Template.FindName("PART_Selector", this) as ListView;
            ClearBtn = Template.FindName("ClearBtn", this) as Button;

            PartSelector.SelectionChanged += PartSelector_SelectionChanged;
            PartSelector.MouseLeftButtonUp += PartSelector_MouseLeftButtonUp;

            PartEditor.TextChanged += PART_Editor_TextChanged;
            PartEditor.LostFocus += (s, e) => { if (!IsKeyboardFocusWithin) IsDropDownOpen = false; };

            ClearBtn.Click += ClearBtn_Click;

            KeyUp += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    closedByUser = true;
                    IsDropDownOpen = false;
                    SuggestionChosen?.Invoke(s, null);
                }
                else if (e.Key == Key.Down)
                {
                    PartSelector.SelectedIndex++;
                    PartEditor.CaretIndex = PartEditor.Text.Length;
                }
                else if (e.Key == Key.Up)
                {
                    PartSelector.SelectedIndex--;
                    PartEditor.CaretIndex = PartEditor.Text.Length;
                }

            };
        }

        private void PartSelector_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as ListView).SelectedItem;
            if (item != null)
            {
                SuggestionChosen?.Invoke(sender, null);
                IsDropDownOpen = false;
            }
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            PartEditor.Text = "";
            ClearBtn.Visibility = Visibility.Collapsed;
            PartEditor.Focus();
        }

        private void PartSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PartSelector.SelectedItem != null)
            {
                PartEditor.TextChanged -= PART_Editor_TextChanged;
                Text = PartSelector.SelectedItem.ToString();
                PartEditor.TextChanged += PART_Editor_TextChanged;
            }
        }

        private void PART_Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            closedByUser = false;
            Text = ((TextBox)sender).Text;
            TextChanged?.Invoke(sender, null);
            if (!string.IsNullOrWhiteSpace(Text)) ClearBtn.Visibility = Visibility.Visible;
            else ClearBtn.Visibility = Visibility.Collapsed;
        }
    }
}
