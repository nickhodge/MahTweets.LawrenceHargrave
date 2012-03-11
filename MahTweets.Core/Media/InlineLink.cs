using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MahTweets.Core.Media
{
    public class InlineLink : Span
    {
        public static readonly DependencyProperty UrlProperty = DependencyProperty.Register("Url", typeof (Uri),
                                                                                            typeof (InlineLink),
                                                                                            new FrameworkPropertyMetadata
                                                                                                (null, OnUrlChanged));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof (string),
                                                                                             typeof (InlineLink),
                                                                                             new FrameworkPropertyMetadata
                                                                                                 (string.Empty,
                                                                                                  OnUrlChanged));

        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image",
                                                                                              typeof (UIElement),
                                                                                              typeof (InlineLink),
                                                                                              new FrameworkPropertyMetadata
                                                                                                  (null, OnUrlChanged));

        private readonly Brush _normal;

        public InlineLink()
        {
            Clicked = false;
            _normal = FindResource("BaseColour") as Brush;
            Foreground = _normal;
        }

        public bool Clicked { get; set; }

        public UIElement Image
        {
            get { return (UIElement) GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public Uri Url
        {
            get { return (Uri) GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        public Brush NormalColour { get; set; }

        public Brush HoverColour { get; set; }

        public string Text
        {
            get { return (String) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnUrlChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var il = (InlineLink) obj;
            il.Inlines.Clear();

            if (args.Property.Name != "Text" && il.Image != null)
                il.Inlines.Add(il.Image);

            var r = new Run(il.Text);
            il.Inlines.Add(r);
            il.Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            Foreground = HoverColour ?? _normal;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            Foreground = NormalColour ?? _normal;
        }
    }
}