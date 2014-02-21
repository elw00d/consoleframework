using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;
using ConsoleFramework.Xaml;

#if !WIN32
using Mono.Unix;
using Mono.Unix.Native;
#endif

namespace ConsoleFramework
{
    public class TerminalSizeChangedEventArgs : EventArgs
    {
        public readonly int Width;
        public readonly int Height;

        public TerminalSizeChangedEventArgs( int width, int height ) {
            Width = width;
            Height = height;
        }
    }

    public delegate void TerminalSizeChangedHandler( object sender, TerminalSizeChangedEventArgs args );

    /// <summary>
    /// Console application entry point.
    /// Encapsulates messages loop and application lifecycle.
    /// Supports Windows and Linux.
    /// </summary>
    public sealed class ConsoleApplication : IDisposable
    {
        public event TerminalSizeChangedHandler TerminalSizeChanged;

        /// <summary>
        /// Default TerminalSizeChanged event handler. Invoked when
        /// initial CanvasSize and RootElementRect are empty and no another
        /// TerminalSizeChanged handler is attached.
        /// </summary>
        public void OnTerminalSizeChangedDefault( object sender, TerminalSizeChangedEventArgs args ) {
            if (!this.userCanvasSize.IsEmpty) throw new InvalidOperationException("Assertion failed.");
            if (!this.userRootElementRect.IsEmpty) throw new InvalidOperationException("Assertion failed.");
            if (this.TerminalSizeChanged != null) throw new InvalidOperationException("Assertion failed.");

            canvas.Size = new Size(args.Width, args.Height);
            renderer.RootElementRect = new Rect(canvas.Size);
            renderer.UpdateRender();
        }

        private Size userCanvasSize;

		/// <summary>
		/// Gets or sets a size of canvas. Whet set, old canvas image will be
		/// copied to new one.
		/// </summary>
        public Size CanvasSize {
            get {
                if ( running && userCanvasSize.IsEmpty )
                    return canvas.Size;
                return userCanvasSize;
            }
            set {
                if ( running && value != canvas.Size ) {
                    canvas.Size = value;
                }
                userCanvasSize = value;
            }
        }

        private Rect userRootElementRect;
		/// <summary>
		/// Gets or sets the root element rect.
		/// When set, root element will be added to invalidation queue automatically.
		/// </summary>
        public Rect RootElementRect {
            get {
                if ( running && userRootElementRect.IsEmpty ) {
                    return renderer.RootElementRect;
                }
                return userRootElementRect;
            }
            set {
                if ( running && value != renderer.RootElementRect ) {
                    renderer.RootElementRect = value;
                }
                userRootElementRect = value;
            }
        }

        private bool running;
        private PhysicalCanvas canvas;

        public static Control LoadFromXaml( string xamlResourceName, object dataContext ) {
            var assembly = Assembly.GetEntryAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(xamlResourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                XamlParser xamlParser = new XamlParser(new List<string>()
                    {
                        "clr-namespace:ConsoleFramework.Xaml;assembly=ConsoleFramework",
                        "clr-namespace:ConsoleFramework.Controls;assembly=ConsoleFramework",
                    });
                return (Control)xamlParser.CreateFromXaml(result, dataContext);
            }
        }

		private static readonly bool usingLinux;
		private static readonly bool isDarwin;

        static ConsoleApplication() {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    usingLinux = false;
                    break;
                case PlatformID.Unix:
					usingLinux = true;
#if !WIN32
					Utsname uname;
					Syscall.uname(out uname);
					if (uname.sysname == "Darwin") {
						isDarwin = true;
					}
#endif
                    break;
                case PlatformID.MacOSX:
                case PlatformID.Xbox:
                    throw new NotSupportedException();
            }
        }
		
        private ConsoleApplication() {
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
        private readonly EventWaitHandle invokeWaitHandle = new EventWaitHandle( false, EventResetMode.ManualReset );
        private int? mainThreadId;
        
        private struct ActionInfo
        {
            public readonly Action action;
            public readonly EventWaitHandle waitHandle;

            public ActionInfo( Action action, EventWaitHandle waitHandle ) {
                this.action = action;
                this.waitHandle = waitHandle;
            }
        }

        private readonly List<ActionInfo> actionsToBeInvoked = new List< ActionInfo >();
        private readonly Object actionsLocker = new object(  );

        /// <summary>
        /// Signals the message loop to be finished.
        /// Application shutdowns after that.
        /// </summary>
        public void Exit() {
			if (usingLinux) {
				int res = Libc.writeInt64(pipeFds[1], 1);
                if (-1 == res) throw new InvalidOperationException("Cannot write to self-pipe.");
			} else {
            	exitWaitHandle.Set();
			}
        }

        private readonly Renderer renderer = new Renderer();
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
        private readonly EventManager eventManager;
        private readonly FocusManager focusManager;

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
			this.running = true;
			try {
				if (usingLinux) {
					runLinux(control);
				} else {
					runWindows(control);
				}
			} finally {
				this.running = false;
			}
		}

        public void Run( Control control, Size canvasSize, Rect rectToUse ) {
			userCanvasSize = canvasSize;
			userRootElementRect = rectToUse;
			Run(control);
        }
		
		/// <summary>
		/// File descriptors for self-pipe.
		/// First descriptor is used to read from pipe, second - to write.
		/// </summary>
		private readonly int[] pipeFds = new int[2];
		private IntPtr termkeyHandle = IntPtr.Zero;

        private void runLinux (Control control) {
			this.mainControl = control;
			
		    if ( userCanvasSize.IsEmpty ) {
		        // Create physical canvas with actual terminal size
		        winsize ws = Libc.GetTerminalSize( isDarwin );
		        canvas = new PhysicalCanvas( ws.ws_col, ws.ws_row );
		    } else {
				canvas = new PhysicalCanvas( userCanvasSize.Width, userCanvasSize.Height );
		    }
		    renderer.Canvas = canvas;
		    if ( userRootElementRect.IsEmpty )
		        renderer.RootElementRect = new Rect( canvas.Size );
		    else
				renderer.RootElementRect = userRootElementRect;
			renderer.RootElement = mainControl;
			// Initialize default focus
			focusManager.AfterAddElementToTree (mainControl);
			//
			mainControl.Invalidate ();
			
			// Terminal initialization sequence
			
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
		    try {
		        renderer.UpdateRender( );

		        termkeyHandle = LibTermKey.termkey_new( 0, TermKeyFlag.TERMKEY_FLAG_SPACESYMBOL );
		        // Setup the input mode
		        Console.Write( "\x1B[?1002h" );
		        pollfd fd = new pollfd( );
		        fd.fd = 0;
		        fd.events = POLL_EVENTS.POLLIN;

		        pollfd[ ] fds = new pollfd[ 2 ];
		        fds[ 0 ] = fd;
		        fds[ 1 ] = new pollfd( );
		        int pipeResult = Libc.pipe( pipeFds );
		        if ( pipeResult == -1 ) {
		            throw new InvalidOperationException( "Cannot create self-pipe." );
		        }
		        fds[ 1 ].fd = pipeFds[ 0 ];
		        fds[ 1 ].events = POLL_EVENTS.POLLIN;

		        try {
#if !WIN32
                    // catch SIGWINCH to handle terminal resizing
			        UnixSignal[] signals = new UnixSignal [] {
			            new UnixSignal (Signum.SIGWINCH)
			        };
			        Thread signal_thread = new Thread (delegate () {
				        while (true) {
					        // Wait for a signal to be delivered
					        int index = UnixSignal.WaitAny (signals, -1);
					        Signum signal = signals [index].Signum;
					        Libc.writeInt64 (pipeFds[1], 2);
				        }
			        }
			        );
			        signal_thread.IsBackground = false;
			        signal_thread.Start ();
#endif
		            TermKeyKey key = new TermKeyKey( );
		            while ( true ) {
		                int pollRes = Libc.poll( fds, 2, -1 );
		                if ( pollRes == 0 ) throw new InvalidOperationException( "Assertion failed." );
                        if ( pollRes == -1 ) {
                            int errorCode = Marshal.GetLastWin32Error();
                            if ( errorCode != Libc.EINTR ) {
                                throw new InvalidOperationException(string.Format("poll() returned with error code {0}", errorCode));
                            }
                        }

		                if ( fds[ 1 ].revents != POLL_EVENTS.NONE ) {
		                    UInt64 u;
		                    Libc.readInt64( fds[ 1 ].fd, out u );
		                    if ( u == 1 ) {
		                        // Exit from application
#if !WIN32
						        signal_thread.Abort ();
#endif
		                        break;
		                    }
		                    if ( u == 2 ) {
		                        // get new term size and process appropriate INPUT_RECORD event
		                        INPUT_RECORD inputRecord = new INPUT_RECORD( );
		                        inputRecord.EventType = EventType.WINDOW_BUFFER_SIZE_EVENT;

		                        winsize ws = Libc.GetTerminalSize( isDarwin );

		                        inputRecord.WindowBufferSizeEvent.dwSize.X = ( short ) ws.ws_col;
		                        inputRecord.WindowBufferSizeEvent.dwSize.Y = ( short ) ws.ws_row;
		                        processInputEvent( inputRecord );
		                    }
		                }

		                if ( ( fds[ 0 ].revents & POLL_EVENTS.POLLIN ) == POLL_EVENTS.POLLIN ||
		                     ( fds[ 0 ].revents & POLL_EVENTS.POLLHUP ) == POLL_EVENTS.POLLHUP ||
		                     ( fds[ 0 ].revents & POLL_EVENTS.POLLERR ) == POLL_EVENTS.POLLERR ) {
		                    LibTermKey.termkey_advisereadable( termkeyHandle );
		                }

		                while ( ( LibTermKey.termkey_getkey( termkeyHandle, ref key ) ) == TermKeyResult.TERMKEY_RES_KEY ) {
		                    processLinuxInput( key );
		                    renderer.UpdateRender( );
		                }
		            }

		        } finally {
		            LibTermKey.termkey_destroy( termkeyHandle );
		            Libc.close( pipeFds[ 0 ] );
		            Libc.close( pipeFds[ 1 ] );
		            Console.Write( "\x1B[?1002l" );
		        }
		    } finally {
		        // Restore cursor visibility before exit
		        ShowCursor( );
		        NCurses.endwin( );
		    }
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
            this.mainThreadId = Thread.CurrentThread.ManagedThreadId;
            //
            stdInputHandle = Win32.GetStdHandle(StdHandleType.STD_INPUT_HANDLE);
            stdOutputHandle = Win32.GetStdHandle(StdHandleType.STD_OUTPUT_HANDLE);
            IntPtr[] handles = new[] {
                exitWaitHandle.SafeWaitHandle.DangerousGetHandle(),
                stdInputHandle,
                invokeWaitHandle.SafeWaitHandle.DangerousGetHandle(  )
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

            if ( userCanvasSize.IsEmpty ) {
                canvas = new PhysicalCanvas( screenBufferInfo.dwSize.X, screenBufferInfo.dwSize.Y, stdOutputHandle );
            } else {
				canvas = new PhysicalCanvas( userCanvasSize.Width, userCanvasSize.Height, stdOutputHandle);
            }
            renderer.Canvas = canvas;
            // fill the canvas by default
            if ( userRootElementRect.IsEmpty ) {
                renderer.RootElementRect = new Rect( new Point(0, 0), canvas.Size );
            } else {
				renderer.RootElementRect = userRootElementRect;
			}
			renderer.RootElement = mainControl;
            // initialize default focus
            focusManager.AfterAddElementToTree(mainControl);
            //
            mainControl.Invalidate();
            renderer.UpdateRender();

            // initially hide the console cursor
            HideCursor();
            
            while (true) {
                uint waitResult = Win32.WaitForMultipleObjects(3, handles, false, Win32.INFINITE);
                if (waitResult == 0) {
                    break;
                }
                if (waitResult == 1) {
                    processInput();
                    processInvokeActions( );
                    renderer.UpdateRender();
                    continue;
                }
                if ( waitResult == 2 ) {
                    processInvokeActions( );
                    renderer.UpdateRender(  );
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

        private void processInvokeActions( ) {
            for ( ;; ) {
                ActionInfo top;
                lock ( actionsLocker ) {
                    if ( actionsToBeInvoked.Count != 0 ) {
                        top = actionsToBeInvoked[ 0 ];
                        actionsToBeInvoked.RemoveAt( 0 );
                    } else {
                        break;
                    }
                }
                top.action.Invoke(  );
                if ( top.waitHandle != null ) {
                    top.waitHandle.Set( );
                }
            }
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

				if ( usingLinux ) {
					// Reinitializing ncurses to deal with new dimensions
					// http://stackoverflow.com/questions/13707137/ncurses-resizing-glitch
					NCurses.endwin();
					// Needs to be called after an endwin() so ncurses will initialize
					// itself with the new terminal dimensions.
					NCurses.refresh();
					NCurses.clear();
				}
				
				COORD dwSize = inputRecord.WindowBufferSizeEvent.dwSize;

				// Invoke default handler if no custom handlers attached and
				// userCanvasSize and userRootElementRect are not defined
                if ( TerminalSizeChanged == null
                     && userCanvasSize.IsEmpty
                     && userRootElementRect.IsEmpty ) {
                    OnTerminalSizeChangedDefault(this, new TerminalSizeChangedEventArgs( dwSize.X, dwSize.Y ));
                } else if ( TerminalSizeChanged != null ) {
					TerminalSizeChanged.Invoke(this, new TerminalSizeChangedEventArgs(dwSize.X, dwSize.Y));					
                }

				// Refresh whole display
				renderer.UpdateRender( true );

                return;
            }
            eventManager.ProcessInput(inputRecord, mainControl);
        }

        /// <summary>
        /// Checks if current thread is same thread from which Run() method
        /// was called.
        /// todo : add Linux support
        /// </summary>
        /// <returns></returns>
        public bool IsUiThread( ) {
            return Thread.CurrentThread.ManagedThreadId == this.mainThreadId;
        }

        /// <summary>
        /// Invokes action in UI thread synchronously.
        /// If run loop was not started yet, nothing will be done.
        /// todo : add Linux support
        /// </summary>
        /// <param name="action"></param>
        public void RunOnUiThread( Action action ) {
            // If run loop is not started, do nothing
            if ( this.mainThreadId == null ) {
                return;
            }
            // If current thread is UI thread, invoke action directly
            if ( IsUiThread( ) ) {
                action.Invoke(  );
                return;
            }
            using ( EventWaitHandle waitHandle = new EventWaitHandle( false, EventResetMode.ManualReset ) ) {
                lock ( actionsLocker ) {
                    actionsToBeInvoked.Add( new ActionInfo( action, waitHandle ) );
                }
                invokeWaitHandle.Set( );
                waitHandle.WaitOne( );
            }
        }

        /// <summary>
        /// Invokes action in main loop thread asynchronously.
        /// If run loop was not started yet, nothing will be done.
        /// todo : add Linux support
        /// </summary>
        /// <param name="action"></param>
        public void Post( Action action ) {
            // If run loop is not started, nothing to do
            if ( this.mainThreadId == null ) {
                return;
            }
            lock ( actionsLocker ) {
                actionsToBeInvoked.Add( new ActionInfo( action, null ) );
            }
            if (!IsUiThread())
                invokeWaitHandle.Set();
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
