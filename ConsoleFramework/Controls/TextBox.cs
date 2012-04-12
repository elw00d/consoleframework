using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleFramework.Events;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    public class TextBox : Control
    {
        public TextBox() {
            this.AddHandler(KeyDownEvent, new KeyEventHandler(TextBox_KeyDown));
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs args) {
            //
        }

        public string Text {
            get;
            set;
        }

        public int MaxLenght {
            get;
            set;
        }

        private int startPosition;
        private int cursorPosition;
        // todo : selection

        public override void Render(RenderingBuffer buffer) {
            ushort attr = Color.Attr(Color.White, Color.DarkBlue);
            buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, ' ', attr);
        }
    }
}
