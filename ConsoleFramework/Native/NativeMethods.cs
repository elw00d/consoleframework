using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleFramework.Native
{
    public static class NativeMethods {
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
}
