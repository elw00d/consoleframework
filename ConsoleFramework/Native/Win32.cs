using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleFramework.Native
{
    /// <summary>
    /// Interop code for Win32 environment.
    /// </summary>
    public static class Win32 {
        public static uint INFINITE = 0xFFFFFFFF;

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern void AllocConsole();

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetStdHandle([MarshalAs(UnmanagedType.I4)]StdHandleType nStdHandle);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern uint WaitForMultipleObjects(uint nCount,
                                                         [MarshalAs(UnmanagedType.LPArray)] IntPtr[] lpHandles,
                                                         bool bWaitAll, uint dwMilliseconds);

        [DllImport("kernel32.dll", EntryPoint = "ReadConsoleInputW", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern bool ReadConsoleInput(IntPtr hConsoleInput,
                                                   [Out] INPUT_RECORD[] lpBuffer,
                                                   uint nLength, out uint lpNumberOfEventsRead);

        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "WriteConsoleOutputW")]
        public static extern bool WriteConsoleOutputCore(IntPtr hConsoleOutput, CHAR_INFO[,] lpBuffer, COORD dwBufferSize,
                                                     COORD dwBufferCoord, ref SMALL_RECT lpWriteRegion);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int FormatMessage(int dwFlags, string lpSource, int dwMessageId, int dwLanguageId,
                                               StringBuilder lpBuffer, int nSize, string[] Arguments);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleCursorPosition(IntPtr hConsoleOutput, COORD dwCursorPosition);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleCursorInfo(IntPtr hConsoleOutput, out CONSOLE_CURSOR_INFO lpConsoleCursorInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleCursorInfo(IntPtr hConsoleOutput, [In] ref CONSOLE_CURSOR_INFO lpConsoleCursorInfo);

        public static string GetLastErrorMessage() {
            StringBuilder strLastErrorMessage = new StringBuilder(255);
            int ret2 = Marshal.GetLastWin32Error();
            const int dwFlags = 4096;
            FormatMessage(dwFlags, null, ret2, 0, strLastErrorMessage, strLastErrorMessage.Capacity, null);
            return strLastErrorMessage.ToString();
        }
    }

    public enum StdHandleType {
        STD_INPUT_HANDLE = -10,
        STD_OUTPUT_HANDLE = -11,
        STD_ERROR_HANDLE = -12
    }

    /// <summary>
    /// CharSet.Unicode is required for proper marshaling.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct CHAR_INFO
    {
        [FieldOffset(0)]
        public char UnicodeChar;
        [FieldOffset(0)]
        public char AsciiChar;
        [FieldOffset(2)] //2 bytes seems to work properly
        public Attr Attributes;

        public override string ToString() {
            return string.Format("CHAR_INFO : '{0}' ({1})", AsciiChar, Attributes);
        }
    }

    /// <summary>
    /// CHAR_ATTRIBUTES native structure.
    /// </summary>
    [Flags]
    public enum Attr : ushort {
        NO_ATTRIBUTES = 0x0000,
        /// <summary>
        /// Text color contains blue.
        /// </summary>
        FOREGROUND_BLUE = 0x0001,
        /// <summary>
        /// Text color contains green.
        /// </summary>
        FOREGROUND_GREEN = 0x0002,
        /// <summary>
        /// Text color contains red.
        /// </summary>
        FOREGROUND_RED = 0x0004,
        /// <summary>
        /// Text color is intensified.
        /// </summary>
        FOREGROUND_INTENSITY = 0x0008,
        /// <summary>
        /// Background color contains blue.
        /// </summary>
        BACKGROUND_BLUE = 0x0010,
        /// <summary>
        /// Background color contains green.
        /// </summary>
        BACKGROUND_GREEN = 0x0020,
        /// <summary>
        /// Background color contains red.
        /// </summary>
        BACKGROUND_RED = 0x0040,
        /// <summary>
        /// Background color is intensified.
        /// </summary>
        BACKGROUND_INTENSITY = 0x0080,
        /// <summary>
        /// Leading byte.
        /// </summary>
        COMMON_LVB_LEADING_BYTE = 0x0100,
        /// <summary>
        /// Trailing byte.
        /// </summary>
        COMMON_LVB_TRAILING_BYTE = 0x0200,
        /// <summary>
        /// Top horizontal
        /// </summary>
        COMMON_LVB_GRID_HORIZONTAL = 0x0400,
        /// <summary>
        /// Left vertical.
        /// </summary>
        COMMON_LVB_GRID_LVERTICAL = 0x0800,
        /// <summary>
        /// Right vertical.
        /// </summary>
        COMMON_LVB_GRID_RVERTICAL = 0x1000,
        /// <summary>
        /// Reverse foreground and background attribute.
        /// </summary>
        COMMON_LVB_REVERSE_VIDEO = 0x4000,
        /// <summary>
        /// Underscore.
        /// </summary>
        COMMON_LVB_UNDERSCORE = 0x8000
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct INPUT_RECORD
    {
        [FieldOffset(0)]
        public EventType EventType;
        [FieldOffset(4)]
        public KEY_EVENT_RECORD KeyEvent;
        [FieldOffset(4)]
        public MOUSE_EVENT_RECORD MouseEvent;
        [FieldOffset(4)]
        public WINDOW_BUFFER_SIZE_RECORD WindowBufferSizeEvent;
        [FieldOffset(4)]
        public MENU_EVENT_RECORD MenuEvent;
        [FieldOffset(4)]
        public FOCUS_EVENT_RECORD FocusEvent;
    };

    public enum EventType : ushort {
        FOCUS_EVENT = 0x0010,
        KEY_EVENT = 0x0001,
        MENU_EVENT = 0x0008,
        MOUSE_EVENT = 0x0002,
        WINDOW_BUFFER_SIZE_EVENT = 0x0004
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct KEY_EVENT_RECORD
    {
        [FieldOffset(0), MarshalAs(UnmanagedType.Bool)]
        public bool bKeyDown;
        [FieldOffset(4), MarshalAs(UnmanagedType.U2)]
        public ushort wRepeatCount;

        [FieldOffset(6), MarshalAs(UnmanagedType.U2)]
        //public VirtualKeys wVirtualKeyCode;
        public short wVirtualKeyCode;
        [FieldOffset(8), MarshalAs(UnmanagedType.U2)]
        public ushort wVirtualScanCode;
        [FieldOffset(10)]
        public char UnicodeChar;
        [FieldOffset(12), MarshalAs(UnmanagedType.U4)]
        public ControlKeyState dwControlKeyState;
    }

    [Flags]
    public enum ControlKeyState {
        CAPSLOCK_ON = 0x0080,
        ENHANCED_KEY = 0x0100,
        LEFT_ALT_PRESSED = 0x0002,
        LEFT_CTRL_PRESSED = 0x0008,
        NUMLOCK_ON = 0x0020,
        RIGHT_ALT_PRESSED = 0x0001,
        RIGHT_CTRL_PRESSED = 0x0004,
        SCROLLLOCK_ON = 0x0040,
        SHIFT_PRESSED = 0x0010
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MOUSE_EVENT_RECORD
    {
        [FieldOffset(0)]
        public COORD dwMousePosition;
        [FieldOffset(4)]
        public MOUSE_BUTTON_STATE dwButtonState;
        [FieldOffset(8)]
        public ControlKeyState dwControlKeyState;
        [FieldOffset(12)]
        public MouseEventFlags dwEventFlags;
    }

    [Flags]
    public enum MOUSE_BUTTON_STATE {
        FROM_LEFT_1ST_BUTTON_PRESSED = 0x0001,
        FROM_LEFT_2ND_BUTTON_PRESSED = 0x0004,
        FROM_LEFT_3RD_BUTTON_PRESSED = 0x0008,
        FROM_LEFT_4TH_BUTTON_PRESSED = 0x0010,
        RIGHTMOST_BUTTON_PRESSED = 0x0002
    }

    [Flags]
    public enum MouseEventFlags {
        PRESSED_OR_RELEASED = 0x0000,
        DOUBLE_CLICK = 0x0002,
        MOUSE_HWHEELED = 0x0008,
        MOUSE_MOVED = 0x0001,
        MOUSE_WHEELED = 0x0004
    }

    public struct WINDOW_BUFFER_SIZE_RECORD
    {
        public COORD dwSize;

        public WINDOW_BUFFER_SIZE_RECORD(short x, short y)
        {
            this.dwSize = new COORD(x, y);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MENU_EVENT_RECORD
    {
        public uint dwCommandId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FOCUS_EVENT_RECORD
    {
        public uint bSetFocus;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct COORD
    {
        public short X;
        public short Y;

        public COORD(short X, short Y)
        {
            this.X = X;
            this.Y = Y;
        }
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct SMALL_RECT
    {
        public short Left;
        public short Top;
        public short Right;
        public short Bottom;

        public SMALL_RECT(short left, short top, short right, short bottom) {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CONSOLE_CURSOR_INFO
    {
        /// <summary>
        /// The percentage of the character cell that is filled by the cursor.
        /// This value is between 1 and 100. The cursor appearance varies, ranging from completely
        /// filling the cell to showing up as a horizontal line at the bottom of the cell.
        /// </summary>
        public uint Size;
        public bool Visible;
    }
}
