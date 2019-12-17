using System;

namespace ConsoleFramework.Events
{
    public delegate void CancelEventHandler(object sender, CancelEventArgs e);

    public class CancelEventArgs : RoutedEventArgs
    {
        private bool cancel;

        public CancelEventArgs(object source, RoutedEvent routedEvent) : base(source, routedEvent)
        {

        }

        public bool Cancel {
            get {
                return cancel;
            }
            set {
                cancel = value;
            }
        }
    }
}
