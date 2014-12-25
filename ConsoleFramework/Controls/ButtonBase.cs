using System;
using ConsoleFramework.Events;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Base class for buttons and toggle buttons (checkboxes and radio buttons).
    /// </summary>
    public abstract class ButtonBase : Control, ICommandSource
    {
        /// <summary>
        /// Is button in clicking mode (when mouse pressed but not released yet).
        /// </summary>
        private bool clicking;

        /// <summary>
        /// Is button pressed using mouse now.
        /// </summary>
        protected bool pressed;

        /// <summary>
        /// True in some time after user has pressed button using keyboard
        /// (~ 0.5 second) - just for animate pressing
        /// </summary>
        protected bool pressedUsingKeyboard;

        private bool disabled;
        public bool Disabled {
            get {
                return disabled;
            }
            set {
                if (disabled != value) {
                    disabled = value;
                    Focusable = !disabled;
                    Invalidate();
                }
            }
        }
        
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", 
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ButtonBase));

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
            Focusable = true;
        }

        private void Button_KeyDown( object sender, KeyEventArgs args ) {
            if (Disabled) return;

            if ( args.wVirtualKeyCode == VirtualKeys.Space
                || args.wVirtualKeyCode == VirtualKeys.Return) {
                RaiseEvent(ClickEvent, new RoutedEventArgs(this, ClickEvent));
                pressedUsingKeyboard = true;
                Invalidate(  );
                ConsoleApplication.Instance.Post( ( ) => {
                    pressedUsingKeyboard = false;
                    Invalidate(  );
                }, TimeSpan.FromMilliseconds( 300 ) );
                args.Handled = true;
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
            if (!clicking && !Disabled) {
                clicking = true;
                pressed = true;
                ConsoleApplication.Instance.BeginCaptureInput(this);
                this.Invalidate();
                args.Handled = true;
            }
        }

        private void Button_OnMouseUp(object sender, MouseButtonEventArgs args) {
            if (clicking && !Disabled) {
                clicking = false;
                if (pressed) {
                    pressed = false;
                    this.Invalidate();
                }
                if (HitTest(args.RawPosition)) {
                    RaiseEvent(ClickEvent, new RoutedEventArgs(this, ClickEvent));
                    if (command != null && command.CanExecute(CommandParameter)) {
                        command.Execute(CommandParameter);
                    }
                }
                ConsoleApplication.Instance.EndCaptureInput(this);
                args.Handled = true;
            }
        }

        private ICommand command;
        public ICommand Command {
            get {
                return command;
            }
            set {
                if (command != value) {
                    if (command != null) {
                        command.CanExecuteChanged -= onCommandCanExecuteChanged;
                    }
                    command = value;
                    command.CanExecuteChanged += onCommandCanExecuteChanged;

                    refreshCanExecute();
                }
            }
        }

        private void onCommandCanExecuteChanged(object sender, EventArgs args) {
            refreshCanExecute();
        }

        private void refreshCanExecute() {
            if (command == null) {
                this.Disabled = false;
                return;
            }

            this.Disabled = !command.CanExecute(CommandParameter);
        }

        public object CommandParameter {
            get;
            set;
        }
    }
}
