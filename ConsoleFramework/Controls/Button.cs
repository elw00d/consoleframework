using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            return new Size(10, 2);
        }
        
        public override void Render(RenderingBuffer buffer) {
            if (pressed) {
                buffer.SetPixel(0, 0, ' ', CHAR_ATTRIBUTES.BACKGROUND_GREEN);
                buffer.SetPixel(1, 0, ' ', CHAR_ATTRIBUTES.BACKGROUND_GREEN);
                buffer.SetPixel(2, 0, ' ', CHAR_ATTRIBUTES.BACKGROUND_GREEN);
                buffer.SetPixel(3, 0, ' ', CHAR_ATTRIBUTES.BACKGROUND_GREEN);
                buffer.SetPixel(4, 0, ' ', CHAR_ATTRIBUTES.BACKGROUND_GREEN);
                buffer.SetPixel(5, 0, ' ', CHAR_ATTRIBUTES.BACKGROUND_GREEN);
                buffer.SetPixel(6, 0, ' ', CHAR_ATTRIBUTES.BACKGROUND_GREEN);
                buffer.SetPixel(7, 0, ' ', CHAR_ATTRIBUTES.BACKGROUND_GREEN);
            } else {
                buffer.SetPixel(1, 0, ' ', CHAR_ATTRIBUTES.BACKGROUND_GREEN);
                buffer.SetPixel(2, 0, ' ', CHAR_ATTRIBUTES.BACKGROUND_GREEN);
                buffer.SetPixel(3, 0, ' ', CHAR_ATTRIBUTES.BACKGROUND_GREEN);
                buffer.SetPixel(4, 0, ' ', CHAR_ATTRIBUTES.BACKGROUND_GREEN);
                buffer.SetPixel(5, 0, ' ', CHAR_ATTRIBUTES.BACKGROUND_GREEN);
                buffer.SetPixel(6, 0, ' ', CHAR_ATTRIBUTES.BACKGROUND_GREEN);
                buffer.SetPixel(7, 0, ' ', CHAR_ATTRIBUTES.BACKGROUND_GREEN);
                buffer.SetPixel(8, 0, ' ', CHAR_ATTRIBUTES.BACKGROUND_GREEN);
            }
        }

        public override void HandleEvent(INPUT_RECORD inputRecord) {
            if (inputRecord.EventType == EventType.MOUSE_EVENT &&
                (inputRecord.MouseEvent.dwButtonState & MouseButtonState.FROM_LEFT_1ST_BUTTON_PRESSED) ==
                MouseButtonState.FROM_LEFT_1ST_BUTTON_PRESSED) {
                if (!pressed) {
                    pressed = true;
                    ConsoleApplication.Instance.BeginCaptureInput(this);
                }
            }
            if (inputRecord.EventType == EventType.MOUSE_EVENT &&
                (inputRecord.MouseEvent.dwButtonState & MouseButtonState.FROM_LEFT_1ST_BUTTON_PRESSED) !=
                MouseButtonState.FROM_LEFT_1ST_BUTTON_PRESSED) {
                if (pressed) {
                    pressed = false;
                    ConsoleApplication.Instance.EndCaptureInput(this);
                }
            }
        }
    }
}
