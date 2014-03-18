using System;
using ConsoleFramework.Native;
using Xaml;

namespace ConsoleFramework.Events
{
    public class KeyGestureConverter : ITypeConverter
    {
        internal const char DISPLAYSTRING_SEPARATOR = ',';
        private static readonly KeyConverter keyConverter = new KeyConverter( );
        private static readonly ModifierKeysConverter modifierKeysConverter = new ModifierKeysConverter( );
        private const char MODIFIERS_DELIMITER = '+';

        public bool CanConvertFrom( Type sourceType ) {
            return ( sourceType == typeof ( string ) );
        }

        public bool CanConvertTo( Type destinationType ) {
            return ( typeof ( string ) == destinationType );
        }

        public object ConvertFrom( object source ) {
            if ( source == null ) throw new ArgumentNullException( "source" );
            if ( !( source is string ) ) throw new ArgumentException( "source must be string", "source" );
            string str = ( ( string ) source ).Trim( );
            if ( str == string.Empty ) throw new ArgumentException( "source is empty" );

            string afterPlus;
            string beforePlus;
            string afterComma;

            int index = str.IndexOf( DISPLAYSTRING_SEPARATOR );
            if ( index >= 0 ) {
                afterComma = str.Substring( index + 1 ).Trim( );
                str = str.Substring( 0, index ).Trim( );
            } else {
                afterComma = string.Empty;
            }
            index = str.LastIndexOf( MODIFIERS_DELIMITER );
            if ( index >= 0 ) {
                beforePlus = str.Substring( 0, index );
                afterPlus = str.Substring( index + 1 );
            } else {
                beforePlus = string.Empty;
                afterPlus = str;
            }
            ModifierKeys none = ModifierKeys.None;
            object keyObj = keyConverter.ConvertFrom( afterPlus );
            if ( keyObj == null ) throw new InvalidOperationException( "Key is not recognised" );
            object modifierObj = modifierKeysConverter.ConvertFrom( beforePlus );
            if ( modifierObj != null ) {
                none = ( ModifierKeys ) modifierObj;
            }
            return new KeyGesture( ( VirtualKeys ) keyObj, none, afterComma );
        }

        public object ConvertTo( object value, Type destinationType ) {
            if ( destinationType == null ) {
                throw new ArgumentNullException( "destinationType" );
            }
            if ( destinationType != typeof ( string ) )
                throw new NotSupportedException( "destinationType should be string" );

            if ( value == null ) return string.Empty;
            KeyGesture gesture = value as KeyGesture;
            if ( gesture == null ) throw new InvalidOperationException( "Cannot convert null value" );
            string str = "";
            string str2 = ( string ) keyConverter.ConvertTo( gesture.Key, destinationType );
            if ( str2 != string.Empty ) {
                str = str + ( modifierKeysConverter.ConvertTo( gesture.Modifiers, destinationType ) as string );
                if ( str != string.Empty ) {
                    str = str + '+';
                }
                str = str + str2;
                if ( !string.IsNullOrEmpty( gesture.DisplayString ) ) {
                    str = str + ',' + gesture.DisplayString;
                }
            }
            return str;
        }
    }
}