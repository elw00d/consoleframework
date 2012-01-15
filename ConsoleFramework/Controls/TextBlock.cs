using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    public class TextBlock : Control {
        private string text;

        public TextBlock(PhysicalCanvas canvas) : base(canvas) {
        }

        public TextBlock(Control parent) : base(parent) {
        }

        public string Text {
            get {
                return text;
            }
            set {
                if (text != value) {
                    text = value;
                    // todo : invalidate graphical representation
                }
            }
        }

        public TextBlock() {
        }

        public override void Draw() {
            //
            for (int x = 0; x < ActualWidth; ++x) {
                for (int y = 0; y < ActualHeight; ++y) {
                    if (y == 0 && x < text.Length) {
                        canvas.SetPixel(x, y, text[x], CHAR_ATTRIBUTES.FOREGROUND_BLUE);
                    }
                    //
                }
            }
        }
    }
}
