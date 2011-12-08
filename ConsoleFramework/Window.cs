using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleFramework.Native;

namespace ConsoleFramework
{
    public class Window : FrameworkElement
    {
        public Window() {
            ConsoleApplication.Instance.Dispatcher.RegisterWindow(this);
        }

        protected override void Draw(VirtualCanvas virtualCanvas) {
            virtualCanvas[0][0].AsciiChar = '+';
            virtualCanvas[0][0].Attributes = CHAR_ATTRIBUTES.FOREGROUND_GREEN;
            virtualCanvas[Width - 1][0].AsciiChar = '+';
            virtualCanvas[Width - 1][0].Attributes = CHAR_ATTRIBUTES.FOREGROUND_GREEN;
            virtualCanvas[0][Height - 1].AsciiChar = '+';
            virtualCanvas[0][Height - 1].Attributes = CHAR_ATTRIBUTES.FOREGROUND_GREEN;
            virtualCanvas[Width - 1][Height - 1].AsciiChar = '+';
            virtualCanvas[Width - 1][Height - 1].Attributes = CHAR_ATTRIBUTES.FOREGROUND_GREEN;
        }
    }
}
