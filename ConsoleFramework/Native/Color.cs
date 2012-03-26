using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleFramework.Native {
    public static class Color {
        public const ushort INTENSITY = 0x0008;
        public const ushort Black = 0x0000;
        public const ushort DarkBlue = 0x0001;
        public const ushort DarkGreen = 0x0002;
        public const ushort DarkRed = 0x0004;

        public const ushort DarkCyan = DarkBlue | DarkGreen;
        public const ushort DarkMagenta = DarkBlue | DarkRed;
        public const ushort DarkYellow = DarkGreen | DarkRed;
        public const ushort Gray = DarkRed | DarkGreen | DarkBlue;

        public const ushort DarkGray = Black | INTENSITY;
        public const ushort Blue = DarkBlue | INTENSITY;
        public const ushort Green = DarkGreen | INTENSITY;
        public const ushort Red = DarkRed | INTENSITY;

        public const ushort Cyan = DarkCyan | INTENSITY;
        public const ushort Magenta = DarkMagenta | INTENSITY;
        public const ushort Yellow = DarkYellow | INTENSITY;
        public const ushort White = Gray | INTENSITY;

        public static ushort Attr(uint foreground, uint background) {
            return (ushort) (foreground + (background << 4));
        }
    }
}