using System;

namespace Xaml
{
    public interface ITypeConverter
    {
        bool CanConvertFrom( Type sourceType );

        bool CanConvertTo( Type destinationType );

        object ConvertFrom( object value );

        object ConvertTo( object value, Type destinationType );
    }
}