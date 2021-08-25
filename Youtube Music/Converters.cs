using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Cache;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Youtube_Music.Converters
{
    internal class ExpanderRotateAngleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double factor = 1.0;
            if (parameter is { } parameterValue)
            {
                if (!double.TryParse(parameterValue.ToString(), out factor))
                {
                    factor = 1.0;
                }
            }
            return value switch
            {
                ExpandDirection.Left => 90 * factor,
                ExpandDirection.Right => -90 * factor,
                _ => 0
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsFirstItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ListViewItem item = value as ListViewItem;
            ListView ListView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
            int index = ListView.ItemContainerGenerator.IndexFromContainer(item);
            if (index == 0) return true;
            else return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsLastItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ListViewItem item = value as ListViewItem;
            ListView ListView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
            int index = ListView.ItemContainerGenerator.IndexFromContainer(item);
            if (index == ListView.Items.Count - 1) return true;
            else return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class ImageConverter : IValueConverter
    {
        public object Convert(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new BitmapImage(new Uri(value.ToString()));
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    internal class UriToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                BitmapImage bi = new();
                bi.BeginInit();
                //bi.DecodePixelWidth = 300;
                bi.DecodePixelHeight = 450;
                bi.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.Revalidate);
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bi.UriSource = new Uri(value.ToString());
                bi.EndInit();
                return bi;
            }
            catch (Exception) { }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public class TimeSpanToSecondsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                TimeSpan span => span.TotalSeconds,
                Duration duration => duration.HasTimeSpan ? duration.TimeSpan.TotalSeconds : 0d,
                _ => 0d,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double == false) return 0d;
            var result = TimeSpan.FromTicks(System.Convert.ToInt64(TimeSpan.TicksPerSecond * (double)value));

            if (targetType == typeof(TimeSpan)) return result;
            return targetType == typeof(Duration) ?
                new Duration(result) : Activator.CreateInstance(targetType);
        }
    }

    public class TimeSpanFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan? p;

            switch (value)
            {
                case TimeSpan position:
                    p = position;
                    break;
                case Duration duration when duration.HasTimeSpan:
                    p = duration.TimeSpan;
                    break;
                case double totals:
                    p = TimeSpan.FromSeconds(totals);
                    break;
                default:
                    return string.Empty;
            }

            if (p.Value.TotalHours < 1) return $"{p.Value.Minutes:00}:{p.Value.Seconds:00}";
            return $"{(int)p.Value.TotalHours:00}:{p.Value.Minutes:00}:{p.Value.Seconds:00}";
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MultiTimeSpanFormatter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan? p;
            TimeSpan? p1;

            switch (values[0])
            {
                case TimeSpan position:
                    p = position;
                    break;
                case Duration duration when duration.HasTimeSpan:
                    p = duration.TimeSpan;
                    break;
                case double totals:
                    p = TimeSpan.FromSeconds(totals);
                    break;
                default:
                    return string.Empty;
            }

            switch (values[1])
            {
                case TimeSpan position:
                    p1 = position;
                    break;
                case Duration duration when duration.HasTimeSpan:
                    p1 = duration.TimeSpan;
                    break;
                case double totals:
                    p1 = TimeSpan.FromSeconds(totals);
                    break;
                default:
                    return string.Empty;
            }

            if (p1 is not null)
            {
                if (p1.Value.TotalHours < 1) return $"{p.Value.Minutes:00}:{p.Value.Seconds:00}";
            }

            return $"{(int)p.Value.TotalHours:00}:{p.Value.Minutes:00}:{p.Value.Seconds:00}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RangeLengthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 4 || values.Any(v => v == null))
                return Binding.DoNothing;

            if (!double.TryParse(values[0].ToString(), out double min)
                || !double.TryParse(values[1].ToString(), out double max)
                || !double.TryParse(values[2].ToString(), out double value)
                || !double.TryParse(values[3].ToString(), out double containerLength))

                return Binding.DoNothing;

            var percent = (value - min) / (max - min);
            var length = percent * containerLength;

            return length > containerLength ? containerLength : length;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class MathConverter : IValueConverter
    {
        public enum MathOperation
        {
            Add,
            Subtract,
            Multiply,
            Divide,
            Pow
        }

        public MathOperation Operation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double value1 = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
                double value2 = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
                return Operation switch
                {
                    MathOperation.Add => value1 + value2,
                    MathOperation.Divide => value1 / value2,
                    MathOperation.Multiply => value1 * value2,
                    MathOperation.Subtract => value1 - value2,
                    MathOperation.Pow => Math.Pow(value1, value2),
                    _ => Binding.DoNothing,
                };
            }
            catch (FormatException)
            {
                return Binding.DoNothing;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }

    public class NotZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse((value ?? "").ToString(), out double val))
            {
                return Math.Abs(val) > 0.0;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }

    public class ArcSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double @double && (@double > 0.0))
            {
                return new Size(@double / 2, @double / 2);
            }

            return new Point();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class ArcEndPointConverter : IMultiValueConverter
    {
        public const string ParameterMidPoint = "MidPoint";

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var actualWidth = values[0].ExtractDouble();
            var value = values[1].ExtractDouble();
            var minimum = values[2].ExtractDouble();
            var maximum = values[3].ExtractDouble();

            if (new[] { actualWidth, value, minimum, maximum }.AnyNan())
                return Binding.DoNothing;

            if (values.Length == 5)
            {
                var fullIndeterminateScaling = values[4].ExtractDouble();
                if (!double.IsNaN(fullIndeterminateScaling) && fullIndeterminateScaling > 0.0)
                {
                    value = (maximum - minimum) * fullIndeterminateScaling;
                }
            }

            var percent = maximum <= minimum ? 1.0 : (value - minimum) / (maximum - minimum);
            if (Equals(parameter, ParameterMidPoint))
                percent /= 2;

            var degrees = 360 * percent;
            var radians = degrees * (Math.PI / 180);

            var centre = new Point(actualWidth / 2, actualWidth / 2);
            var hypotenuseRadius = (actualWidth / 2);

            var adjacent = Math.Cos(radians) * hypotenuseRadius;
            var opposite = Math.Sin(radians) * hypotenuseRadius;

            return new Point(centre.X + opposite, centre.Y - adjacent);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RotateTransformCentreConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //value == actual width
            return (double)value / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class StartPointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double @double && (@double > 0.0))
            {
                return new Point(@double / 2, 0);
            }

            return new Point();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

    }

    internal static class LocalEx
    {
        public static double ExtractDouble(this object val)
        {
            var d = val as double? ?? double.NaN;
            return double.IsInfinity(d) ? double.NaN : d;
        }


        public static bool AnyNan(this IEnumerable<double> vals)
        {
            return vals.Any(double.IsNaN);
        }
    }
}
