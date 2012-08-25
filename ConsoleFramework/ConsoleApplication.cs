using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using System.Diagnostics;
using System.IO;
using Linux.Native;

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
			if (usingLinux) {
				int res = LibTermKey.writeInt64(eventfd, 1);
				//Console.WriteLine("write(1) returned {0}\n", res);
				//if (res == -1) {
				//	int lastError = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
				//	Console.WriteLine("Last error is {0}\n", lastError);
				//}
			} else {
            	exitWaitHandle.Set();
			}
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
		
		private int eventfd = -1;
		private IntPtr termkeyHandle = IntPtr.Zero;
		
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
			
			// terminal initialization sequence
			IntPtr stdscr = LinuxConsoleApplication.initscr();
			LinuxConsoleApplication.cbreak();
			LinuxConsoleApplication.noecho();
			LinuxConsoleApplication.nonl();
			LinuxConsoleApplication.intrflush(stdscr, false);
			LinuxConsoleApplication.keypad (stdscr, true);
			LinuxConsoleApplication.start_color();
			//LinuxConsoleApplication.mousemask(0xFFFFFFFF, IntPtr.Zero);
			
			renderer.UpdateRender();
			
			//do {
			//	int c = LinuxConsoleApplication.getch();
			//	processLinuxInput(c);
			//	renderer.UpdateRender();
			//} while (true);
			
			termkeyHandle = LibTermKey.termkey_new(0, TermKeyFlag.TERMKEY_FLAG_SPACESYMBOL);
			Console.Write("\x1B[?1002h");
			pollfd fd = new pollfd();
			fd.fd = 0;
			fd.events = POLL_EVENTS.POLLIN;
			
			pollfd[] fds = new pollfd[2];
			fds[0] = fd;
			
			fds[1] = new pollfd();
			eventfd = LibTermKey.eventfd(0, EVENTFD_FLAGS.EFD_CLOEXEC);
			if (eventfd == -1) {
				Console.WriteLine("Cannot create eventfd\n");
				int lastError = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
				Console.WriteLine("Last error is {0}\n", lastError);
			}
			fds[1].fd = eventfd;
			fds[1].events = POLL_EVENTS.POLLIN;
			
			TermKeyKey key = new TermKeyKey();
			while (true) {
				int pollRes = LibTermKey.poll(fds, 2, -1);
				if (0 == pollRes) {
					// timed out
					Console.WriteLine("Timed out");
					if (LibTermKey.termkey_getkey_force(termkeyHandle, ref key) == TermKeyResult.TERMKEY_RES_KEY) {
						Console.WriteLine("got TERMKEY_RES_KEY");
					}					
				} else if (-1 == pollRes) {
					int errorCode = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
					//Console.WriteLine(string.Format("ErrorCode = {0}", errorCode));
					// todo : write to debug console if error code differs from 4
				}
				//Console.WriteLine(string.Format("PollRes is {0}", pollRes));
				
				if (fds[1].revents != POLL_EVENTS.NONE) {
					// exit signal
					break;
				}
				
				for (int i = 0; i < 2; i++) {
					if (fds[i].revents != POLL_EVENTS.NONE) {
						if (i == 1) {
							UInt64 u;
							LibTermKey.readInt64(fds[i].fd, out u);
							Console.WriteLine("Readed eventfd counter : {0}\n", u);
						}
					}
				}
				
				if ((fds[0].revents & POLL_EVENTS.POLLIN) == POLL_EVENTS.POLLIN ||
				    (fds[0].revents & POLL_EVENTS.POLLHUP) == POLL_EVENTS.POLLHUP ||
				    (fds[0].revents & POLL_EVENTS.POLLERR) == POLL_EVENTS.POLLERR)
				{
					// todo : log return value
					LibTermKey.termkey_advisereadable(termkeyHandle);
				}
				
				TermKeyResult result;
				while ((result = LibTermKey.termkey_getkey(termkeyHandle, ref key)) == TermKeyResult.TERMKEY_RES_KEY) {
					processLinuxInput(key);
					renderer.UpdateRender();
				}
			}
			
			LibTermKey.termkey_destroy(termkeyHandle);
			LibTermKey.close(eventfd);
			Console.Write("\x1B[?1002l");
			
			//
			LinuxConsoleApplication.endwin();
		}
		
		private void processLinuxInput(TermKeyKey key) {
			if (key.type == TermKeyType.TERMKEY_TYPE_UNICODE) {
				byte[] data = new byte[7];
				data[0] = key.utf8_0;
				data[1] = key.utf8_1;
				data[2] = key.utf8_2;
				data[3] = key.utf8_3;
				data[4] = key.utf8_4;
				data[5] = key.utf8_5;
				data[6] = key.utf8_6;
				string d = System.Text.Encoding.UTF8.GetString(data);
				char unicodeCharacter = d[0];
				INPUT_RECORD inputRecord = new INPUT_RECORD();
				inputRecord.EventType = EventType.KEY_EVENT;
				inputRecord.KeyEvent.bKeyDown = true;
				inputRecord.KeyEvent.wRepeatCount = 1;
				inputRecord.KeyEvent.UnicodeChar = unicodeCharacter;
				inputRecord.KeyEvent.dwControlKeyState = 0;
				if ((key.modifiers & 4) == 4) {
					inputRecord.KeyEvent.dwControlKeyState |= ControlKeyState.LEFT_CTRL_PRESSED;
				}
				if ((key.modifiers & 2) == 2) {
					inputRecord.KeyEvent.dwControlKeyState |= ControlKeyState.LEFT_ALT_PRESSED;
				}
				if (unicodeCharacter == 'd' && key.modifiers == 4) {
					Exit ();
				}
				processInputEvent(inputRecord);
				//
			} else if (key.type == TermKeyType.TERMKEY_TYPE_MOUSE) {
				TermKeyMouseEvent ev;
				int button;
				int line, col;
				LibTermKey.termkey_interpret_mouse(termkeyHandle, ref key, out ev, out button, out line, out col);
				//
				INPUT_RECORD inputRecord = new INPUT_RECORD();
				inputRecord.EventType = EventType.MOUSE_EVENT;
				if (ev == TermKeyMouseEvent.TERMKEY_MOUSE_PRESS || ev == TermKeyMouseEvent.TERMKEY_MOUSE_RELEASE)
					inputRecord.MouseEvent.dwEventFlags = MouseEventFlags.PRESSED_OR_RELEASED;
				if (ev == TermKeyMouseEvent.TERMKEY_MOUSE_DRAG)
					inputRecord.MouseEvent.dwEventFlags = MouseEventFlags.MOUSE_MOVED;
				inputRecord.MouseEvent.dwMousePosition = new COORD((short) (col - 1), (short) (line - 1));
				if (ev == TermKeyMouseEvent.TERMKEY_MOUSE_RELEASE) {
					inputRecord.MouseEvent.dwButtonState = 0;
				} else if (ev == TermKeyMouseEvent.TERMKEY_MOUSE_DRAG) {
					inputRecord.MouseEvent.dwButtonState = MOUSE_BUTTON_STATE.FROM_LEFT_1ST_BUTTON_PRESSED;
				} else if (ev == TermKeyMouseEvent.TERMKEY_MOUSE_PRESS) {
					inputRecord.MouseEvent.dwButtonState = MOUSE_BUTTON_STATE.FROM_LEFT_1ST_BUTTON_PRESSED;
				}
				//
				processInputEvent(inputRecord);
			}
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
