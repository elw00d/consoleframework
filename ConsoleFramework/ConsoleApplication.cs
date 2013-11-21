using System;
using System.Threading;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;

#if !WIN32
using Mono.Unix;
#endif

namespace ConsoleFramework
{
    /// <summary>
    /// Console application entry point.
    /// Encapsulates messages loop and application lifecycle.
    /// Supports Windows and Linux.
    /// </summary>
    public sealed class ConsoleApplication : IDisposable {
		
		private bool usingLinux = false;
		
        private ConsoleApplication(bool usingLinux) {
			this.usingLinux = usingLinux;
            eventManager = new EventManager();
            focusManager = new FocusManager(eventManager);
        }

        private static volatile ConsoleApplication instance;
        private static readonly object syncRoot = new object();

        /// <summary>
        /// Instance of Application object.
        /// </summary>
        public static ConsoleApplication Instance {
            get {
                if (instance == null) {
                    lock (syncRoot) {
                        if (instance == null) {
                            instance = new ConsoleApplication(false);
                        }
                    }
                }
                return instance;
            }
        }

        private IntPtr stdInputHandle;
        private IntPtr stdOutputHandle;
        private readonly EventWaitHandle exitWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        /// <summary>
        /// Signals the message loop to be finished.
        /// Application shutdowns after that.
        /// </summary>
        public void Exit() {
			if (usingLinux) {
				int res = Libc.writeInt64(eventfd, 1);
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

        /// <summary>
        /// Returns the root control of the application.
        /// </summary>
        public Control RootControl {
            get { return mainControl; }
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
		
        internal void SetCursorPosition (Point position)
		{
			if (!usingLinux) {				
				Win32.SetConsoleCursorPosition (stdOutputHandle, new COORD ((short)position.x, (short)position.y));
			} else {
				NCurses.move (position.y, position.x);
				NCurses.refresh ();
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
        /// Делает курсор консоли видимым и устанавливает значение CursorIsVisible в true.
        /// </summary>
        internal void ShowCursor ()
		{
			if (!usingLinux) {
				CONSOLE_CURSOR_INFO consoleCursorInfo = new CONSOLE_CURSOR_INFO {
	                Size = 5,
	                Visible = true
	            };
				Win32.SetConsoleCursorInfo (stdOutputHandle, ref consoleCursorInfo);
			} else {
				NCurses.curs_set (CursorVisibility.Visible);
			}
            CursorIsVisible = true;
        }

        /// <summary>
        /// Делает курсор консоли невидимым и устанавливает значение
        /// CursorIsVisible в false.
        /// </summary>
        internal void HideCursor ()
		{
			if (!usingLinux) {
				CONSOLE_CURSOR_INFO consoleCursorInfo = new CONSOLE_CURSOR_INFO {
	                Size = 5,
	                Visible = false
	            };
				Win32.SetConsoleCursorInfo (stdOutputHandle, ref consoleCursorInfo);
			} else {
				NCurses.curs_set (CursorVisibility.Invisible);
			}
            CursorIsVisible = false;
        }
		
        /// <summary>
        /// Runs application using specified control as root control.
        /// Application will run until method <see cref="Exit"/> is called.
        /// </summary>
        /// <param name="control"></param>
		public void Run(Control control) {
			if (usingLinux) {
				runLinux(control);
			} else {
				runWindows(control);
			}
		}
		
		private int eventfd = -1;
		private IntPtr termkeyHandle = IntPtr.Zero;
		
		private void runLinux (Control control)
		{
			this.mainControl = control;
			//
			PhysicalCanvas canvas = new PhysicalCanvas (100, 35);
			renderer.Canvas = canvas;
			renderer.RootElementRect = new Rect (0, 0, 80, 25);
			renderer.RootElement = mainControl;
			// initialize default focus
			focusManager.AfterAddElementToTree (mainControl);
			//
			mainControl.Invalidate ();
			
			// terminal initialization sequence
			
			// This is magic workaround to avoid messing up terminal after program finish
			// The bug is described at https://bugzilla.xamarin.com/show_bug.cgi?id=15118
			bool ignored = Console.KeyAvailable;
			
			IntPtr stdscr = NCurses.initscr ();
			NCurses.cbreak ();
			NCurses.noecho ();
			NCurses.nonl ();
			NCurses.intrflush (stdscr, false);
			NCurses.keypad (stdscr, true);
			NCurses.start_color ();
			
			HideCursor ();
			renderer.UpdateRender ();
			
			termkeyHandle = LibTermKey.termkey_new (0, TermKeyFlag.TERMKEY_FLAG_SPACESYMBOL);
			// setup the input mode
			Console.Write ("\x1B[?1002h");
			pollfd fd = new pollfd ();
			fd.fd = 0;
			fd.events = POLL_EVENTS.POLLIN;
			
			pollfd[] fds = new pollfd[2];
			fds [0] = fd;
			
			fds [1] = new pollfd ();
			eventfd = Libc.eventfd (0, EVENTFD_FLAGS.EFD_CLOEXEC);
			if (eventfd == -1) {
				Console.WriteLine ("Cannot create eventfd\n");
				int lastError = System.Runtime.InteropServices.Marshal.GetLastWin32Error ();
				Console.WriteLine ("Last error is {0}\n", lastError);
			}	
			fds [1].fd = eventfd;
			fds [1].events = POLL_EVENTS.POLLIN;
			
#if !WIN32
			// catch SIGWINCH to handle terminal resizing
			UnixSignal[] signals = new UnixSignal [] {
			    new UnixSignal (Mono.Unix.Native.Signum.SIGWINCH)
			};
			Thread signal_thread = new Thread (delegate () {
				while (true) {
					// Wait for a signal to be delivered
					int index = UnixSignal.WaitAny (signals, -1);
					Mono.Unix.Native.Signum signal = signals [index].Signum;
					Libc.writeInt64 (eventfd, 2);
				}
			}
			);
			signal_thread.IsBackground = false;
			signal_thread.Start ();
#endif
			
			TermKeyKey key = new TermKeyKey ();
			while (true) {
				int pollRes = Libc.poll (fds, 2, -1);
				if (0 == pollRes) {
					// timed out
					Console.WriteLine ("Timed out");
					if (LibTermKey.termkey_getkey_force (termkeyHandle, ref key) == TermKeyResult.TERMKEY_RES_KEY) {
						Console.WriteLine ("got TERMKEY_RES_KEY");
					}					
				} else if (-1 == pollRes) {
					int errorCode = System.Runtime.InteropServices.Marshal.GetLastWin32Error ();
					//Console.WriteLine(string.Format("ErrorCode = {0}", errorCode));
					// todo : write to debug console if error code differs from 4
				}
				
				if (fds [1].revents != POLL_EVENTS.NONE) {
					UInt64 u;
					Libc.readInt64 (fds [1].fd, out u);
					//Console.WriteLine ("Readed eventfd counter : {0}\n", u);
					if (u == 1) {
						// exit from application
#if !WIN32
						signal_thread.Abort ();
#endif
						break;
					}
					if (u == 2) {
						// reinitializing ncurses to deal with new dimensions
						// http://stackoverflow.com/questions/13707137/ncurses-resizing-glitch
						NCurses.endwin ();
						// Needs to be called after an endwin() so ncurses will initialize
						// itself with the new terminal dimensions.
						NCurses.refresh ();
						NCurses.clear ();
						
						// get new term size and process appropriate INPUT_RECORD event
						INPUT_RECORD inputRecord = new INPUT_RECORD ();
						inputRecord.EventType = EventType.WINDOW_BUFFER_SIZE_EVENT;
						
						winsize ws;
						Libc.ioctl (Libc.STDIN_FILENO, Libc.TIOCGWINSZ, out ws);
						
						inputRecord.WindowBufferSizeEvent.dwSize.X = (short)ws.ws_col;
						inputRecord.WindowBufferSizeEvent.dwSize.Y = (short)ws.ws_row;
						processInputEvent (inputRecord);
						
						renderer.UpdateRender ();
					}
				}
				
				if ((fds [0].revents & POLL_EVENTS.POLLIN) == POLL_EVENTS.POLLIN ||
					(fds [0].revents & POLL_EVENTS.POLLHUP) == POLL_EVENTS.POLLHUP ||
					(fds [0].revents & POLL_EVENTS.POLLERR) == POLL_EVENTS.POLLERR) {
					// todo : log return value
					LibTermKey.termkey_advisereadable (termkeyHandle);
				}
				
				TermKeyResult result;
				while ((result = LibTermKey.termkey_getkey(termkeyHandle, ref key)) == TermKeyResult.TERMKEY_RES_KEY) {
					processLinuxInput (key);
					renderer.UpdateRender ();
				}
			}
			
			LibTermKey.termkey_destroy (termkeyHandle);
			Libc.close (eventfd);
			Console.Write ("\x1B[?1002l");
			
			// restore cursor visibility before exit
			ShowCursor ();
			NCurses.endwin ();
		}
		
		private void processLinuxInput (TermKeyKey key)
		{
			// if any special button has been pressed (Tab, Enter, etc)
			// we should convert its code to INPUT_RECORD.KeyEvent
			// Because INPUT_RECORD.KeyEvent depends on Windows' scan codes,
			// we convert codes retrieved from LibTermKey to Windows virtual scan codes
			// In the future, this logic may be changed (for example, both Windows and Linux
			// raw codes can be converted into ConsoleFramework's own abstract enum)
			if (key.type == TermKeyType.TERMKEY_TYPE_KEYSYM) {
				INPUT_RECORD inputRecord = new INPUT_RECORD ();
				inputRecord.EventType = EventType.KEY_EVENT;
				inputRecord.KeyEvent.bKeyDown = true;
				inputRecord.KeyEvent.wRepeatCount = 1;
				switch (key.code.sym) {
				case TermKeySym.TERMKEY_SYM_TAB:
					inputRecord.KeyEvent.wVirtualKeyCode = 0x09;
					break;
				case TermKeySym.TERMKEY_SYM_ENTER:
					inputRecord.KeyEvent.wVirtualKeyCode = 0x0D;
					break;
				// in gnome-terminal it is backspace by default
				// (see default compatibility settings in Profile's settings)
				case TermKeySym.TERMKEY_SYM_DEL:
				case TermKeySym.TERMKEY_SYM_BACKSPACE:
					inputRecord.KeyEvent.wVirtualKeyCode = 0x08;
					break;
				case TermKeySym.TERMKEY_SYM_DELETE:
					inputRecord.KeyEvent.wVirtualKeyCode = 0x2E;
					break;
				case TermKeySym.TERMKEY_SYM_HOME:
					inputRecord.KeyEvent.wVirtualKeyCode = 0x24;
					break;
				case TermKeySym.TERMKEY_SYM_END:
					inputRecord.KeyEvent.wVirtualKeyCode = 0x23;
					break;
				case TermKeySym.TERMKEY_SYM_PAGEUP:
					inputRecord.KeyEvent.wVirtualKeyCode = 0x21;
					break;
				case TermKeySym.TERMKEY_SYM_PAGEDOWN:
					inputRecord.KeyEvent.wVirtualKeyCode = 0x22;
					break;
				case TermKeySym.TERMKEY_SYM_SPACE:
					inputRecord.KeyEvent.UnicodeChar = ' ';
					inputRecord.KeyEvent.wVirtualKeyCode = 0x20;
					break;
				case TermKeySym.TERMKEY_SYM_ESCAPE:
					inputRecord.KeyEvent.wVirtualKeyCode = 0x1B;
					break;
				case TermKeySym.TERMKEY_SYM_INSERT:
					inputRecord.KeyEvent.wVirtualKeyCode = 0x2D;
					break;
				case TermKeySym.TERMKEY_SYM_UP:
					inputRecord.KeyEvent.wVirtualKeyCode = 0x26;
					break;
				case TermKeySym.TERMKEY_SYM_DOWN:
					inputRecord.KeyEvent.wVirtualKeyCode = 0x28;
					break;
				case TermKeySym.TERMKEY_SYM_LEFT:
					inputRecord.KeyEvent.wVirtualKeyCode = 0x25;
					break;
				case TermKeySym.TERMKEY_SYM_RIGHT:
					inputRecord.KeyEvent.wVirtualKeyCode = 0x27;
					break;
				default:
					throw new NotSupportedException ("Not supported keyboard code detected: " + key.code.sym);
				}
				inputRecord.KeyEvent.dwControlKeyState = 0;
				if ((key.modifiers & 4) == 4) {
					inputRecord.KeyEvent.dwControlKeyState |= ControlKeyState.LEFT_CTRL_PRESSED;
				}
				if ((key.modifiers & 2) == 2) {
					inputRecord.KeyEvent.dwControlKeyState |= ControlKeyState.LEFT_ALT_PRESSED;
				}
				processInputEvent (inputRecord);
			} else if (key.type == TermKeyType.TERMKEY_TYPE_UNICODE) {
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
                // todo : remove hardcoded exit combo after testing
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
		
        private void runWindows(Control control) {
            this.mainControl = control;
            //
            stdInputHandle = Win32.GetStdHandle(StdHandleType.STD_INPUT_HANDLE);
            stdOutputHandle = Win32.GetStdHandle(StdHandleType.STD_OUTPUT_HANDLE);
            IntPtr[] handles = new[] {
                exitWaitHandle.SafeWaitHandle.DangerousGetHandle(),
                stdInputHandle
            };

            // set console mode to enable mouse and window resizing events
            const uint ENABLE_WINDOW_INPUT = 0x0008;
            const uint ENABLE_MOUSE_INPUT = 0x0010;
            uint consoleMode;
            Win32.GetConsoleMode( stdInputHandle, out consoleMode );
            Win32.SetConsoleMode(stdInputHandle, consoleMode | ENABLE_MOUSE_INPUT | ENABLE_WINDOW_INPUT);

            // get console screen buffer size
            CONSOLE_SCREEN_BUFFER_INFO screenBufferInfo;
            Win32.GetConsoleScreenBufferInfo( stdOutputHandle, out screenBufferInfo );

            PhysicalCanvas canvas = new PhysicalCanvas(screenBufferInfo.dwSize.X, screenBufferInfo.dwSize.Y, stdOutputHandle);
            renderer.Canvas = canvas;
            // fill the canvas by default
            renderer.RootElementRect = new Rect(0, 0, canvas.Width, canvas.Height);
            renderer.RootElement = mainControl;
            // initialize default focus
            focusManager.AfterAddElementToTree(mainControl);
            //
            mainControl.Invalidate();
            renderer.UpdateRender();

            // initially hide the console cursor
            HideCursor();
            
            while (true) {
                uint waitResult = Win32.WaitForMultipleObjects(2, handles, false, Win32.INFINITE);
                if (waitResult == 0) {
                    break;
                }
                if (waitResult == 1) {
                    processInput();
                    renderer.UpdateRender();
                    continue;
                }
                // if we received WAIT_TIMEOUT or WAIT_FAILED
                if (waitResult == 0x00000102 || waitResult == 0xFFFFFFFF) {
                    throw new InvalidOperationException("Invalid wait result of WaitForMultipleObjects.");
                }
            }

            // restore cursor visibility before exit
            ShowCursor();

            // restore console mode before exit
            Win32.SetConsoleMode( stdInputHandle, consoleMode );

            // todo : restore attributes of console output
        }

        private void processInput() {
            INPUT_RECORD[] buffer = new INPUT_RECORD[10];
            uint read;
            bool bReaded = Win32.ReadConsoleInput(stdInputHandle, buffer, (uint) buffer.Length, out read);
            if (!bReaded) {
                throw new InvalidOperationException("ReadConsoleInput method failed.");
            }
            for (int i = 0; i < read; ++i) {
                processInputEvent(buffer[i]);
            }
        }

        private void processInputEvent(INPUT_RECORD inputRecord) {
            if ( inputRecord.EventType == EventType.WINDOW_BUFFER_SIZE_EVENT ) {
                COORD dwSize = inputRecord.WindowBufferSizeEvent.dwSize;
                renderer.Canvas.Width = dwSize.X;
                renderer.Canvas.Height = dwSize.Y;
                renderer.RootElementRect = new Rect(0, 0, dwSize.X, dwSize.Y);
                return;
            }
            eventManager.ProcessInput(inputRecord, mainControl, renderer.RootElementRect);
        }

        /// <summary>
        /// Начинает захват мыши и маршрутизируемых событий
        /// указанным элементом управления. После этого контрол принимает все события от мыши
        /// в качестве источника события (вне зависимости от позиции курсора мыши), а все маршрутизируемые
        /// события передаются только в этот контрол и к его потомкам.
        /// Используется, например, при обработке клика на кнопке - после нажатия ввод захватывается, и
        /// события приходят только в кнопку. Когда пользователь отпускает кнопку мыши, захват прекращается.
        /// </summary>
        public void BeginCaptureInput(Control control) {
            eventManager.BeginCaptureInput(control);
        }

        /// <summary>
        /// Завершает захват мыши и маршрутизируемых событий.
        /// </summary>
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
