using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using MahTweets.Core.Extensions;
using MahTweets.Core.Filters;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Converters
{
    public class UpdateColourConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var update = values[0] as IStatusUpdate;
            var filters = values[1] as StreamModel;
            var IsMouseOver = (bool) values[2];
            Double Opacity = 1;

            if (IsMouseOver)
                Opacity = 0.0;

            if (update == null)
                return ApplyOpacity(new SolidColorBrush(Colors.White), Opacity);

            if (filters != null)
            {
                Color found = filters.GetColorForContact(update);
                    //.FilterList.Where(x => (String.Compare(x.Identifier, update.Contact.Name, true) == 0)).FirstOrDefault());
                if (found != StreamModel.InvalidColor)
                    //; && found.Color != (Color)ColorConverter.ConvertFromString("#00000000") && found.Color != (Color)ColorConverter.ConvertFromString("#FFFFFFFF"))
                    return ApplyOpacity(new SolidColorBrush(found), Opacity*((double) found.A/100));

                found = filters.GetColorForMicroblog(update);
                if (found != StreamModel.InvalidColor)
                    return ApplyOpacity(new SolidColorBrush(found), Opacity*((double) found.A/100));
            }

            UpdateType topType = null;
            if (update.Types != null)
                topType = update.Types.ToList().Where(type => type.HasColor())
                    .OrderBy(x => x.Order).LastOrDefault();

            if (topType != null)
            {
                object convertFromString = ColorConverter.ConvertFromString(topType.ColorARGB);
                if (convertFromString != null)
                    return ApplyOpacity(new SolidColorBrush((Color) convertFromString), Opacity);
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion

        private Brush ApplyOpacity(SolidColorBrush b, Double Opacity)
        {
            b.Opacity *= Opacity;
            return b;
        }
    }
}