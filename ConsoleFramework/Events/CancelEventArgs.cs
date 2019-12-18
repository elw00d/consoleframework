namespace ConsoleFramework.Events
{
    public delegate void CancelEventHandler(object sender, CancelEventArgs e);

    public class CancelEventArgs : RoutedEventArgs
    {
        private bool _cancel;

        public CancelEventArgs(object source, RoutedEvent routedEvent) : base(source, routedEvent)
        {

        }

        public bool Cancel {
            get {
                return _cancel;
            }
            set {
                _cancel = value;
            }
        }
    }
}
