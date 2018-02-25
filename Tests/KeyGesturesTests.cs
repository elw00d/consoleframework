using ConsoleFramework.Events;
using ConsoleFramework.Native;
using Xunit;

namespace Tests
{
    public class KeyGesturesTests
    {
        [Fact]
        public void TestConverter() {
            KeyGestureConverter converter = new KeyGestureConverter();
            
            KeyGesture gesture = (KeyGesture) converter.ConvertFrom("CTRL+COMMA");
            Assert.Equal(VirtualKeys.OEMComma, gesture.Key);
            Assert.Equal(ModifierKeys.Control, gesture.Modifiers);
            Assert.Equal(string.Empty, gesture.DisplayString);

            gesture = (KeyGesture) converter.ConvertFrom("ENTER");
            Assert.Equal(VirtualKeys.Return, gesture.Key);
            Assert.Equal(ModifierKeys.None, gesture.Modifiers);

            gesture = (KeyGesture) converter.ConvertFrom("ALT+CONTROL+PGUP");
            Assert.Equal(VirtualKeys.PageUp, gesture.Key);
            Assert.Equal(ModifierKeys.Alt | ModifierKeys.Control, gesture.Modifiers);

            gesture = (KeyGesture) converter.ConvertFrom("SHIFT+F");
            Assert.Equal(VirtualKeys.F, gesture.Key);
            Assert.Equal(ModifierKeys.Shift, gesture.Modifiers);
        }

        [Fact]
        public void TestMatch() {
            KeyGestureConverter converter = new KeyGestureConverter();
            
            KeyGesture gesture = (KeyGesture)converter.ConvertFrom("CTRL+COMMA");
//            Assert.IsFalse(gesture.Matches(new KEY_EVENT_RECORD() {
//                    wVirtualKeyCode = VirtualKeys.OEMComma,
//                    dwControlKeyState = ControlKeyState.LEFT_ALT_PRESSED | ControlKeyState.RIGHT_CTRL_PRESSED
//                }));
//            Assert.IsTrue(gesture.Matches(new KEY_EVENT_RECORD() {
//                wVirtualKeyCode = VirtualKeys.OEMComma,
//                dwControlKeyState = ControlKeyState.RIGHT_CTRL_PRESSED
//            }));
//            Assert.IsFalse(gesture.Matches(new KEY_EVENT_RECORD() {
//                wVirtualKeyCode = VirtualKeys.Return,
//                dwControlKeyState = ControlKeyState.RIGHT_CTRL_PRESSED
//            }));
//            Assert.IsTrue(gesture.Matches(new KEY_EVENT_RECORD() {
//                wVirtualKeyCode = VirtualKeys.OEMComma,
//                dwControlKeyState = ControlKeyState.LEFT_CTRL_PRESSED | ControlKeyState.RIGHT_CTRL_PRESSED
//            }));
//
//            gesture = (KeyGesture)converter.ConvertFrom("CTRL+ALT+D");
//            Assert.IsTrue(gesture.Matches(new KEY_EVENT_RECORD() {
//                wVirtualKeyCode = VirtualKeys.D,
//                dwControlKeyState = ControlKeyState.LEFT_ALT_PRESSED | ControlKeyState.RIGHT_CTRL_PRESSED
//            }));
//            Assert.IsFalse(gesture.Matches(new KEY_EVENT_RECORD() {
//                wVirtualKeyCode = VirtualKeys.D,
//                dwControlKeyState = ControlKeyState.SHIFT_PRESSED | ControlKeyState.LEFT_ALT_PRESSED | ControlKeyState.RIGHT_CTRL_PRESSED
//            }));
//            Assert.IsTrue(gesture.Matches(new KEY_EVENT_RECORD() {
//                wVirtualKeyCode = VirtualKeys.D,
//                dwControlKeyState = ControlKeyState.CAPSLOCK_ON | 
//                    ControlKeyState.NUMLOCK_ON | ControlKeyState.SCROLLLOCK_ON |
//                    ControlKeyState.LEFT_ALT_PRESSED | ControlKeyState.RIGHT_CTRL_PRESSED
//            }));
        }
    }
}
