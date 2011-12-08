using System;
using System.Threading;
using ConsoleFramework.Native;

namespace ConsoleFramework
{
    public sealed class ConsoleApplication : IDisposable {

        private ConsoleApplication() {
        }

        private static volatile ConsoleApplication instance;
        private static readonly object syncRoot = new object();
        public static ConsoleApplication Instance {
            get {
                if (instance == null) {
                    lock (syncRoot) {
                        if (instance == null) {
                            instance = new ConsoleApplication();
                        }
                    }
                }
                return instance;
            }
        }

        private IntPtr stdInputHandle;
        private IntPtr stdOutputHandle;
        private readonly EventWaitHandle exitWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private readonly ConsoleDispatcher dispatcher = new ConsoleDispatcher();
        public ConsoleDispatcher Dispatcher {
            get {
                return dispatcher;
            }
        }

        public void Exit() {
            exitWaitHandle.Set();
        }

        public void Run() {
            //
            stdInputHandle = NativeMethods.GetStdHandle(StdHandleType.STD_INPUT_HANDLE);
            stdOutputHandle = NativeMethods.GetStdHandle(StdHandleType.STD_OUTPUT_HANDLE);
            IntPtr[] handles = new[] {
                exitWaitHandle.SafeWaitHandle.DangerousGetHandle(),
                stdInputHandle
            };
            // small test of physical canvas, todo : remove after tests
            PhysicalCanvas canvas = new PhysicalCanvas(80, 25, stdOutputHandle);
            for (int x = 0; x < 80; x++ ) {
                for (int y = 0; y < 25; y++) {
                    if (x == 40) {
                        canvas[x][y].UnicodeChar = '8';
                        canvas[x][y].Attributes = CHAR_ATTRIBUTES.BACKGROUND_RED;
                    }
                    else {
                        canvas[x][y].AsciiChar = '0';
                        canvas[x][y].Attributes = CHAR_ATTRIBUTES.FOREGROUND_BLUE | CHAR_ATTRIBUTES.FOREGROUND_GREEN;
                    }
                    
                }
            }
            canvas.Flush();

            while (true) {
                uint waitResult = NativeMethods.WaitForMultipleObjects(2, handles, false, NativeMethods.INFINITE);
                if (waitResult == 0) {
                    break;
                }
                if (waitResult == 1) {
                    processInput();
                    continue;
                }
                // if we received WAIT_TIMEOUT or WAIT_FAILED
                if (waitResult == 0x00000102 || waitResult == 0xFFFFFFFF) {
                    throw new InvalidOperationException("Invalid wait result of WaitForMultipleObjects.");
                }
            }
        }

        private void processInput() {
            INPUT_RECORD[] buffer = new INPUT_RECORD[10];
            uint read;
            bool bReaded = NativeMethods.ReadConsoleInput(stdInputHandle, buffer, (uint) buffer.Length, out read);
            if (!bReaded) {
                throw new InvalidOperationException("ReadConsoleInput method failed.");
            }
            for (int i = 0; i < read; ++i) {
                processInputEvent(buffer[i]);
            }
        }

        private void processInputEvent(INPUT_RECORD inputRecord) {
            // todo : remove after tests
            if (inputRecord.EventType == EventType.MOUSE_EVENT) {
                if (inputRecord.MouseEvent.dwButtonState == MouseButtonState.RIGHTMOST_BUTTON_PRESSED && inputRecord.MouseEvent.dwEventFlags == MouseEventFlags.DOUBLE_CLICK) {
                    this.Exit();
                }
            }
            //
            dispatcher.DispatchInputEvent(inputRecord);
        }

        private void dispose(bool isDisposing) {
            if (isDisposing) {
                if (exitWaitHandle != null) {
                    exitWaitHandle.Dispose();
                }
            }
        }

        public void Dispose() {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ConsoleApplication() {
            dispose(false);
        }
    }
}
