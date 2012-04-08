using System.Collections.Generic;
using System.Diagnostics;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    public class TextBlock : Control {
        private string text;

        private void initialize() {
            AddHandler(KeyDownEvent, new KeyEventHandler(TextBlock_KeyDown));
        }

        public void TextBlock_KeyDown(object sender, KeyEventArgs args) {
            Text = Text + "5";
            args.Handled = true;
        }

        public TextBlock(Control parent) : base(parent) {
            initialize();
        }

        public TextBlock() {
            initialize();
        }

        public string Text {
            get {
                return text;
            }
            set {
                if (text != value) {
                    text = value;
                    this.Invalidate();
                }
            }
        }

        protected override Size MeasureOverride(Size availableSize) {
            if (null != text)
                return new Size(text.Length, 1);
            return new Size(0, 0);
        }

        public override void Render(RenderingBuffer buffer) {
            for (int x = 0; x < ActualWidth; ++x) {
                for (int y = 0; y < ActualHeight; ++y) {
                    if (y == 0 && x < text.Length) {
                        buffer.SetPixel(x, y, text[x], CHAR_ATTRIBUTES.FOREGROUND_BLUE | CHAR_ATTRIBUTES.FOREGROUND_GREEN | CHAR_ATTRIBUTES.FOREGROUND_RED);
                    }
                }
            }
        }

        public override string ToString() {
            return "TextBlock";
        }
    }
}
