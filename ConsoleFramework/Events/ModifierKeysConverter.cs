using System;
using Xaml;

namespace ConsoleFramework.Events
{
    [Flags, TypeConverter( typeof ( ModifierKeysConverter ) )]
    public enum ModifierKeys
    {
        Alt = 1,
        Control = 2,
        None = 0,
        Shift = 4
    }

    public class ModifierKeysConverter : ITypeConverter
    {
        private const char Modifier_Delimiter = '+';

        public bool CanConvertFrom( Type sourceType ) {
            return ( sourceType == typeof ( string ) );
        }

        public bool CanConvertTo( Type destinationType ) {
            return destinationType == typeof ( string );
        }

        public object ConvertFrom( object source ) {
            if ( !( source is string ) ) throw new NotSupportedException( "Cannot convert from this object." );
            string modifiersToken = ( ( string ) source ).Trim( );
            return this.GetModifierKeys( modifiersToken );
        }

        public object ConvertTo( object value, Type destinationType ) {
            if ( destinationType == null ) throw new ArgumentNullException( "destinationType" );
            if ( destinationType != typeof ( string ) ) throw new NotSupportedException( "value should be string" );

            ModifierKeys modifierKeys = ( ModifierKeys ) value;
            string str = "";
            if ( ( modifierKeys & ModifierKeys.Control ) == ModifierKeys.Control ) {
                str = str + MatchModifiers( ModifierKeys.Control );
            }
            if ( ( modifierKeys & ModifierKeys.Alt ) == ModifierKeys.Alt ) {
                if ( str.Length > 0 ) {
                    str = str + Modifier_Delimiter;
                }
                str = str + MatchModifiers( ModifierKeys.Alt );
            }
            if ( ( modifierKeys & ModifierKeys.Shift ) != ModifierKeys.Shift ) {
                return str;
            }
            if ( str.Length > 0 ) {
                str = str + Modifier_Delimiter;
            }
            return ( str + MatchModifiers( ModifierKeys.Shift ) );
        }

        private ModifierKeys GetModifierKeys( string modifiersToken ) {
            ModifierKeys none = ModifierKeys.None;
            if ( modifiersToken.Length != 0 ) {
                int length = 0;
                do {
                    length = modifiersToken.IndexOf( Modifier_Delimiter );
                    string str = ( length < 0 ) ? modifiersToken : modifiersToken.Substring( 0, length );
                    str = str.Trim( ).ToUpper( );
                    switch ( str ) {
                        case "CONTROL":
                        case "CTRL":
                            none |= ModifierKeys.Control;
                            break;
                        case "SHIFT":
                            none |= ModifierKeys.Shift;
                            break;
                        case "ALT":
                            none |= ModifierKeys.Alt;
                            break;
                        case "":
                            return none;
                        default:
                            throw new NotSupportedException( "Unsupported modifier " + str );
                    }
                    modifiersToken = modifiersToken.Substring( length + 1 );
                } while ( length != -1 );
            }
            return none;
        }

        internal static string MatchModifiers( ModifierKeys modifierKeys ) {
            string str = string.Empty;
            switch ( modifierKeys ) {
                case ModifierKeys.Alt:
                    return "Alt";

                case ModifierKeys.Control:
                    return "Ctrl";

                case ( ModifierKeys.Control | ModifierKeys.Alt ):
                    return str;

                case ModifierKeys.Shift:
                    return "Shift";
            }
            return str;
        }
    }
}