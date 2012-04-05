using System;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;

namespace ConsoleFramework.Events {

    public delegate void MouseEventHandler(object sender, MouseEventArgs e);

    public delegate void MouseButtonEventHandler(object sender, MouseButtonEventArgs e);

    public delegate void MouseWheelEventHandler(object sender, MouseWheelEventArgs e);

    public enum MouseButtonState
    {
        Released,
        Pressed
    }

    public class MouseWheelEventArgs : MouseEventArgs
    {
        // Properties
        public MouseWheelEventArgs(object source, RoutedEvent routedEvent)
            : base(source, routedEvent) {
        }

        public int Delta {
            get;
            private set;
        }
    }

    public enum MouseButton
    {
        Left,
        Middle,
        Right
    }

    public class MouseButtonEventArgs : MouseEventArgs
    {
        private readonly MouseButton button;

        public MouseButtonEventArgs(object source, RoutedEvent routedEvent)
            : base(source, routedEvent) {
        }

        public MouseButtonEventArgs(object source, RoutedEvent routedEvent, Point rawPosition,
                                    MouseButtonState leftButton, MouseButtonState middleButton,
                                    MouseButtonState rightButton,
                                    MouseButton button)
            : base(source, routedEvent, rawPosition, leftButton, middleButton, rightButton) {
            this.button = button;
        }

        public MouseButtonState ButtonState {
            get {
                switch (button) {
                    case MouseButton.Left:
                        return LeftButton;
                    case MouseButton.Middle:
                        return MiddleButton;
                    case MouseButton.Right:
                        return RightButton;
                }
                throw new InvalidOperationException("This code should not be reached.");
            }
        }

        public MouseButton ChangedButton {
            get {
                return button;
            }
        }
    }


    public class MouseEventArgs : RoutedEventArgs
    {
        public MouseEventArgs(object source, RoutedEvent routedEvent) : base(source, routedEvent) {
        }

        public MouseEventArgs(object source, RoutedEvent routedEvent, Point rawPosition,
                              MouseButtonState leftButton, MouseButtonState middleButton, MouseButtonState rightButton)
            : base(source, routedEvent) {
            //
            this.rawPosition = rawPosition;
            this.LeftButton = leftButton;
            this.MiddleButton = middleButton;
            this.RightButton = rightButton;
        }

        private readonly Point rawPosition;
        public Point RawPosition {
            get {
                return rawPosition;
            }
        }

        public Point GetPosition(Control relativeTo) {
            return Control.TranslatePoint(null, rawPosition, relativeTo);
        }

        public MouseButtonState LeftButton {
            get;
            private set;
        }

        public MouseButtonState MiddleButton {
            get;
            private set;
        }

        public MouseButtonState RightButton {
            get;
            private set;
        }
    }
}