using System;
using System.Collections.Generic;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;

namespace ConsoleFramework.Controls
{
    public class PasswordBox : TextBox
    {

        private string mask;
        private string text;

        public new string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (text != value)
                {
                    text = value;
                    Invalidate();

                    RaisePropertyChanged("mask");
                }
            }
        }

        public char PassChar
        {
            get
            {
                return PassChar;
            }
            set
            {
                PassChar = value;
            }
        }


        public PasswordBox()
        {
            KeyDown += MaskBox_KeyDown;
            MouseDown += OnMouseDown;
            CursorVisible = true;
            CursorPosition = new Point(1, 0);
            Focusable = true;
        }
        private void OnMouseDown(object sender, MouseButtonEventArgs args)
        {
            var point = args.GetPosition(this);
            if (point.X > 0 && point.X - 1 < getSize())
            {
                var x = point.X - 1;
                if (!String.IsNullOrEmpty(mask))
                {
                    if (x <= mask.Length)
                        cursorPosition = x;
                    else
                    {
                        cursorPosition = mask.Length;
                    }
                    CursorPosition = new Point(cursorPosition + 1, 0);
                }
                args.Handled = true;
            }
        }

        private void MaskBox_KeyDown(object sender, KeyEventArgs args)
        {
            // TODO : add right alt & ctrl support
            var keyInfo = new ConsoleKeyInfo(args.UnicodeChar, (ConsoleKey)args.wVirtualKeyCode,
                                                        (args.dwControlKeyState & ControlKeyState.SHIFT_PRESSED) ==
                                                        ControlKeyState.SHIFT_PRESSED,
                                                        (args.dwControlKeyState & ControlKeyState.LEFT_ALT_PRESSED) ==
                                                        ControlKeyState.LEFT_ALT_PRESSED,
                                                        (args.dwControlKeyState & ControlKeyState.LEFT_CTRL_PRESSED) ==
                                                        ControlKeyState.LEFT_CTRL_PRESSED);
            if (!char.IsControl(keyInfo.KeyChar))
            {
                // insert keychar into a mask string according to cursorPosition and offset
                if (mask != null)
                {
                    string leftPart = mask.Substring(0, cursorPosition + displayOffset);
                    string rightPart = mask.Substring(cursorPosition + displayOffset);
                    mask = leftPart + PassChar + rightPart;
                    string aLeftPart = mask.Substring(0, cursorPosition + displayOffset);
                    string aRightPart = mask.Substring(cursorPosition + displayOffset);
                    Text = aLeftPart + keyInfo.KeyChar.ToString() + aRightPart;
                }
                else
                {
                    mask = PassChar.ToString();
                    Text = keyInfo.KeyChar.ToString();
                }
                if (cursorPosition + 1 < ActualWidth - 2)
                {
                    cursorPosition++;
                    CursorPosition = new Point(cursorPosition + 1, 0);
                }
                else
                {
                    displayOffset++;
                }
            }
            else
            {
                if (keyInfo.Key == ConsoleKey.Delete)
                {
                    if (!String.IsNullOrEmpty(mask) && displayOffset + cursorPosition < mask.Length)
                    {
                        var leftPart = mask.Substring(0, cursorPosition + displayOffset);
                        var rightPart = mask.Substring(cursorPosition + displayOffset + 1);
                        mask = leftPart + rightPart;
                        var aLeftPart = mask.Substring(0, cursorPosition + displayOffset);
                        var aRightPart = mask.Substring(cursorPosition + displayOffset + 1);
                        Text = aLeftPart + aRightPart;
                    }
                    else
                    {
                        Console.Beep();
                    }
                }
                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (!String.IsNullOrEmpty(mask) && (displayOffset != 0 || cursorPosition != 0))
                    {

                        var leftPart = mask.Substring(0, cursorPosition + displayOffset - 1);
                        var rightPart = mask.Substring(cursorPosition + displayOffset);
                        mask = leftPart + rightPart;
                        var aLeftPart = mask.Substring(0, cursorPosition + displayOffset - 1);
                        var aRightPart = mask.Substring(cursorPosition + displayOffset);
                        Text = aLeftPart + aRightPart;
                        if (displayOffset > 0)
                            displayOffset--;
                        else
                        {
                            if (cursorPosition > 0)
                            {
                                cursorPosition--;
                                CursorPosition = new Point(cursorPosition + 1, 0);
                            }
                        }
                    }
                    else
                    {
                        Console.Beep();
                    }
                }
                if (keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    if (!String.IsNullOrEmpty(mask) && (displayOffset != 0 || cursorPosition != 0))
                    {
                        if (cursorPosition > 0)
                        {
                            cursorPosition--;
                            CursorPosition = new Point(cursorPosition + 1, 0);
                        }
                        else
                        {
                            if (displayOffset > 0)
                            {
                                displayOffset--;
                                Invalidate();
                            }
                        }
                    }
                    else
                    {
                        Console.Beep();
                    }
                }
                if (keyInfo.Key == ConsoleKey.RightArrow)
                {
                    if (!String.IsNullOrEmpty(mask) && displayOffset + cursorPosition < mask.Length)
                    {
                        if (cursorPosition + 1 < ActualWidth - 2)
                        {
                            cursorPosition++;
                            CursorPosition = new Point(cursorPosition + 1, 0);
                        }
                        else
                        {
                            if (displayOffset + cursorPosition < mask.Length)
                            {
                                displayOffset++;
                                Invalidate();
                            }
                        }
                    }
                    else
                    {
                        Console.Beep();
                    }
                }
                if (keyInfo.Key == ConsoleKey.Home)
                {
                    if (displayOffset != 0 || cursorPosition != 0)
                    {
                        displayOffset = 0;
                        cursorPosition = 0;
                        CursorPosition = new Point(cursorPosition + 1, 0);
                        Invalidate();
                    }
                    else
                    {
                        Console.Beep();
                    }
                }
                if (keyInfo.Key == ConsoleKey.End)
                {
                    if (!String.IsNullOrEmpty(mask) && cursorPosition + displayOffset < ActualWidth - 2)
                    {
                        displayOffset = mask.Length >= ActualWidth - 2 ? mask.Length - (ActualWidth - 2) + 1 : 0;
                        cursorPosition = mask.Length >= ActualWidth - 2 ? ActualWidth - 2 - 1 : mask.Length;
                        CursorPosition = new Point(cursorPosition + 1, 0);
                        Invalidate();
                    }
                    else
                    {
                        Console.Beep();
                    }
                }
            }
            //Debugger.Log(0, "", String.Format("cursorPos : {0} offset {1}\n", cursorPosition, displayOffset));
        }

        

        private int GetSize()
        {
            if (Size.HasValue) return Size.Value;
            return mask != null ? mask.Length + 1 : 1;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var desired = new Size(GetSize() + 2, 1);
            return new Size(
                Math.Min(desired.Width, availableSize.Width),
                Math.Min(desired.Height, availableSize.Height)
            );
        }

        // this fields describe the whole state of maskbox
        private int displayOffset;
        private int cursorPosition;
        // -1 if no selection started

        public override void Render(RenderingBuffer buffer)
        {
            var attr = Colors.Blend(Color.White, Color.DarkBlue);
            buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, ' ', attr);
            if (null != mask)
            {
                for (int i = displayOffset; i < mask.Length; i++)
                {
                    if (i - displayOffset < ActualWidth - 2 && i - displayOffset >= 0)
                    {
                        buffer.SetPixel(1 + i - displayOffset, 0, mask[i]);
                    }
                }
            }
            var arrowsAttr = Colors.Blend(Color.Green, Color.DarkBlue);
            if (displayOffset > 0)
                buffer.SetPixel(0, 0, '<', arrowsAttr);
            if (!String.IsNullOrEmpty(mask) && ActualWidth - 2 + displayOffset < mask.Length)
                buffer.SetPixel(ActualWidth - 1, 0, '>', arrowsAttr);
        }
    }
}
