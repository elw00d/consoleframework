using ConsoleFramework.Core;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;
using Xaml;

namespace ConsoleFramework.Controls
{
    [ContentProperty("Caption")]
    public class Button : ButtonBase {
        private string caption;
        public string Caption {
            get {
                return caption;
            }
            set {
                caption = value;
                Invalidate(  );
            }
        }

        protected override Size MeasureOverride(Size availableSize) {
            if (!string.IsNullOrEmpty(caption)) {
                Size minButtonSize = new Size(caption.Length + 10, 2);
                return minButtonSize;
            } else return new Size(8, 2);
        }
        
        public override void Render(RenderingBuffer buffer) {
            Attr captionAttrs;
            if (Disabled) {
                captionAttrs = Colors.Blend(Color.Gray, Color.DarkGray);
            } else {
                if (HasFocus)
                    captionAttrs = Colors.Blend(Color.White, Color.DarkGreen);
                else
                    captionAttrs = Colors.Blend(Color.Black, Color.DarkGreen);
            }

            if (pressed || pressedUsingKeyboard) {
                buffer.FillRectangle(1, 0, ActualWidth - 1, ActualHeight - 1, ' ', captionAttrs);
                buffer.SetOpacityRect(0, 0, 1, ActualHeight, 3);
                buffer.FillRectangle(0, 0, 1, ActualHeight, ' ', captionAttrs);
                if (!string.IsNullOrEmpty(Caption)) {
                    RenderString(Caption, buffer, 2 + 1 +(ActualWidth - 2 * 2 - Caption.Length) / 2,
                        (ActualHeight-1)/2, ActualWidth - 2 * 2, captionAttrs);
                }
                buffer.SetOpacityRect(0, ActualHeight-1, ActualWidth, 1, 3);
                buffer.FillRectangle(0, ActualHeight - 1, ActualWidth, 1, ' ', Attr.NO_ATTRIBUTES);
            } else {
                buffer.FillRectangle(0, 0, ActualWidth - 1, ActualHeight, ' ', captionAttrs);
                if (!string.IsNullOrEmpty(Caption)) {
                    RenderString(Caption, buffer, 2 + (ActualWidth - 2 * 2 - Caption.Length) / 2, 
                        (ActualHeight - 1) / 2, ActualWidth - 2 * 2, captionAttrs);
                }
                buffer.SetPixel(0, ActualHeight-1, ' ');
                buffer.SetOpacityRect(0, ActualHeight -1, ActualWidth, 1, 3);
                buffer.FillRectangle(1, ActualHeight-1, ActualWidth - 1, 1, UnicodeTable.UpperHalfBlock, Attr.NO_ATTRIBUTES);
                buffer.SetOpacityRect(ActualWidth - 1, 0, 1, ActualHeight, 3);
                buffer.FillRectangle(ActualWidth - 1, 1, 1, ActualHeight - 2, UnicodeTable.FullBlock, Attr.NO_ATTRIBUTES);
                buffer.SetPixel(ActualWidth - 1, 0, UnicodeTable.LowerHalfBlock);
            }
        }
    }
}
