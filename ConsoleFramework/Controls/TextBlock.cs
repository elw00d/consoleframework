using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    public class TextBlock : Control {
        private string text;
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

        public override void Draw(int actualLeft, int actualTop, int actualWidth, int actualHeight) {
            //
            for (int x = 0; x < actualWidth; ++x) {
                for (int y = 0; y < actualHeight; ++y) {
                    if (y == 0 && x < text.Length) {
                        canvas.SetPixel(x, y, text[x], CHAR_ATTRIBUTES.FOREGROUND_BLUE);
                    }
                    //
                }
            }
        }
    }
}
