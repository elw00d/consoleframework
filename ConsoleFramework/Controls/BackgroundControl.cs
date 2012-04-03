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

        public override void Render(RenderingBuffer buffer) {
            for (int x = 0; x < ActualWidth; ++x) {
                for (int y = 0; y < ActualHeight; ++y) {
                    buffer.SetPixel(x + ActualOffset.X, y + ActualOffset.Y, FillCharacter, FillAttributes);
                }
            }
        }

        public override bool HandleEvent(INPUT_RECORD inputRecord) {
            // do nothing
            return true;
        }
    }
}
