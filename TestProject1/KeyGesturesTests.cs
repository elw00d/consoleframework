using ConsoleFramework.Events;
using ConsoleFramework.Native;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1
{
    [TestClass]
    public class KeyGesturesTests
    {
        [TestMethod]
        public void TestConverter() {
            KeyGestureConverter converter = new KeyGestureConverter();
            
            KeyGesture gesture = (KeyGesture) converter.ConvertFrom("CTRL+COMMA");
            Assert.AreEqual(VirtualKeys.OEMComma, gesture.Key);
            Assert.AreEqual(ModifierKeys.Control, gesture.Modifiers);
            Assert.AreEqual(string.Empty, gesture.DisplayString);

            gesture = (KeyGesture) converter.ConvertFrom("ENTER");
            Assert.AreEqual(VirtualKeys.Return, gesture.Key);
            Assert.AreEqual(ModifierKeys.None, gesture.Modifiers);

            gesture = (KeyGesture) converter.ConvertFrom("ALT+CONTROL+PGUP");
            Assert.AreEqual(VirtualKeys.PageUp, gesture.Key);
            Assert.AreEqual(ModifierKeys.Alt | ModifierKeys.Control, gesture.Modifiers);

            gesture = (KeyGesture) converter.ConvertFrom("SHIFT+F");
            Assert.AreEqual(VirtualKeys.F, gesture.Key);
            Assert.AreEqual(ModifierKeys.Shift, gesture.Modifiers);
        }

        [TestMethod]
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
