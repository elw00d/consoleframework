using System;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    public class Button : Control {

        public Button() {
            EventManager.AddHandler(this, Control.MouseDownEvent, new MouseButtonEventHandler(Button_OnMouseDown));
            EventManager.AddHandler(this, Control.MouseUpEvent, new MouseButtonEventHandler(Button_OnMouseUp));
        }

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

        public void Button_OnMouseDown(object sender, MouseButtonEventArgs args) {
            if (!pressed) {
                pressed = true;
                ConsoleApplication.Instance.BeginCaptureInput(this);
                this.Invalidate();
                args.Handled = true;
            }
        }

        public void Button_OnMouseUp(object sender, MouseButtonEventArgs args) {
            if (pressed) {
                pressed = false;
                ConsoleApplication.Instance.EndCaptureInput(this);
                this.Invalidate();
                args.Handled = true;
            }
        }
    }
}
