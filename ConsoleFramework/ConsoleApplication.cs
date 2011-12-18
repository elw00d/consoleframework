using System;
using System.Threading;
using ConsoleFramework.Controls;
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

        public void Exit() {
            exitWaitHandle.Set();
        }

        private Control mainControl;

        public void Run(Control control) {
            this.mainControl = control;
            //
            stdInputHandle = NativeMethods.GetStdHandle(StdHandleType.STD_INPUT_HANDLE);
            stdOutputHandle = NativeMethods.GetStdHandle(StdHandleType.STD_OUTPUT_HANDLE);
            IntPtr[] handles = new[] {
                exitWaitHandle.SafeWaitHandle.DangerousGetHandle(),
                stdInputHandle
            };
            
            // todo : introduce settings instead hardcode 80x25
            PhysicalCanvas canvas = new PhysicalCanvas(80, 25, stdOutputHandle);
            this.mainControl.canvas = new VirtualCanvas(control, canvas, 0, 0);
            this.mainControl.Draw(0, 0, 80, 25);
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
            //dispatcher.DispatchInputEvent(inputRecord);
            mainControl.HandleEvent(inputRecord);
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
