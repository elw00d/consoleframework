using System;
using ConsoleFramework.Core;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    public class Button : Control {
        private string caption;
        public string Caption {
            get {
                return caption;
            }
            set {
                if (caption != value) {
                    caption = value;
                }
            }
        }

        private bool pressed;

        protected override Size MeasureOverride(Size availableSize) {
            Size minButtonSize = new Size(caption.Length + 3, 2);
            return minButtonSize;
            //return new Size(Math.Max(minButtonSize.width, availableSize.width - 1), Math.Max(minButtonSize.height, availableSize.height - 1));
        }
        
        public override void Render(RenderingBuffer buffer) {
            // todo : print caption at center
            if (pressed) {
                buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, ' ', CHAR_ATTRIBUTES.BACKGROUND_GREEN | CHAR_ATTRIBUTES.BACKGROUND_INTENSITY);
            } else {
                buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, 'b', CHAR_ATTRIBUTES.BACKGROUND_GREEN);
            }
        }

        public override void HandleEvent(INPUT_RECORD inputRecord) {
            if (inputRecord.EventType == EventType.MOUSE_EVENT &&
                (inputRecord.MouseEvent.dwButtonState & MouseButtonState.FROM_LEFT_1ST_BUTTON_PRESSED) ==
                MouseButtonState.FROM_LEFT_1ST_BUTTON_PRESSED) {
                if (!pressed) {
                    pressed = true;
                    ConsoleApplication.Instance.BeginCaptureInput(this);
                    this.Invalidate();
                    //this.Parent.Invalidate();
                    //this.Parent.Parent.Invalidate();
                }
            }
            if (inputRecord.EventType == EventType.MOUSE_EVENT &&
                (inputRecord.MouseEvent.dwButtonState & MouseButtonState.FROM_LEFT_1ST_BUTTON_PRESSED) !=
                MouseButtonState.FROM_LEFT_1ST_BUTTON_PRESSED) {
                if (pressed) {
                    pressed = false;
                    ConsoleApplication.Instance.EndCaptureInput(this);
                    this.Invalidate();
                    //this.Parent.Invalidate();
                    //this.Parent.Parent.Invalidate();
                }
            }
        }
    }
}
