using System;
using Windows.UI.Xaml.Data;

namespace GBxUWP.Converters
{
    class CartridgeMapperToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is CartridgeMapperType m)
            {
                return m.DisplayName();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new InvalidOperationException();
        }
    }
}
