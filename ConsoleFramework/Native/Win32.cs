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

        /// <summary>
        /// Returns current console mode. Program saves it before changing and
        /// restores before exit.
        /// </summary>
        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool GetConsoleMode( IntPtr hConsoleHandle, [Out] out uint mode );

        /// <summary>
        /// It is used to set ENABLE_WINDOW_INPUT flag, which enables the events
        /// about console screen buffer resize.
        /// </summary>
        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool SetConsoleMode( IntPtr hConsoleHandle, uint mode );

        [DllImport("kernel32.dll")]
        public static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput,
            out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

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

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
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

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        public static extern bool IsZoomed(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hwnd);

        public const UInt32 WM_SYSCOMMAND = 0x0112;

        public static readonly IntPtr SC_MAXIMIZE = new IntPtr( 0xF030 );

        public static readonly IntPtr SC_RESTORE = new IntPtr( 0xF120 );
    }

    public enum StdHandleType {
        STD_INPUT_HANDLE = -10,
        STD_OUTPUT_HANDLE = -11,
        STD_ERROR_HANDLE = -12
    }

    public struct CONSOLE_SCREEN_BUFFER_INFO
    {
        public COORD dwSize;
        public COORD dwCursorPosition;
        public short wAttributes;
        public SMALL_RECT srWindow;
        public COORD dwMaximumWindowSize;
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
        public VirtualKeys wVirtualKeyCode;
        //public short wVirtualKeyCode;
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

    /// <summary>
    /// Enumeration for virtual keys.
    /// </summary>
    public enum VirtualKeys
        : ushort
    {
        LeftButton = 0x01,
        RightButton = 0x02,
        Cancel = 0x03,
        MiddleButton = 0x04,
        ExtraButton1 = 0x05,
        ExtraButton2 = 0x06,
        Back = 0x08,
        Tab = 0x09,
        Clear = 0x0C,
        Return = 0x0D,
        Shift = 0x10,
        Control = 0x11,
        Menu = 0x12,
        Pause = 0x13,
        CapsLock = 0x14,
        Kana = 0x15,
        Hangeul = 0x15,
        Hangul = 0x15,
        Junja = 0x17,
        Final = 0x18,
        Hanja = 0x19,
        Kanji = 0x19,
        Escape = 0x1B,
        Convert = 0x1C,
        NonConvert = 0x1D,
        Accept = 0x1E,
        ModeChange = 0x1F,
        Space = 0x20,
        PageUp = 0x21,
        Prior = 0x21,
        PageDown = 0x22,
        Next = 0x22,
        End = 0x23,
        Home = 0x24,
        Left = 0x25,
        Up = 0x26,
        Right = 0x27,
        Down = 0x28,
        Select = 0x29,
        Print = 0x2A,
        Execute = 0x2B,
        Snapshot = 0x2C,
        Insert = 0x2D,
        Delete = 0x2E,
        Help = 0x2F,
        N0 = 0x30,
        N1 = 0x31,
        N2 = 0x32,
        N3 = 0x33,
        N4 = 0x34,
        N5 = 0x35,
        N6 = 0x36,
        N7 = 0x37,
        N8 = 0x38,
        N9 = 0x39,
        A = 0x41,
        B = 0x42,
        C = 0x43,
        D = 0x44,
        E = 0x45,
        F = 0x46,
        G = 0x47,
        H = 0x48,
        I = 0x49,
        J = 0x4A,
        K = 0x4B,
        L = 0x4C,
        M = 0x4D,
        N = 0x4E,
        O = 0x4F,
        P = 0x50,
        Q = 0x51,
        R = 0x52,
        S = 0x53,
        T = 0x54,
        U = 0x55,
        V = 0x56,
        W = 0x57,
        X = 0x58,
        Y = 0x59,
        Z = 0x5A,
        LeftWindows = 0x5B,
        RightWindows = 0x5C,
        Application = 0x5D,
        Sleep = 0x5F,
        Numpad0 = 0x60,
        Numpad1 = 0x61,
        Numpad2 = 0x62,
        Numpad3 = 0x63,
        Numpad4 = 0x64,
        Numpad5 = 0x65,
        Numpad6 = 0x66,
        Numpad7 = 0x67,
        Numpad8 = 0x68,
        Numpad9 = 0x69,
        Multiply = 0x6A,
        Add = 0x6B,
        Separator = 0x6C,
        Subtract = 0x6D,
        Decimal = 0x6E,
        Divide = 0x6F,
        F1 = 0x70,
        F2 = 0x71,
        F3 = 0x72,
        F4 = 0x73,
        F5 = 0x74,
        F6 = 0x75,
        F7 = 0x76,
        F8 = 0x77,
        F9 = 0x78,
        F10 = 0x79,
        F11 = 0x7A,
        F12 = 0x7B,
        F13 = 0x7C,
        F14 = 0x7D,
        F15 = 0x7E,
        F16 = 0x7F,
        F17 = 0x80,
        F18 = 0x81,
        F19 = 0x82,
        F20 = 0x83,
        F21 = 0x84,
        F22 = 0x85,
        F23 = 0x86,
        F24 = 0x87,
        NumLock = 0x90,
        ScrollLock = 0x91,
        NEC_Equal = 0x92,
        Fujitsu_Jisho = 0x92,
        Fujitsu_Masshou = 0x93,
        Fujitsu_Touroku = 0x94,
        Fujitsu_Loya = 0x95,
        Fujitsu_Roya = 0x96,
        LeftShift = 0xA0,
        RightShift = 0xA1,
        LeftControl = 0xA2,
        RightControl = 0xA3,
        LeftMenu = 0xA4,
        RightMenu = 0xA5,
        BrowserBack = 0xA6,
        BrowserForward = 0xA7,
        BrowserRefresh = 0xA8,
        BrowserStop = 0xA9,
        BrowserSearch = 0xAA,
        BrowserFavorites = 0xAB,
        BrowserHome = 0xAC,
        VolumeMute = 0xAD,
        VolumeDown = 0xAE,
        VolumeUp = 0xAF,
        MediaNextTrack = 0xB0,
        MediaPrevTrack = 0xB1,
        MediaStop = 0xB2,
        MediaPlayPause = 0xB3,
        LaunchMail = 0xB4,
        LaunchMediaSelect = 0xB5,
        LaunchApplication1 = 0xB6,
        LaunchApplication2 = 0xB7,
        OEM1 = 0xBA,
        OEMPlus = 0xBB,
        OEMComma = 0xBC,
        OEMMinus = 0xBD,
        OEMPeriod = 0xBE,
        OEM2 = 0xBF,
        OEM3 = 0xC0,
        OEM4 = 0xDB,
        OEM5 = 0xDC,
        OEM6 = 0xDD,
        OEM7 = 0xDE,
        OEM8 = 0xDF,
        OEMAX = 0xE1,
        OEM102 = 0xE2,
        ICOHelp = 0xE3,
        ICO00 = 0xE4,
        ProcessKey = 0xE5,
        ICOClear = 0xE6,
        Packet = 0xE7,
        OEMReset = 0xE9,
        OEMJump = 0xEA,
        OEMPA1 = 0xEB,
        OEMPA2 = 0xEC,
        OEMPA3 = 0xED,
        OEMWSCtrl = 0xEE,
        OEMCUSel = 0xEF,
        OEMATTN = 0xF0,
        OEMFinish = 0xF1,
        OEMCopy = 0xF2,
        OEMAuto = 0xF3,
        OEMENLW = 0xF4,
        OEMBackTab = 0xF5,
        ATTN = 0xF6,
        CRSel = 0xF7,
        EXSel = 0xF8,
        EREOF = 0xF9,
        Play = 0xFA,
        Zoom = 0xFB,
        Noname = 0xFC,
        PA1 = 0xFD,
        OEMClear = 0xFE
    }
}
