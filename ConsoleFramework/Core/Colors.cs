using ConsoleFramework.Native;

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

    public static class Colors
    {
        /// <summary>
        /// Blends foreground and background colors into one char attributes code.
        /// </summary>
        /// <param name="foreground">Foreground color</param>
        /// <param name="background">Background color</param>
        public static Attr Blend(Color foreground, Color background)
        {
            return ( Attr ) ( (ushort)foreground + (((ushort) background) << 4) );
        }
    }
}