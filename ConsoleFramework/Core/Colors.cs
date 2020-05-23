using System;
using ConsoleFramework.Native;
using Xaml;

namespace ConsoleFramework.Core {
    /// <summary>
    /// Set of predefined colors.
    /// </summary>
    public enum Color : ushort {
        Black = 0x0000,
        DarkBlue = 0x0001,
        DarkGreen = 0x0002,
        DarkRed = 0x0004,

        DarkCyan = DarkBlue | DarkGreen,
        DarkMagenta = DarkBlue | DarkRed,
        DarkYellow = DarkGreen | DarkRed,
        Gray = DarkRed | DarkGreen | DarkBlue,

        DarkGray = Black | Attr.FOREGROUND_INTENSITY,
        Blue = DarkBlue | Attr.FOREGROUND_INTENSITY,
        Green = DarkGreen | Attr.FOREGROUND_INTENSITY,
        Red = DarkRed | Attr.FOREGROUND_INTENSITY,

        Cyan = DarkCyan | Attr.FOREGROUND_INTENSITY,
        Magenta = DarkMagenta | Attr.FOREGROUND_INTENSITY,
        Yellow = DarkYellow | Attr.FOREGROUND_INTENSITY,
        White = Gray | Attr.FOREGROUND_INTENSITY
    }

    public static class Colors {
        /// <summary>
        /// Blends foreground and background colors into one char attributes code.
        /// </summary>
        /// <param name="foreground">Foreground color</param>
        /// <param name="background">Background color</param>
        public static Attr Blend(Color foreground, Color background) {
            return (Attr) ((ushort) foreground + (((ushort) background) << 4));
        }
    }

    [TypeConverter(typeof(ColorPairConverter))]
    public class ColorPair : Tuple<Color, Color> {
        public ColorPair(Color foreground, Color background) : base(foreground, background) {
        }

        public Color ForegroundColor => Item1;
        public Color BackgroundColor => Item2;
    }

    /// <summary>
    /// Converter for color pair
    /// String value example: "Blue on Gray"
    /// </summary>
    public class ColorPairConverter : ITypeConverter {
        public bool CanConvertFrom(Type sourceType) {
            return Type.GetTypeCode(sourceType) == TypeCode.String;
        }

        public bool CanConvertTo(Type destinationType) {
            return destinationType == typeof(string);
        }

        public object ConvertFrom(object value) {
            var parts = ((String) value).Split(new[] {"on"}, StringSplitOptions.RemoveEmptyEntries);
            return new ColorPair(
                (Color) Enum.Parse(typeof(Color), parts[0].Trim()),
                (Color) Enum.Parse(typeof(Color), parts[1].Trim())
            );
        }

        public object ConvertTo(object value, Type destinationType) {
            var colorPair = ((ColorPair) value);
            return $"{colorPair.ForegroundColor}:{colorPair.BackgroundColor}";
        }
    }
}
