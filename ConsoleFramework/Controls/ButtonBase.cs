using ConsoleFramework.Events;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Base class for buttons and toggle buttons (checkboxes and radio buttons).
    /// </summary>
    public abstract class ButtonBase : Control
    {
        /// <summary>
        /// Is button in clicking mode (when mouse pressed but not released yet).
        /// </summary>
        private bool clicking;

        /// <summary>
        /// Is button pressed using mouse now.
        /// </summary>
        protected bool pressed;

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

        protected ButtonBase() {
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
            if ( args.wVirtualKeyCode == 0x20 ) { // VK_SPACE
                RaiseEvent(ClickEvent, new RoutedEventArgs(this, ClickEvent));
            }
        }

        private void Button_MouseEnter(object sender, MouseEventArgs args) {
            if (clicking) {
                if (!pressed) {
                    pressed = true;
                    Invalidate();
                }
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs args) {
            if (clicking) {
                if (pressed) {
                    pressed = false;
                    Invalidate();
                }
            }
        }

        private void Button_OnMouseDown(object sender, MouseButtonEventArgs args) {
            if (!clicking) {
                clicking = true;
                pressed = true;
                ConsoleApplication.Instance.BeginCaptureInput(this);
                this.Invalidate();
                args.Handled = true;
            }
        }

        private void Button_OnMouseUp(object sender, MouseButtonEventArgs args) {
            if (clicking) {
                clicking = false;
                if (pressed) {
                    pressed = false;
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
