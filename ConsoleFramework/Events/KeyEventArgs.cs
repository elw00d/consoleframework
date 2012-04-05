using ConsoleFramework.Native;

namespace ConsoleFramework.Events {

    public delegate void KeyEventHandler(object sender, KeyEventArgs args);

    public class KeyEventArgs : RoutedEventArgs {
        public KeyEventArgs(object source, RoutedEvent routedEvent) : base(source, routedEvent) {
        }

        public bool bKeyDown;
        public ushort wRepeatCount;
        public short wVirtualKeyCode;
        public ushort wVirtualScanCode;
        public char UnicodeChar;
        public ControlKeyState dwControlKeyState;
    }
}