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
		
		private bool usingLinux = false;
		
        private ConsoleApplication(bool usingLinux) {
			this.usingLinux = usingLinux;
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
                            instance = new ConsoleApplication(true);
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

        public EventManager EventManager {
            get {
                return eventManager;
            }
        }

        internal void SetCursorPosition(Point position) {
			if (!usingLinux) {
            	NativeMethods.SetConsoleCursorPosition(stdOutputHandle, new COORD((short) position.x, (short) position.y));
			}
        }

        /// <summary>
        /// Состояние курсора консоли для избежания повторных вызовов Show и Hide.
        /// Консистентность этого свойства может быть нарушена, если пользоваться в приложении
        /// нативными функциями для работы с курсором напрямую.
        /// </summary>
        internal bool CursorIsVisible {
            get;
            private set;
        }

        /// <summary>
        /// Делает курсор консоли видимым и устанавливает значение
        /// CursorIsVisible в true.
        /// </summary>
        internal void ShowCursor() {
			if (!usingLinux) {
	            CONSOLE_CURSOR_INFO consoleCursorInfo = new CONSOLE_CURSOR_INFO {
	                Size = 5,
	                Visible = true
	            };
	            NativeMethods.SetConsoleCursorInfo(stdOutputHandle, ref consoleCursorInfo);
			}
            CursorIsVisible = true;
        }

        /// <summary>
        /// Делает курсор консоли невидимым и устанавливает значение
        /// CursorIsVisible в false.
        /// </summary>
        internal void HideCursor() {
			if (!usingLinux) {
	            CONSOLE_CURSOR_INFO consoleCursorInfo = new CONSOLE_CURSOR_INFO {
	                Size = 5,
	                Visible = false
	            };
	            NativeMethods.SetConsoleCursorInfo(stdOutputHandle, ref consoleCursorInfo);
			}
            CursorIsVisible = false;
        }
		
		public void Run(Control control) {
			if (usingLinux) {
				RunLinux(control);
			} else {
				RunWindows(control);
			}
		}
		
		public void RunLinux(Control control) {
			this.mainControl = control;
			//
			PhysicalCanvas canvas = new PhysicalCanvas(100, 35);
            renderer.Canvas = canvas;
            renderer.RootElementRect = new Rect(0, 0, 80, 25);
            renderer.RootElement = mainControl;
            // initialize default focus
            focusManager.AfterAddElementToTree(mainControl);
            //
            mainControl.Invalidate();
			
			LinuxConsoleApplication.initscr();
			LinuxConsoleApplication.start_color();
			LinuxConsoleApplication.init_pair(1, LinuxConsoleApplication.COLOR_BLACK, 5);
			LinuxConsoleApplication.attron(LinuxConsoleApplication.COLOR_PAIR(1));
			
            renderer.UpdateRender();
			//LinuxConsoleApplication.addstr("lksjdf ыловаыва\u2591");
			
			
			//addstr("Hello from C-sharp ! И немного русского текста.");
			//refresh();
			LinuxConsoleApplication.getch();
			LinuxConsoleApplication.endwin();
		}
		
        public void RunWindows(Control control) {
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

            // initially hide the console cursor
            HideCursor();
            
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
