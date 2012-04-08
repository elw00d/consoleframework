using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;

namespace ConsoleFramework
{
    public sealed class ConsoleApplication : IDisposable {

        private ConsoleApplication() {
            eventManager = new EventManager();
            focusManager = new FocusManager(eventManager);
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

        private Renderer renderer = new Renderer();
        public Renderer Renderer {
            get {
                return renderer;
            }
        }

        private Control mainControl;
        private EventManager eventManager;
        private FocusManager focusManager;

        public FocusManager FocusManager {
            get {
                return focusManager;
            }
        }

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
            PhysicalCanvas canvas = new PhysicalCanvas(100, 35, stdOutputHandle);
            renderer.Canvas = canvas;
            renderer.RootElementRect = new Rect(5, 5, 80, 25);
            renderer.RootElement = mainControl;
            // initialize default focus
            focusManager.AfterAddElementToTree(mainControl);
            //
            mainControl.Invalidate();
            renderer.UpdateRender();
            
            while (true) {
                uint waitResult = NativeMethods.WaitForMultipleObjects(2, handles, false, NativeMethods.INFINITE);
                if (waitResult == 0) {
                    break;
                }
                if (waitResult == 1) {
                    processInput();
                    // update 
                    renderer.UpdateRender();
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
            eventManager.ProcessInput(inputRecord, mainControl, renderer.RootElementRect);
        }

        public void BeginCaptureInput(Control control) {
            eventManager.BeginCaptureInput(control);
        }

        public void EndCaptureInput(Control control) {
            eventManager.EndCaptureInput(control);
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
