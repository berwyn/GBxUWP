using System;
using System.ComponentModel;
using Windows.UI.Xaml.Data;

namespace GBxUWP.Converters
{
    public class VoltageToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return null;
            }
            else if (value is Voltage v)
            {
                switch (v)
                {
                    case Voltage.Gameboy:
                        return "Gameboy";
                    case Voltage.GameboyAdvance:
                        return "Gameboy Advance";
                    case Voltage.Unknown:
                        return null;
                    default:
                        // In theory, this is infallible but we need a newer version of C# to actually track that >:(
                        throw new InvalidEnumArgumentException("Invalid case.");
                }
            }
            else
            {
                throw new InvalidOperationException("Unsupported target type!");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string s)
            {
                switch (s)
                {
                    case "Gameboy":
                        return Voltage.Gameboy;
                    case "Gameboy Advance":
                        return Voltage.GameboyAdvance;
                    default:
                        throw new InvalidEnumArgumentException("Invalid voltage!");
                }
            }
            else
            {
                throw new InvalidOperationException("Unsupported target type!");
            }
        }
    }
}
