using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Simple control that always fill the all space with specified color.
    /// </summary>
    public class BackgroundControl : Control
    {
        public CHAR_ATTRIBUTES FillAttributes {
            get;
            set;
        }

        public char FillCharacter {
            get;
            set;
        }

        public BackgroundControl() {
            FillAttributes = CHAR_ATTRIBUTES.BACKGROUND_BLUE | CHAR_ATTRIBUTES.BACKGROUND_GREEN;
            FillCharacter = ' ';
        }

        public BackgroundControl(Control underlyingControl) : base(underlyingControl) {
            //
        }

        public BackgroundControl(PhysicalCanvas pcanvas) : base(pcanvas) {
            
        }

        public override void Draw(int actualLeft, int actualTop, int actualWidth, int actualHeight) {
            for (int x = 0; x < actualWidth; ++x) {
                for (int y = 0; y < actualHeight; ++y) {
                    canvas.SetPixel(x + actualLeft, y + actualTop, FillCharacter, FillAttributes);
                }
            }
        }

        public override void HandleEvent(INPUT_RECORD inputRecord) {
            // do nothing
        }
    }
}
