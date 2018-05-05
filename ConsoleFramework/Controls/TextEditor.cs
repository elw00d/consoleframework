using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;
using Xaml;

// TODO : move cursorPos + window to another class > ?
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

        public string Text {
            get => textHolder.Text;
            set {
                if (textHolder.Text != value) {
                    textHolder.Text = value;
                    CursorPos = new Point();
                    Window = new Rect(new Point(), Window.Size);
                }
            }
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

            //void Undo();
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
        /// TODO :
        /// </summary>
        static void moveWindowToCursor(Point cursor,
            TextEditorController controller, bool light = false) {
            Rect oldWindow = controller.Window;

            int? windowX = null;
            int? windowY = null;

            if (cursor.X >= oldWindow.Width) {
                // Move window 3px right if nextChar is outside the window after add char
                windowX = oldWindow.X + cursor.X - oldWindow.Width + 3;
            } else if (cursor.X < 0) {
                // Move window left if need (with 4px gap from left)
                windowX = Math.Max(0, oldWindow.X + cursor.X - 4);
            }

            // Move window down if nextChar is outside the window
            if (cursor.Y >= controller.Window.Height) {
                windowY = controller.Window.Top + cursor.Y - controller.Window.Height + 1;
            } else if (cursor.Y < 0) {
                windowY = controller.Window.Y + cursor.Y;
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
                    // TODO : remaining directions
                }

                return controller.Window != oldWindow;
            }
        }
    }

    public class TextHolder {
        // TODO : change to more appropriate data structure
        private List<string> lines;

        public TextHolder(string text) {
            setText(text);
        }

        private void setText(string text) {
            lines = new List<string>(text.Split(new[] {Environment.NewLine}, StringSplitOptions.None));
        }

        public string Text {
            get => string.Join(Environment.NewLine, lines);
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

            string[] linesToInsert = s.Split(new string[] {Environment.NewLine}, StringSplitOptions.None);

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

        public void Delete(int ln, int col, int count) {
            //
        }
    }


    /// <summary>
    /// Multiline text editor.
    /// </summary>
    [ContentProperty("Text")]
    public class TextEditor : Control {
        private TextEditorController controller;
        private char[,] buffer;

        public string Text {
            get => controller.Text;
            set {
                if (value != controller.Text) {
                    controller.Text = value;
                    invalidate(true);
                }
            }
        }

        private void invalidate(bool changeCursorPos = false) {
            CursorPosition = controller.CursorPos;
            Invalidate();
        }

        private void applyCommand(TextEditorController.ICommand cmd) {
            var oldCursorPos = controller.CursorPos;
            if (cmd.Do(controller)) {
                invalidate();
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
        }

        protected override Size MeasureOverride(Size availableSize) {
            return new Size(0, 0);
        }

        protected override Size ArrangeOverride(Size finalSize) {
            controller.Window = new Rect(controller.Window.TopLeft, finalSize);
            buffer = new char[finalSize.Height, finalSize.Width];
            return finalSize;
        }

        public override void Render(RenderingBuffer buffer) {
            var attrs = Colors.Blend(Color.Green, Color.DarkBlue);
            buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, ' ', attrs);

            controller.WriteToWindow(this.buffer);
            for (int y = 0; y < ActualHeight; y++) {
                for (int x = 0; x < ActualWidth; x++) {
                    buffer.SetPixel(x, y, this.buffer[y, x]);
                }
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

            if (keyInfo.Key == ConsoleKey.Enter) {
                applyCommand(new TextEditorController.AppendStringCmd(Environment.NewLine));
            }

            if (keyInfo.Key == ConsoleKey.UpArrow) {
                applyCommand(new TextEditorController.MoveCursorCmd(TextEditorController.Direction.Up));
            }

            if (keyInfo.Key == ConsoleKey.DownArrow) {
                applyCommand(new TextEditorController.MoveCursorCmd(TextEditorController.Direction.Down));
            }

            if (keyInfo.Key == ConsoleKey.LeftArrow) {
                applyCommand(new TextEditorController.MoveCursorCmd(TextEditorController.Direction.Left));
            }

            if (keyInfo.Key == ConsoleKey.RightArrow) {
                applyCommand(new TextEditorController.MoveCursorCmd(TextEditorController.Direction.Right));
            }
        }
    }
}
