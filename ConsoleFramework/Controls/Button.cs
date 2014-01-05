using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;

namespace ConsoleFramework.Controls
{
    public class Button : Control {

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", 
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Button));

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
            AddHandler( KeyDownEvent, new KeyEventHandler(Button_KeyDown) );
            AddHandler(GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotKeyboardFocus));
            AddHandler(LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnLostKeyboardFocus));
            Focusable = true;
        }

        private void OnLostKeyboardFocus( object sender, KeyboardFocusChangedEventArgs args ) {
            Invalidate(  );
        }

        private void OnGotKeyboardFocus( object sender, KeyboardFocusChangedEventArgs args ) {
            Invalidate(  );
        }

        private void Button_KeyDown( object sender, KeyEventArgs args ) {
            if ( args.wVirtualKeyCode == 13 ) { // VK_RETURN
                RaiseEvent(ClickEvent, new RoutedEventArgs(this, ClickEvent));
            }
        }

        private string caption;
        public string Caption {
            get {
                return caption;
            }
            set {
                caption = value;
            }
        }

        private bool clicking;
        private bool showPressed;

        protected override Size MeasureOverride(Size availableSize) {
            if (!string.IsNullOrEmpty(caption)) {
                Size minButtonSize = new Size(caption.Length + 14, 2);
                return minButtonSize;
            } else return new Size(8, 2);
        }
        
        public override void Render(RenderingBuffer buffer) {
            Attr captionAttrs;
            if (HasFocus)
                captionAttrs = Colors.Blend(Color.White, Color.DarkGreen);
            else
                captionAttrs = Colors.Blend(Color.Black, Color.DarkGreen);
            
            if (showPressed) {
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
                buffer.FillRectangle(1, ActualHeight-1, ActualWidth - 1, 1, '\u2580', Attr.NO_ATTRIBUTES);
                buffer.SetOpacityRect(ActualWidth - 1, 0, 1, ActualHeight, 3);
                buffer.FillRectangle(ActualWidth - 1, 1, 1, ActualHeight - 2, '\u2588', Attr.NO_ATTRIBUTES);
                buffer.SetPixel(ActualWidth - 1, 0, '\u2584');
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
                if (showPressed) {
                    showPressed = false;
                    this.Invalidate();
                }
                if (HitTest(args.RawPosition)) {
                    RaiseEvent(ClickEvent, new RoutedEventArgs(this, ClickEvent));
                }
                ConsoleApplication.Instance.EndCaptureInput(this);
                args.Handled = true;
            }
        }
    }
}
