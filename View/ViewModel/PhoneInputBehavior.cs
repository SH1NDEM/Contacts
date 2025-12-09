using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ViewModel
{
    public static class PhoneInputBehavior
    {
        private static readonly Regex _allowed = new Regex(@"^[0-9+\-() ]+$");

        public static readonly DependencyProperty EnablePhoneFilterProperty =
            DependencyProperty.RegisterAttached(
                "EnablePhoneFilter",
                typeof(bool),
                typeof(PhoneInputBehavior),
                new PropertyMetadata(false, OnEnablePhoneFilterChanged));

        public static bool GetEnablePhoneFilter(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnablePhoneFilterProperty);
        }

        public static void SetEnablePhoneFilter(DependencyObject obj, bool value)
        {
            obj.SetValue(EnablePhoneFilterProperty, value);
        }

        private static void OnEnablePhoneFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox textBox) return;
            bool enabled = (bool)e.NewValue;

            if (enabled)
            {
                textBox.PreviewTextInput += OnPreviewTextInput;
                DataObject.AddPastingHandler(textBox, OnPasting);
            }
            else
            {
                textBox.PreviewTextInput -= OnPreviewTextInput;
                DataObject.RemovePastingHandler(textBox, OnPasting);
            }
        }

        private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !_allowed.IsMatch(e.Text);
        }

        private static void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string paste = (string)e.DataObject.GetData(typeof(string));
                if (!_allowed.IsMatch(paste))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
