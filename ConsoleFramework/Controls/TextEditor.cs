using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;
using Xaml;

// TODO : Autoindent
// TODO : Ctrl+Home/Ctrl+End
// TODO : Alt+Backspace deletes word
// TODO : Shift+Delete deletes line
// TODO : Scrollbars full support
// TODO : Ctrl+arrows
// TODO : Selection
// TODO : Selection copy/paste/cut/delete
// TODO : Undo/Redo, commands autogrouping
// TODO : Read only mode
// TODO : Tabs (converting to spaces when loading?)
namespace ConsoleFramework.Controls {
    /// <summary>
    /// Incapsulates text holder and all the data required to display the content
    /// properly in predictable way. Should be covered by unit tests. Unit tests
    /// can be written easy using steps like theese:
    /// 1. Initialize using some text and initial cursorPos, window values
    /// 2. Apply some commands
    /// 3. Check the result state
    /// </summary>
    public class TextEditorController {
        /// <summary>
        /// Gap shown after scrolling to the very end of document
        /// </summary>
        public const int LINES_BOTTOM_MAX_GAP = 4;

        /// <summary>
        /// Gap shown after typing last character in line if there is no remaining space
        /// (and when End key was pressed)
        /// </summary>
        public const int COLUMNS_RIGHT_MAX_GAP = 3;

        /// <summary>
        /// Gap to left char of line when returning to the line which was out of window
        /// (if window moves from right to left)
        /// </summary>
        public const int COLUMNS_LEFT_GAP = 4;

        /// <summary>
        /// Logical cursor position (points to symbol in textItems, not to display coord)
        /// </summary>
        public Point CursorPos {
            get => cursorPos;
            set {
                cursorPos = value;
                lastTextPosX = cursorPosToTextPos(cursorPos, Window).X;
            }
        }

        /// <summary>
        /// Stores the last X coord of cursor, before line was changed
        /// (when PageUp/PageDown/ArrowUp/ArrowDown pressed)
        /// </summary>
        private int lastTextPosX;

        /// <summary>
        /// Changes cursor position without changing lastCursorX value
        /// </summary>
        private void setCursorPosLight(Point cursorPos) {
            this.cursorPos = cursorPos;
        }

        /// <summary>
        /// Current display window
        /// </summary>
        public Rect Window { get; set; }

        /// <summary>
        /// Current text in editor
        /// </summary>
        private TextHolder textHolder;

        private Point cursorPos;

        public void WriteToWindow(char[,] buffer) {
            textHolder.WriteToWindow(Window.Left, Window.Top, Window.Width, Window.Height, buffer);
        }

        // TODO : property
        private string newLine;

        public string Text {
            get => textHolder.Text;
            set {
                if (textHolder.Text != value) {
                    string newLineToUse;
                    if (newLine == null) {
                        // Auto-detect newline format
                        newLineToUse = detectNewLine(value);
                    } else {
                        newLineToUse = newLine;
                    }
                    textHolder = new TextHolder(value, newLineToUse);
                    CursorPos = new Point();
                    Window = new Rect(new Point(), Window.Size);
                }
            }
        }

        /// <summary>
        /// Since XamlReader passes \n-delimited lines in all platforms
        /// (without looking what really is in CDATA, for example), we should provide
        /// auto-detection feature according to the principle of least astonishment.
        /// Then, one xaml-file can be used in different platforms without changes.
        /// </summary>
        private string detectNewLine(string text) {
            if (text.Contains("\r\n")) {
                return "\r\n";
            }

            return "\n";
        }

        public int LinesCount => textHolder.LinesCount;
        public int ColumnsCount => textHolder.ColumnsCount;

        public TextEditorController(string text, int width, int height) :
            this(new TextHolder(text), new Point(), new Rect(0, 0, width, height)) {
        }

        public TextEditorController(TextHolder textHolder, Point cursorPos, Rect window) {
            this.textHolder = textHolder;
            this.CursorPos = cursorPos;
            this.Window = window;
        }

        public interface ICommand {
            /// <summary>
            /// Returns true if visible content has changed during the operation
            /// (and therefore should be invalidated), false otherwise.
            /// </summary>
            bool Do(TextEditorController controller);
        }

        static Point cursorPosToTextPos(Point cursorPos, Rect window) {
            cursorPos.Offset(window.X, window.Y);
            return cursorPos;
        }

        static Point textPosToCursorPos(Point textPos, Rect window) {
            textPos.Offset(-window.X, -window.Y);
            return textPos;
        }

        public class AppendStringCmd : ICommand {
            private readonly string s;

            public AppendStringCmd(string s) {
                this.s = s;
            }

            public bool Do(TextEditorController controller) {
                Point textPos = cursorPosToTextPos(controller.CursorPos, controller.Window);
                Point nextCharPos =
                    controller.textHolder.Insert(textPos.Y, textPos.X, s);

                // Move window to just edited place if need
                Point cursor = textPosToCursorPos(nextCharPos, controller.Window);

                moveWindowToCursor(cursor, controller);

                return true;
            }
        }

        /// <summary>
        /// Moves window to make the cursor visible in it
        /// </summary>
        static void moveWindowToCursor(Point cursor, TextEditorController controller, bool light = false) {
            Rect oldWindow = controller.Window;

            int? windowX;
            int? windowY;

            if (cursor.X >= oldWindow.Width) {
                // Move window 3px right if nextChar is outside the window after add char
                windowX = oldWindow.X + cursor.X - oldWindow.Width + COLUMNS_RIGHT_MAX_GAP;
            } else if (cursor.X < 0) {
                // Move window left if need (with 4px gap from left)
                windowX = Math.Max(0, oldWindow.X + cursor.X - COLUMNS_LEFT_GAP);
            } else {
                windowX = null;
            }

            // Move window down if nextChar is outside the window
            if (cursor.Y >= controller.Window.Height) {
                windowY = controller.Window.Top + cursor.Y - controller.Window.Height + 1;
            } else if (cursor.Y < 0) {
                windowY = controller.Window.Y + cursor.Y;
            } else {
                windowY = null;
            }

            if (windowX != null || windowY != null) {
                controller.Window = new Rect(
                    new Point(windowX ?? oldWindow.X, windowY ?? oldWindow.Y), oldWindow.Size);
            }

            // Actualize cursor position to new window
            Point cursorPos = textPosToCursorPos(cursorPosToTextPos(cursor, oldWindow), controller.Window);
            if (light) {
                controller.setCursorPosLight(cursorPos);
            } else {
                controller.CursorPos = cursorPos;
            }
        }

        public enum Direction {
            Up,
            Down,
            Left,
            Right
        }

        public class EndCommand : ICommand {
            public bool Do(TextEditorController controller) {
                Point oldCursorPos = controller.CursorPos;
                Rect oldWindow = controller.Window;
                Point oldTextPos = cursorPosToTextPos(oldCursorPos, oldWindow);

                string line = controller.textHolder.Lines[oldTextPos.Y];
                Point textPos = new Point(line.Length, oldTextPos.Y);
                
                moveWindowToCursor(textPosToCursorPos(textPos, oldWindow), controller);

                return oldWindow != controller.Window || oldCursorPos != controller.CursorPos;
            }
        }
        
        public class HomeCommand : ICommand {
            public bool Do(TextEditorController controller) {
                Point oldCursorPos = controller.CursorPos;
                Rect oldWindow = controller.Window;
                Point oldTextPos = cursorPosToTextPos(oldCursorPos, oldWindow);

                string line = controller.textHolder.Lines[oldTextPos.Y];
                int homeIndex = homeOfLine(line);
                int x;
                if (oldTextPos.X == 0) {
                    x = homeIndex;
                } else {
                    x = oldTextPos.X <= homeIndex ? 0 : homeIndex;
                }
                Point textPos = new Point(x, oldTextPos.Y);

                moveWindowToCursor(textPosToCursorPos(textPos, oldWindow), controller);

                return oldWindow != controller.Window || oldCursorPos != controller.CursorPos;
            }

            /// <summary>
            /// Returns index of first non-space symbol (or s.Length if there is no non-space symbols)
            /// </summary>
            private int homeOfLine(string s) {
                for (int i = 0; i < s.Length; i++) {
                    if (!char.IsWhiteSpace(s[i])) {
                        return i;
                    }
                }
                return s.Length;
            }
        }
        
        
        
        public class PageUpCmd : ICommand {
            public bool Do(TextEditorController controller) {
                Point oldCursorPos = controller.CursorPos;
                Rect oldWindow = controller.Window;
                Point oldTextPos = cursorPosToTextPos(oldCursorPos, oldWindow);

                // Scroll window one page up
                if (controller.Window.Y > 0) {
                    int y = Math.Max(0, controller.Window.Y - controller.Window.Height);
                    controller.Window = new Rect(new Point(controller.Window.X, y), controller.Window.Size);
                }
                
                // Move cursor up too
                Rect window = controller.Window;
                Point textPos;
                if (oldTextPos.Y == 0) {
                    textPos = oldTextPos.X == 0 ? oldTextPos : new Point(0, 0);
                } else {
                    int lineIndex = Math.Max(0, oldTextPos.Y - window.Height);
                    string line = controller.textHolder.Lines[lineIndex];
                    int x;
                    if (oldTextPos.Y - window.Height < 0) {
                        x = 0;
                    } else {
                        x = Math.Min(controller.lastTextPosX, line.Length);
                    }
                    textPos = new Point(x, lineIndex);
                }

                // Actualize cursor
                moveWindowToCursor(textPosToCursorPos(textPos, window), controller, true);

                return oldWindow != controller.Window || oldCursorPos != controller.CursorPos;
            }
        }
        
        public class PageDownCmd : ICommand {
            public bool Do(TextEditorController controller) {
                Point oldCursorPos = controller.CursorPos;
                Rect oldWindow = controller.Window;
                Point oldTextPos = cursorPosToTextPos(oldCursorPos, oldWindow);

                // Scroll window one page down
                int maxWindowY = controller.textHolder.LinesCount + LINES_BOTTOM_MAX_GAP - controller.Window.Height;
                if (controller.Window.Y < maxWindowY) {
                    int y = Math.Min(controller.Window.Y + controller.Window.Height, maxWindowY);
                    controller.Window = new Rect(new Point(controller.Window.X, y), controller.Window.Size);
                }
                
                // Move cursor down too
                Rect window = controller.Window;
                Point textPos;
                if (oldTextPos.Y == controller.textHolder.LinesCount - 1) {
                    string lastLine = controller.textHolder.Lines[controller.textHolder.LinesCount - 1];
                    if (oldTextPos.X == lastLine.Length) {
                        textPos = oldTextPos;
                    } else {
                        textPos = new Point(lastLine.Length, controller.textHolder.LinesCount - 1);
                    }
                } else {
                    int lineIndex = Math.Min(oldTextPos.Y + window.Height, controller.LinesCount - 1);
                    string line = controller.textHolder.Lines[lineIndex];
                    int x;
                    if (oldTextPos.Y + window.Height > lineIndex) {
                        x = line.Length;
                    } else {
                        x = Math.Min(controller.lastTextPosX, line.Length);
                    }
                    textPos = new Point(x, lineIndex);
                }

                // Actualize cursor
                moveWindowToCursor(textPosToCursorPos(textPos, window), controller, true);

                return oldWindow != controller.Window || oldCursorPos != controller.CursorPos;
            }
        }

        /// <summary>
        /// Command accepts coord from mouse and applies it to current text window,
        /// don't allowing to set cursor out of filled text
        /// </summary>
        public class TrySetCursorCmd : ICommand {
            private readonly Point coord;

            public TrySetCursorCmd(Point coord) {
                this.coord = coord;
            }

            public bool Do(TextEditorController controller) {
                if (!new Rect(new Point(), controller.Window.Size).Contains(coord)) {
                    throw new ArgumentException("coord should be inside window");
                }

                Point desiredTextPos = cursorPosToTextPos(coord, controller.Window);
                int y = Math.Min(desiredTextPos.Y, controller.textHolder.LinesCount - 1);
                int x = Math.Min(desiredTextPos.X, controller.textHolder.Lines[y].Length);

                moveWindowToCursor(textPosToCursorPos(new Point(x, y), controller.Window), controller);

                return false;
            }
        }

        /// <summary>
        /// Initiated with BackSpace key
        /// </summary>
        public class DeleteLeftSymbolCmd : ICommand {
            public bool Do(TextEditorController controller) {
                Point toTextPos = cursorPosToTextPos(controller.CursorPos, controller.Window);
                Point fromTextPos;
                if (toTextPos.X == 0) {
                    if (toTextPos.Y == 0) {
                        return false;
                    }
                    fromTextPos = new Point(controller.textHolder.Lines[toTextPos.Y - 1].Length, toTextPos.Y - 1);
                } else {
                    fromTextPos = new Point(toTextPos.X - 1, toTextPos.Y);
                }

                controller.textHolder.Delete(fromTextPos.Y, fromTextPos.X, toTextPos.Y, toTextPos.X);
                moveWindowToCursor(textPosToCursorPos(fromTextPos, controller.Window), controller);

                return true;
            }
        }

        /// <summary>
        /// Initiated with Delete key
        /// </summary>
        public class DeleteRightSymbolCmd : ICommand {
            public bool Do(TextEditorController controller) {
                Point fromTextPos = cursorPosToTextPos(controller.CursorPos, controller.Window);
                Point toTextPos;
                string line = controller.textHolder.Lines[fromTextPos.Y];
                if (fromTextPos.X == line.Length) {
                    if (fromTextPos.Y == controller.textHolder.LinesCount - 1) {
                        return false;
                    }
                    toTextPos = new Point(0, fromTextPos.Y + 1);
                } else {
                    toTextPos = new Point(fromTextPos.X + 1, fromTextPos.Y);
                }

                controller.textHolder.Delete(fromTextPos.Y, fromTextPos.X, toTextPos.Y, toTextPos.X);
                moveWindowToCursor(textPosToCursorPos(fromTextPos, controller.Window), controller);

                return true;
            }
        }

        public class MoveCursorCmd : ICommand {
            private readonly Direction direction;

            public MoveCursorCmd(Direction direction) {
                this.direction = direction;
            }

            public bool Do(TextEditorController controller) {
                var oldCursorPos = controller.CursorPos;
                var oldWindow = controller.Window;
                switch (direction) {
                    case Direction.Up: {
                        Point oldTextPos = cursorPosToTextPos(oldCursorPos, oldWindow);
                        Point textPos;
                        if (oldTextPos.Y == 0) {
                            if (oldTextPos.X == 0) {
                                break;
                            }

                            textPos = new Point(0, 0);
                        } else {
                            string prevLine = controller.textHolder.Lines[oldTextPos.Y - 1];
                            textPos = new Point(
                                Math.Min(controller.lastTextPosX, prevLine.Length),
                                oldTextPos.Y - 1
                            );
                        }

                        moveWindowToCursor(textPosToCursorPos(textPos, oldWindow), controller, true);
                        break;
                    }
                    case Direction.Down: {
                        Point oldTextPos = cursorPosToTextPos(oldCursorPos, oldWindow);
                        Point textPos;
                        if (oldTextPos.Y == controller.textHolder.LinesCount - 1) {
                            string lastLine = controller.textHolder.Lines[controller.textHolder.LinesCount - 1];
                            if (oldTextPos.X == lastLine.Length) {
                                break;
                            }

                            textPos = new Point(lastLine.Length, controller.textHolder.LinesCount - 1);
                        } else {
                            string nextLine = controller.textHolder.Lines[oldTextPos.Y + 1];
                            textPos = new Point(
                                Math.Min(controller.lastTextPosX, nextLine.Length),
                                oldTextPos.Y + 1
                            );
                        }

                        moveWindowToCursor(textPosToCursorPos(textPos, oldWindow), controller, true);
                        break;
                    }
                    case Direction.Left: {
                        Point oldTextPos = cursorPosToTextPos(oldCursorPos, oldWindow);
                        Point textPos;
                        if (oldTextPos.X == 0) {
                            if (oldTextPos.Y == 0) {
                                break;
                            }

                            string prevLine = controller.textHolder.Lines[oldTextPos.Y - 1];
                            textPos = new Point(prevLine.Length, oldTextPos.Y - 1);
                        } else {
                            textPos = new Point(oldTextPos.X - 1, oldTextPos.Y);
                        }

                        moveWindowToCursor(textPosToCursorPos(textPos, oldWindow), controller);
                        break;
                    }
                    case Direction.Right: {
                        Point oldTextPos = cursorPosToTextPos(oldCursorPos, oldWindow);
                        Point textPos;
                        if (oldTextPos.Y == controller.textHolder.LinesCount - 1) {
                            string lastLine = controller.textHolder.Lines[controller.textHolder.LinesCount - 1];
                            if (oldTextPos.X == lastLine.Length) {
                                break;
                            }

                            textPos = new Point(oldTextPos.X + 1, oldTextPos.Y);
                        } else {
                            string line = controller.textHolder.Lines[oldTextPos.Y];
                            if (oldTextPos.X < line.Length) {
                                textPos = new Point(oldTextPos.X + 1, oldTextPos.Y);
                            } else {
                                textPos = new Point(0, oldTextPos.Y + 1);
                            }
                        }

                        moveWindowToCursor(textPosToCursorPos(textPos, oldWindow), controller);
                        break;
                    }
                }

                return controller.Window != oldWindow;
            }
        }
    }

    public class TextHolder {
        // TODO : change to more appropriate data structure
        private List<string> lines;
        private readonly string newLine = Environment.NewLine;

        public TextHolder(string text, string newLine) {
            this.newLine = newLine;
            setText(text);
        }

        public TextHolder(string text) {
            setText(text);
        }

        private void setText(string text) {
            lines = new List<string>(text.Split(new[] { newLine }, StringSplitOptions.None));
        }

        public string Text {
            get => string.Join(newLine, lines);
            set => setText(value);
        }

        public IList<string> Lines => lines.AsReadOnly();

        public int LinesCount => lines.Count;
        public int ColumnsCount => lines.Max(it => it.Length);

        /// <summary>
        /// Inserts string after specified position with respect to newline symbols.
        /// Returns the coords (col+ln) of next symbol after inserted.
        /// TODO : write unit test to check return value
        /// </summary>
        public Point Insert(int ln, int col, string s) {
            // There are at least one empty line even if no text at all
            if (ln >= lines.Count) {
                throw new ArgumentException("ln is out of range", nameof(ln));
            }

            string currentLine = lines[ln];
            if (col > currentLine.Length) {
                throw new ArgumentException("col is out of range", nameof(col));
            }

            string leftPart = currentLine.Substring(0, col);
            string rightPart = currentLine.Substring(col);

            string[] linesToInsert = s.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

            if (linesToInsert.Length == 1) {
                lines[ln] = leftPart + linesToInsert[0] + rightPart;
                return new Point(leftPart.Length + linesToInsert[0].Length, ln);
            } else {
                lines[ln] = leftPart + linesToInsert[0];
                lines.InsertRange(ln + 1, linesToInsert.Skip(1).Take(linesToInsert.Length - 1));
                string lastStrLeftPart = lines[ln + linesToInsert.Length - 1];
                lines[ln + linesToInsert.Length - 1] = lastStrLeftPart + rightPart;
                return new Point(lastStrLeftPart.Length, ln + linesToInsert.Length - 1);
            }
        }

        /// <summary>
        /// Will write the content of text editor to matrix constrained with width/height,
        /// starting from (left, top) coord. Coords may be negative.
        /// If there are any gap before (or after) text due to margin, window will be filled
        /// with spaces there.
        /// Window size should be equal to width/height passed.
        /// </summary>
        public void WriteToWindow(int left, int top, int width, int height, char[,] window) {
            if (window.GetLength(0) != height) {
                throw new ArgumentException("window height differs from viewport height");
            }

            if (window.GetLength(1) != width) {
                throw new ArgumentException("window width differs from viewport width");
            }

            for (int y = top; y < 0; y++) {
                for (int x = 0; x < width; x++) {
                    window[y - top, x] = ' ';
                }
            }

            for (int y = Math.Max(0, top); y < Math.Min(top + height, lines.Count); y++) {
                string line = lines[y];
                for (int x = left; x < 0; x++) {
                    window[y - top, x - left] = ' ';
                }

                for (int x = Math.Max(0, left); x < Math.Min(left + width, line.Length); x++) {
                    window[y - top, x - left] = line[x];
                }

                for (int x = Math.Max(line.Length, left); x < left + width; x++) {
                    window[y - top, x - left] = ' ';
                }
            }

            for (int y = lines.Count; y < top + height; y++) {
                for (int x = 0; x < width; x++) {
                    window[y - top, x] = ' ';
                }
            }
        }

        /// <summary>
        /// Deletes text from lnFrom+colFrom to lnTo+colTo (exclusive).
        /// </summary>
        public void Delete(int lnFrom, int colFrom, int lnTo, int colTo) {
            if (lnFrom > lnTo) {
                throw new ArgumentException("lnFrom should be <= lnTo");
            }
            if (lnFrom == lnTo && colFrom >= colTo) {
                throw new ArgumentException("colFrom should be < colTo on the same line");
            }
            //
            lines[lnFrom] = lines[lnFrom].Substring(0, colFrom) + lines[lnTo].Substring(colTo);
            lines.RemoveRange(lnFrom + 1, lnTo - lnFrom);
        }
    }


    /// <summary>
    /// Multiline text editor.
    /// </summary>
    [ContentProperty("Text")]
    public class TextEditor : Control {
        private TextEditorController controller;
        private char[,] buffer;
        private ScrollBar horizontalScrollbar;
        private ScrollBar verticalScrollbar;

        public string Text {
            get => controller.Text;
            set {
                if (value != controller.Text) {
                    controller.Text = value;
                    CursorPosition = controller.CursorPos;
                    Invalidate();
                }
            }
        }

        // TODO : Scrollbars always visible

        private void applyCommand(TextEditorController.ICommand cmd) {
            var oldCursorPos = controller.CursorPos;
            if (cmd.Do(controller)) {
                Invalidate();
            }

            if (oldCursorPos != controller.CursorPos) {
                CursorPosition = controller.CursorPos;
            }
        }

        public TextEditor() {
            controller = new TextEditorController("", 0, 0);
            KeyDown += OnKeyDown;
            MouseDown += OnMouseDown;
            CursorVisible = true;
            CursorPosition = new Point(0, 0);
            Focusable = true;

            horizontalScrollbar = new ScrollBar {
                Orientation = Orientation.Horizontal,
                Visibility = Visibility.Hidden
            };
            verticalScrollbar = new ScrollBar {
                Orientation = Orientation.Vertical,
                Visibility = Visibility.Hidden
            };
            AddChild(horizontalScrollbar);
            AddChild(verticalScrollbar);
        }

        protected override Size MeasureOverride(Size availableSize) {
            verticalScrollbar.Measure(new Size(1, availableSize.Height));
            horizontalScrollbar.Measure(new Size(availableSize.Width, 1));
            return new Size(0, 0);
        }

        protected override Size ArrangeOverride(Size finalSize) {
            if (controller.LinesCount > finalSize.Height) {
                verticalScrollbar.Visibility = Visibility.Visible;
                verticalScrollbar.MaxValue =
                    controller.LinesCount + TextEditorController.LINES_BOTTOM_MAX_GAP - controller.Window.Height;
                verticalScrollbar.Value = controller.Window.Top;
                verticalScrollbar.Invalidate();
            } else {
                verticalScrollbar.Visibility = Visibility.Collapsed;
                verticalScrollbar.Value = 0;
                verticalScrollbar.MaxValue = 10;
            }
            if (controller.ColumnsCount >= finalSize.Width) {
                horizontalScrollbar.Visibility = Visibility.Visible;
                horizontalScrollbar.MaxValue =
                    controller.ColumnsCount + TextEditorController.COLUMNS_RIGHT_MAX_GAP - controller.Window.Width;
                horizontalScrollbar.Value = controller.Window.Left;
                horizontalScrollbar.Invalidate();
            } else {
                horizontalScrollbar.Visibility = Visibility.Collapsed;
                horizontalScrollbar.Value = 0;
                horizontalScrollbar.MaxValue = 10;
            }
            horizontalScrollbar.Arrange(new Rect(
                0,
                Math.Max(0, finalSize.Height - 1),
                Math.Max(0, finalSize.Width -
                            (verticalScrollbar.Visibility == Visibility.Visible
                             || horizontalScrollbar.Visibility != Visibility.Visible
                                ? 1
                                : 0)),
                1
            ));
            verticalScrollbar.Arrange(new Rect(
                Math.Max(0, finalSize.Width - 1),
                0,
                1,
                Math.Max(0, finalSize.Height -
                            (horizontalScrollbar.Visibility == Visibility.Visible
                             || verticalScrollbar.Visibility != Visibility.Visible
                                ? 1
                                : 0))
            ));
            Size contentSize = new Size(
                Math.Max(0, finalSize.Width - (verticalScrollbar.Visibility == Visibility.Visible ? 1 : 0)),
                Math.Max(0, finalSize.Height - (horizontalScrollbar.Visibility == Visibility.Visible ? 1 : 0))
            );
            controller.Window = new Rect(controller.Window.TopLeft, contentSize);
            buffer = new char[contentSize.Height, contentSize.Width];
            return finalSize;
        }

        public override void Render(RenderingBuffer buffer) {
            var attrs = Colors.Blend(Color.Green, Color.DarkBlue);
            buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, ' ', attrs);

            controller.WriteToWindow(this.buffer);
            Size contentSize = controller.Window.Size;
            for (int y = 0; y < contentSize.Height; y++) {
                for (int x = 0; x < contentSize.Width; x++) {
                    buffer.SetPixel(x, y, this.buffer[y, x]);
                }
            }

            if (verticalScrollbar.Visibility == Visibility.Visible
                && horizontalScrollbar.Visibility == Visibility.Visible) {
                buffer.SetPixel(buffer.Width - 1, buffer.Height - 1,
                    UnicodeTable.SingleFrameBottomRightCorner,
                    Colors.Blend(Color.DarkCyan, Color.DarkBlue));
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
            Point position = mouseButtonEventArgs.GetPosition(this);
            Point constrained = new Point(
                Math.Max(0, Math.Min(controller.Window.Size.Width - 1, position.X)),
                Math.Max(0, Math.Min(controller.Window.Size.Height - 1, position.Y))
            );
            applyCommand(new TextEditorController.TrySetCursorCmd(constrained));
            mouseButtonEventArgs.Handled = true;
        }

        private void OnKeyDown(object sender, KeyEventArgs args) {
            ConsoleKeyInfo keyInfo = new ConsoleKeyInfo(args.UnicodeChar,
                (ConsoleKey) args.wVirtualKeyCode,
                (args.dwControlKeyState & ControlKeyState.SHIFT_PRESSED) == ControlKeyState.SHIFT_PRESSED,
                (args.dwControlKeyState & ControlKeyState.LEFT_ALT_PRESSED) == ControlKeyState.LEFT_ALT_PRESSED
                || (args.dwControlKeyState & ControlKeyState.RIGHT_ALT_PRESSED) == ControlKeyState.RIGHT_ALT_PRESSED,
                (args.dwControlKeyState & ControlKeyState.LEFT_CTRL_PRESSED) == ControlKeyState.LEFT_CTRL_PRESSED
                || (args.dwControlKeyState & ControlKeyState.RIGHT_CTRL_PRESSED) == ControlKeyState.RIGHT_CTRL_PRESSED
            );
            if (!char.IsControl(keyInfo.KeyChar)) {
                applyCommand(new TextEditorController.AppendStringCmd(new string(keyInfo.KeyChar, 1)));
            }

            switch (keyInfo.Key) {
                case ConsoleKey.Enter:
                    applyCommand(new TextEditorController.AppendStringCmd(Environment.NewLine));
                    break;
                case ConsoleKey.UpArrow:
                    applyCommand(new TextEditorController.MoveCursorCmd(TextEditorController.Direction.Up));
                    break;
                case ConsoleKey.DownArrow:
                    applyCommand(new TextEditorController.MoveCursorCmd(TextEditorController.Direction.Down));
                    break;
                case ConsoleKey.LeftArrow:
                    applyCommand(new TextEditorController.MoveCursorCmd(TextEditorController.Direction.Left));
                    break;
                case ConsoleKey.RightArrow:
                    applyCommand(new TextEditorController.MoveCursorCmd(TextEditorController.Direction.Right));
                    break;
                case ConsoleKey.Backspace:
                    applyCommand(new TextEditorController.DeleteLeftSymbolCmd());
                    break;
                case ConsoleKey.Delete:
                    applyCommand(new TextEditorController.DeleteRightSymbolCmd());
                    break;
                case ConsoleKey.PageDown:
                    applyCommand(new TextEditorController.PageDownCmd());
                    break;
                case ConsoleKey.PageUp:
                    applyCommand(new TextEditorController.PageUpCmd());
                    break;
                case ConsoleKey.Home:
                    applyCommand(new TextEditorController.HomeCommand());
                    break;
                case ConsoleKey.End:
                    applyCommand(new TextEditorController.EndCommand());
                    break;
            }
        }
    }
}
