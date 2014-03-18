using System;
using ConsoleFramework.Native;
using Xaml;

namespace ConsoleFramework.Events
{
    public class KeyConverter : ITypeConverter
    {
        public bool CanConvertFrom(Type sourceType) {
            return (sourceType == typeof(string));
        }

        public bool CanConvertTo(Type destinationType) {
            return (destinationType == typeof (string));
        }

        public object ConvertFrom(object source) {
            if (!(source is string)) throw new NotSupportedException();
            string keyToken = ((string)source).Trim();
            object key = this.parseKey(keyToken);
            if (key == null) {
                throw new NotSupportedException("Unsupported key " + keyToken );
            }
            return (VirtualKeys)key;
        }

        public object ConvertTo(object value, Type destinationType) {
//            if (destinationType == null) {
//                throw new ArgumentNullException("destinationType");
//            }
//            if ((destinationType == typeof(string)) && (value != null)) {
//                VirtualKeys key = (VirtualKeys)value;
//                if ((key >= VirtualKeys.N0) && (key <= VirtualKeys.N9)) {
//                    return char.ToString((char)((ushort)((key - '0') + (ushort) VirtualKeys.N0)));
//                }
//                if ((key >= VirtualKeys.A) && (key <= VirtualKeys.Z)) {
//                    return char.ToString((char)((ushort)((key - 0x2c) + 0x41)));
//                }
//                string str = key.ToString( );
//                if ((str != null) && ((str.Length != 0) || (str == string.Empty))) {
//                    return str;
//                }
//            }
//            throw base.GetConvertToException(value, destinationType);
            // todo :
            throw new NotSupportedException("todo :");
        }

        private VirtualKeys parseKey(string keyToken) {
            if (keyToken == string.Empty) {
                throw new ArgumentException("keyToken is empty");
            }
            keyToken = keyToken.ToUpper();
            if ((keyToken.Length == 1) && char.IsLetterOrDigit(keyToken[0])) {
                if ((char.IsDigit(keyToken[0]) && (keyToken[0] >= '0')) && (keyToken[0] <= '9')) {
                    return ( VirtualKeys ) ( ((int) VirtualKeys.N0) + (keyToken[0] - '0') );
                }
                if ((!char.IsLetter(keyToken[0]) || (keyToken[0] < 'A')) || (keyToken[0] > 'Z')) {
                    throw new ArgumentException("Cannot convert string to VirtualKeys "+ keyToken);
                }
                return ( VirtualKeys ) ( ((int) VirtualKeys.A) + (keyToken[0] - 0x41) );
            }
            VirtualKeys escape = 0;
            switch (keyToken) {
                case "ENTER":
                    escape = VirtualKeys.Return;
                    break;
                case "ESC":
                    escape = VirtualKeys.Escape;
                    break;
                case "PGUP":
                    escape = VirtualKeys.Prior;
                    break;
                case "PGDN":
                    escape = VirtualKeys.Next;
                    break;
                case "PRTSC":
                    escape = VirtualKeys.Snapshot;
                    break;
                case "INS":
                    escape = VirtualKeys.Insert;
                    break;
                case "DEL":
                    escape = VirtualKeys.Delete;
                    break;
                case "WINDOWS":
                case "WIN":
                case "LEFTWINDOWS":
                    escape = VirtualKeys.LeftWindows;
                    break;
                case "RIGHTWINDOWS":
                    escape = VirtualKeys.RightWindows;
                    break;
                case "APPS":
                    escape = VirtualKeys.Application;
                    break;
                case "BREAK":
                    escape = VirtualKeys.Cancel;
                    break;
                case "BACKSPACE":
                case "BKSP":
                case "BS":
                    escape = VirtualKeys.Back;
                    break;
                case "SHIFT":
                case "LEFTSHIFT":
                    escape = VirtualKeys.LeftShift;
                    break;
                case "RIGHTSHIFT":
                    escape = VirtualKeys.RightShift;
                    break;
                case "CONTROL":
                case "CTRL":
                case "LEFTCTRL":
                    escape = VirtualKeys.LeftControl;
                    break;
                case "RIGHTCTRL":
                    escape = VirtualKeys.RightControl;
                    break;
                case "SEMICOLON":
                    escape = VirtualKeys.OEM1;
                    break;
                case "PLUS":
                    escape = VirtualKeys.OEMPlus;
                    break;
                case "COMMA":
                    escape = VirtualKeys.OEMComma;
                    break;
                case "MINUS":
                    escape = VirtualKeys.OEMMinus;
                    break;
                case "PERIOD":
                    escape = VirtualKeys.OEMPeriod;
                    break;
                case "QUESTION":
                    escape = VirtualKeys.OEM2;
                    break;
                case "TILDE":
                    escape = VirtualKeys.OEM3;
                    break;
                case "OPENBRACKETS":
                    escape = VirtualKeys.OEM4;
                    break;
                case "PIPE":
                    escape = VirtualKeys.OEM5;
                    break;
                case "CLOSEBRACKETS":
                    escape = VirtualKeys.OEM6;
                    break;
                case "QUOTES":
                    escape = VirtualKeys.OEM7;
                    break;
                case "BACKSLASH":
                    escape = VirtualKeys.OEM102;
                    break;
                case "FINISH":
                    escape = VirtualKeys.OEMFinish;
                    break;
                case "ATTN":
                    escape = VirtualKeys.ATTN;
                    break;
                case "CRSEL":
                    escape = VirtualKeys.CRSel;
                    break;
                case "EXSEL":
                    escape = VirtualKeys.EXSel;
                    break;
                case "ERASEEOF":
                    escape = VirtualKeys.EREOF;
                    break;
                case "PLAY":
                    escape = VirtualKeys.Play;
                    break;
                case "ZOOM":
                    escape = VirtualKeys.Zoom;
                    break;
                case "PA1":
                    escape = VirtualKeys.PA1;
                    break;
                default:
                    escape = (VirtualKeys)Enum.Parse(typeof(VirtualKeys), keyToken, true);
                    break;
            }
            if (escape != 0) {
                return escape;
            }
            throw new InvalidOperationException("Cannot convert " + keyToken + " to VirtualKeys");
        }
    }
}
