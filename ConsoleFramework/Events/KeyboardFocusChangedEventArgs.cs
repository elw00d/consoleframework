using ConsoleFramework.Controls;

namespace ConsoleFramework.Events {

    public delegate void KeyboardFocusChangedEventHandler(object sender, KeyboardFocusChangedEventArgs args);

    public class KeyboardFocusChangedEventArgs : RoutedEventArgs {
        public KeyboardFocusChangedEventArgs(object source, RoutedEvent routedEvent) : base(source, routedEvent) {
        }

        public KeyboardFocusChangedEventArgs(object source, RoutedEvent routedEvent,
                                             Control oldFocus, Control newFocus) : base(source, routedEvent) {
            //
            OldFocus = oldFocus;
            NewFocus = newFocus;
        }

        public Control OldFocus {
            get;
            private set;
        }

        public Control NewFocus {
            get;
            private set;
        }
    }
}