using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls {
    public class TextBox : Control {
        public TextBox() {
            KeyDown += TextBox_KeyDown;
            GotKeyboardFocus += OnGotKeyboardFocus;
            LostKeyboardFocus += OnLostKeyboardFocus;
            CursorVisible = true;
        }

        private void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs args) {
            //HideCursor();
        }

        private void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs args) {
            //            Point point = new Point(cursorPosition, 0);
            //            if (this.IsPointVisible(point)) {
            //                SetCursorPosition(point);
            //                ShowCursor();
            //            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs args) {
            // todo : add right alt & ctrl support
            ConsoleKeyInfo keyInfo = new ConsoleKeyInfo(args.UnicodeChar, (ConsoleKey) args.wVirtualKeyCode,
                                                        (args.dwControlKeyState & ControlKeyState.SHIFT_PRESSED) ==
                                                        ControlKeyState.SHIFT_PRESSED,
                                                        (args.dwControlKeyState & ControlKeyState.LEFT_ALT_PRESSED) ==
                                                        ControlKeyState.LEFT_ALT_PRESSED,
                                                        (args.dwControlKeyState & ControlKeyState.LEFT_CTRL_PRESSED) ==
                                                        ControlKeyState.LEFT_CTRL_PRESSED);
            if (!char.IsControl(keyInfo.KeyChar)) {
                // insert keychar into a text string according to cursorPosition and offset
                if (text != null) {
                    string leftPart = text.Substring(0, cursorPosition + displayOffset);
                    string rightPart = text.Substring(cursorPosition + displayOffset);
                    Text = leftPart + keyInfo.KeyChar + rightPart;
                } else {
                    Text = keyInfo.KeyChar.ToString();
                }
            } else {
                if (keyInfo.Key == ConsoleKey.Delete) {
                }
                if (keyInfo.Key == ConsoleKey.Backspace) {
                }
                if (keyInfo.Key == ConsoleKey.LeftArrow) {
                }
                if (keyInfo.Key == ConsoleKey.RightArrow) {
                }
                if (keyInfo.Key == ConsoleKey.Home) {
                }
                if (keyInfo.Key == ConsoleKey.End) {
                }
            }
        }

        private string text;

        public string Text {
            get {
                return text;
            }
            set {
                if (text != value) {
                    text = value;
                    Invalidate();
                }
            }
        }

        public int MaxLenght {
            get;
            set;
        }

        public int Size {
            get;
            set;
        }

        protected override Size MeasureOverride(Size availableSize) {
            return new Size(Size, 1);
        }

        protected override Size ArrangeOverride(Size finalSize) {
            return base.ArrangeOverride(finalSize);
        }

        // this fields describe the whole state of textbox
        private int displayOffset;
        private int cursorPosition;
        // -1 if no selection started
        private int startSelection;

        public override void Render(RenderingBuffer buffer) {
            ushort attr = Color.Attr(Color.White, Color.DarkBlue);
            buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, ' ', attr);
            if (null != text) {
                for (int i = displayOffset; i < text.Length; i++) {
                    if (i - displayOffset < ActualWidth) {
                        buffer.SetPixel(i - displayOffset, 0, text[i]);
                    }
                }
            }
        }
    }
}