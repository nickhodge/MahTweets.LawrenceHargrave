using System;
using System.Windows;

namespace MahTweets.UI.Controls
{
    public class Margins
    {
        public static readonly DependencyProperty LeftProperty = DependencyProperty.RegisterAttached("Left",
                                                                                                     typeof (Double),
                                                                                                     typeof (Margins),
                                                                                                     new UIPropertyMetadata
                                                                                                         (0.0,
                                                                                                          TriggerChanged));

        public static readonly DependencyProperty RightProperty = DependencyProperty.RegisterAttached("Right",
                                                                                                      typeof (Double),
                                                                                                      typeof (Margins),
                                                                                                      new UIPropertyMetadata
                                                                                                          (0.0,
                                                                                                           TriggerChanged));

        public static readonly DependencyProperty TopProperty = DependencyProperty.RegisterAttached("Top",
                                                                                                    typeof (Double),
                                                                                                    typeof (Margins),
                                                                                                    new UIPropertyMetadata
                                                                                                        (0.0,
                                                                                                         TriggerChanged));

        public static readonly DependencyProperty BottomProperty = DependencyProperty.RegisterAttached("Bottom",
                                                                                                       typeof (Double),
                                                                                                       typeof (Margins),
                                                                                                       new UIPropertyMetadata
                                                                                                           (0.0,
                                                                                                            TriggerChanged));

        public static Double GetLeft(DependencyObject obj)
        {
            return (Double) obj.GetValue(LeftProperty);
        }

        public static void SetLeft(DependencyObject obj, Double value)
        {
            obj.SetValue(LeftProperty, value);
        }

        public static Double GetRight(DependencyObject obj)
        {
            return (Double) obj.GetValue(RightProperty);
        }

        public static void SetRight(DependencyObject obj, Double value)
        {
            obj.SetValue(RightProperty, value);
        }

        public static void SetTop(DependencyObject obj, Double value)
        {
            obj.SetValue(TopProperty, value);
        }

        public static Double GetTop(DependencyObject obj)
        {
            return (Double) obj.GetValue(TopProperty);
        }

        public static void SetBottom(DependencyObject obj, Double value)
        {
            obj.SetValue(BottomProperty, value);
        }

        public static Double GetBottom(DependencyObject obj)
        {
            return (Double) obj.GetValue(BottomProperty);
        }

        private static void TriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = ((FrameworkElement) d);

            if (e.Property == TopProperty)
                element.Margin = new Thickness(element.Margin.Left, (Double) e.NewValue, element.Margin.Right,
                                               element.Margin.Bottom);

            if (e.Property == LeftProperty)
                element.Margin = new Thickness((Double) e.NewValue, element.Margin.Top, element.Margin.Right,
                                               element.Margin.Bottom);

            if (e.Property == RightProperty)
                element.Margin = new Thickness(element.Margin.Left, element.Margin.Top, (Double) e.NewValue,
                                               element.Margin.Bottom);

            if (e.Property == BottomProperty)
                element.Margin = new Thickness(element.Margin.Left, element.Margin.Top, element.Margin.Right,
                                               (Double) e.NewValue);
        }
    }
}