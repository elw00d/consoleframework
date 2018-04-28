using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Rendering;
using Xaml;

namespace ConsoleFramework.Controls {
    public class TextHolder
    {
        // TODO : change to more appropriate data structure
        private List<string> lines;

        /// <summary>
        /// Logical cursor position (points to symbol in textItems, not to display coord)
        /// </summary>
        private Point cursorPos;

        public TextHolder(string text) {
            lines = new List<string>(text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
            cursorPos = new Point(0, 0);
        }

        public string Text => string.Join(Environment.NewLine, lines);

        public void Insert(int ln, int col, string s) {
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

            string[] linesToInsert = s.Split(new string[]{ Environment.NewLine}, StringSplitOptions.None);

            if (linesToInsert.Length == 1) {
                lines[ln] = leftPart + linesToInsert[0] + rightPart;
            } else {
                lines[ln] = leftPart + linesToInsert[0];
                lines.InsertRange(ln + 1, linesToInsert.Skip(1).Take(linesToInsert.Length - 1));
                lines[ln + linesToInsert.Length - 1] = lines[ln + linesToInsert.Length - 1] + rightPart;
            }
        }

        /// <summary>
        /// Will write the content of text editor to matrix constrained with width/height,
        /// starting from (left, top) coord. Coords may be negative.
        /// If there are any gap before (or after) text due to margin, window will be filled
        /// with spaces there.
        /// Window size should be equal to width/height passed.
        /// </summary>
        public void WriteToWindow(int left, int top, int width, int height, char[,] window)
        {
            if (window.GetLength(0) != height)
            {
                throw new ArgumentException("window height differs from viewport height");
            }
            if (window.GetLength(1) != width)
            {
                throw new ArgumentException("window width differs from viewport width");
            }

            for (int y = top; y < 0; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    window[y - top, x] = ' ';
                }
            }
            for (int y = Math.Max(0, top); y < Math.Min(top + height, lines.Count); y++)
            {
                string line = lines[y];
                for (int x = left; x < 0; x++)
                {
                    window[y - top, x - left] = ' ';
                }
                for (int x = Math.Max(0, left); x < Math.Min(left + width, line.Length); x++)
                {
                    window[y - top, x - left] = line[x];
                }
                for (int x = line.Length; x < left + width; x++)
                {
                    window[y - top, x - left] = ' ';
                }
            }
            for (int y = lines.Count; y < top + height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    window[y - top, x] = ' ';
                }
            }
        }

        public void Delete(int ln, int col, int count)
        {
            //
        }
    }

    /// <summary>
    /// Multiline text editor.
    /// </summary>
    [ContentProperty("Text")]
    public class TextEditor : Control {
        private LinkedList<string> textItems;
        public string Text {
            get {
                return string.Join("\n", textItems);
            }
            set {
                textItems = new LinkedList<string>(value.Split('\n'));
                Invalidate();
            }
        }

        /// <summary>
        /// Logical cursor position (points to symbol in textItems, not to display coord)
        /// </summary>
        private Point cursorPos;

        public TextEditor()
        {
            KeyDown += OnKeyDown;
            MouseDown += OnMouseDown;
            CursorVisible = true;
            CursorPosition = new Point(0, 0);
            Focusable = true;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(textItems.Max(s => s.Length), textItems.Count);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return finalSize;
        }

        public override void Render(RenderingBuffer buffer)
        {
            var attrs = Colors.Blend(Color.Yellow, Color.DarkGreen);
            buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, ' ', attrs);
            int i = 0;
            foreach (var textItem in textItems)
            {
                RenderString(textItem, buffer, 0, i, ActualWidth, attrs);
                i++;
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            //
        }

        private void OnKeyDown(object sender, KeyEventArgs args)
        {
            //
        }
    }
}
