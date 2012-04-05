using System;

namespace ConsoleFramework.Events {

    public delegate void RoutedEventHandler(object sender, RoutedEventArgs e);

    public class RoutedEventArgs : EventArgs {
        private bool handled;
        private readonly object source;
        private readonly RoutedEvent routedEvent;

        public bool Handled {
            get {
                return handled;
            }
            set {
                handled = value;
            }
        }

        public object Source {
            get {
                return source;
            }
        }

        public RoutedEvent RoutedEvent {
            get {
                return routedEvent;
            }
        }

        public RoutedEventArgs (object source, RoutedEvent routedEvent) {
            this.source = source;
            this.routedEvent = routedEvent;
        }
    }
}