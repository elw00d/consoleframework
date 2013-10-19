using System;
using System.Collections.Generic;
using ConsoleFramework.Core;
using ConsoleFramework.Native;

namespace ConsoleFramework.Rendering
{
    /// <summary>
    /// Represents the buffer prepared to output into terminal.
    /// Provides indexer-like access to buffer and method <see cref="Flush(ConsoleFramework.Core.Rect)"/>.
    /// </summary>
    public sealed class PhysicalCanvas {
        private readonly IntPtr stdOutputHandle = IntPtr.Zero;
		
		public PhysicalCanvas(int width, int height) {
			this.width = width;
			this.height = height;
			this.buffer = new CHAR_INFO[height, width];
		}
		
        public PhysicalCanvas(int width, int height, IntPtr stdOutputHandle) {
            this.width = width;
            this.height = height;
            this.stdOutputHandle = stdOutputHandle;
            this.buffer = new CHAR_INFO[height, width];
        }

        /// <summary>
        /// Buffer to marshal between application and Win32 API layer.
        /// </summary>
        private CHAR_INFO[,] buffer;

        /// <summary>
        /// Indexers cache to avoid objects creation on every [][] call.
        /// </summary>
        private readonly Dictionary<int, NestedIndexer> cachedIndexers = new Dictionary<int, NestedIndexer>();

        private int width;
        public int Width {
            get {
                return width;
            }
            set {
                if (width != value) {
                    width = value;
                    buffer = new CHAR_INFO[height, width];
                }
            }
        }

        private int height;
        public int Height {
            get {
                return height;
            }
            set {
                if (height != value) {
                    height = value;
                    buffer = new CHAR_INFO[height, width];
                }
            }
        }

        /// <summary>
        /// Flyweight to provide [][]-style access to buffer.
        /// </summary>
        public sealed class NestedIndexer {
            private readonly int x;
            private readonly PhysicalCanvas canvas;
            private readonly Dictionary<int, CHAR_INFO_ref> references = new Dictionary<int, CHAR_INFO_ref>();

            public NestedIndexer(int x, PhysicalCanvas canvas) {
                this.x = x;
                this.canvas = canvas;
            }

            public CHAR_INFO_ref this[int index] {
                get {
                    if (index < 0 || index >= canvas.height) {
                        throw new IndexOutOfRangeException("index exceeds specified buffer height.");
                    }
                    if (references.ContainsKey(index)) {
                        return references[index];
                    }
                    CHAR_INFO_ref res = new CHAR_INFO_ref(x, index, canvas);
                    references[index] = res;
                    return res;
                }
            }

            /// <summary>
            /// Wrapper to provide reference-style access to struct properties (assignment and change
            /// without temporary copying in user code).
            /// </summary>
            public sealed class CHAR_INFO_ref {
                private readonly int x;
                private readonly int y;
                private readonly PhysicalCanvas canvas;

                public CHAR_INFO_ref(int x, int y, PhysicalCanvas canvas) {
                    this.x = x;
                    this.y = y;
                    this.canvas = canvas;
                }

                public char UnicodeChar {
                    get {
                        return canvas.buffer[y, x].UnicodeChar;
                    }
                    set {
                        CHAR_INFO charInfo = canvas.buffer[y, x];
                        charInfo.UnicodeChar = value;
                        canvas.buffer[y, x] = charInfo;
                    }
                }

                public char AsciiChar {
                    get {
                        return canvas.buffer[y, x].AsciiChar;
                    }
                    set {
                        CHAR_INFO charInfo = canvas.buffer[y, x];
                        charInfo.AsciiChar = value;
                        canvas.buffer[y, x] = charInfo;
                    }
                }

                public Attr Attributes {
                    get {
                        return canvas.buffer[y, x].Attributes;
                    }
                    set {
                        CHAR_INFO charInfo = canvas.buffer[y, x];
                        charInfo.Attributes = value;
                        canvas.buffer[y, x] = charInfo;
                    }
                }

                public void Assign(CHAR_INFO charInfo) {
                    canvas.buffer[y, x] = charInfo;
                }

                public void Assign(CHAR_INFO_ref charInfoRef) {
                    canvas.buffer[y, x] = charInfoRef.canvas.buffer[charInfoRef.y, charInfoRef.x];
                }
            }
        }

        public NestedIndexer this[int index] {
            get {
                if (index < 0 || index >= width) {
                    throw new IndexOutOfRangeException("index exceeds specified buffer width.");
                }
                if (cachedIndexers.ContainsKey(index)) {
                    return cachedIndexers[index];
                }
                NestedIndexer res = new NestedIndexer(index, this);
                cachedIndexers[index] = res;
                return res;
            }
        }

        /// <summary>
        /// Writes collected data to console screen buffer.
        /// </summary>
        public void Flush() {
            Flush(new Rect(0, 0, width, height));
        }

        /// <summary>
        /// Writes collected data to console screen buffer, but paints specified rect only.
        /// </summary>
        public void Flush(Rect affectedRect) {
			if (stdOutputHandle != IntPtr.Zero) {
				// we are in windows environment
	            SMALL_RECT rect = new SMALL_RECT((short) affectedRect.x, (short) affectedRect.y,
	                (short) (affectedRect.width + affectedRect.x), (short) (affectedRect.height + affectedRect.y));
	            if (!Win32.WriteConsoleOutputCore(stdOutputHandle, buffer, new COORD((short) width, (short) height),
	                new COORD((short) affectedRect.x, (short) affectedRect.y), ref rect)) {
	                throw new InvalidOperationException(string.Format("Cannot write to console : {0}", Win32.GetLastErrorMessage()));
	            }
			} else {
				// we are in linux
				for (int i = 0; i < affectedRect.width; i++) {
					int x = i + affectedRect.x;
					for (int j = 0; j < affectedRect.height; j++) {
						int y = j + affectedRect.y;
						// todo : convert attributes and optimize rendering
						bool fgIntensity;
						short index = NCurses.winAttrsToNCursesAttrs(buffer[y, x].Attributes,
							out fgIntensity);
						if (fgIntensity) {
							NCurses.attrset(
								(int) (NCurses.COLOR_PAIR(index) | NCurses.A_BOLD));
						} else {
							NCurses.attrset(
								(int) NCurses.COLOR_PAIR(index));
						}
						NCurses.mvaddstr(y, x, new string(buffer[y, x].UnicodeChar,1));
					}
				}
				NCurses.refresh();
			}
        }
    }
}
