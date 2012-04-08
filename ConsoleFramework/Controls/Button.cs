using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    public class Button : Control {

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Button));

        public event RoutedEventHandler OnClick {
            add {
                AddHandler(ClickEvent, value);
            }
            remove {
                RemoveHandler(ClickEvent, value);
            }
        }

        public Button() {
            AddHandler(MouseDownEvent, new MouseButtonEventHandler(Button_OnMouseDown));
            AddHandler(MouseUpEvent, new MouseButtonEventHandler(Button_OnMouseUp));
            AddHandler(MouseEnterEvent, new MouseEventHandler(Button_MouseEnter));
            AddHandler(MouseLeaveEvent, new MouseEventHandler(Button_MouseLeave));
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

        private bool clicking;
        private bool showPressed;

        protected override Size MeasureOverride(Size availableSize) {
            Size minButtonSize = new Size(caption.Length + 3, 2);
            return minButtonSize;
            //return new Size(Math.Max(minButtonSize.width, availableSize.width - 1), Math.Max(minButtonSize.height, availableSize.height - 1));
        }
        
        public override void Render(RenderingBuffer buffer) {
            // todo : print caption at center
            if (showPressed) {
                buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, ' ', CHAR_ATTRIBUTES.BACKGROUND_GREEN | CHAR_ATTRIBUTES.BACKGROUND_INTENSITY);
            } else {
                buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, 'b', CHAR_ATTRIBUTES.BACKGROUND_GREEN);
            }
        }

        private void Button_MouseEnter(object sender, MouseEventArgs args) {
            if (clicking) {
                if (!showPressed) {
                    showPressed = true;
                    Invalidate();
                }
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs args) {
            if (clicking) {
                if (showPressed) {
                    showPressed = false;
                    Invalidate();
                }
            }
        }

        public void Button_OnMouseDown(object sender, MouseButtonEventArgs args) {
            if (!clicking) {
                clicking = true;
                showPressed = true;
                ConsoleApplication.Instance.BeginCaptureInput(this);
                this.Invalidate();
                args.Handled = true;
            }
        }

        public void Button_OnMouseUp(object sender, MouseButtonEventArgs args) {
            if (clicking) {
                clicking = false;
                showPressed = false;
                Point point = args.GetPosition(Parent);
                if (RenderSlotRect.Contains(point)) {
                    RaiseEvent(ClickEvent, new RoutedEventArgs(this, ClickEvent));
                }
                ConsoleApplication.Instance.EndCaptureInput(this);
                this.Invalidate();
                args.Handled = true;
            }
        }
    }
}
