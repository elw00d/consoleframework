using System;

namespace Xaml
{
    /// <summary>
    /// Provides a unified way of converting types of values to other types,
    /// as well as for accessing standard values and subproperties.
    /// </summary>
    public interface ITypeConverter
    {
        /// <summary>
        /// Returns whether this converter can convert an object of one
        /// type to the type of this converter.
        /// </summary>
        bool CanConvertFrom( Type sourceType );

        /// <summary>
        /// Returns whether this converter can convert the object to the specified type.
        /// </summary>
        bool CanConvertTo( Type destinationType );

        /// <summary>
        /// Converts the given value to the type of this converter.
        /// </summary>
        object ConvertFrom( object value );

        /// <summary>
        /// Converts the given value object to the specified type.
        /// </summary>
        object ConvertTo( object value, Type destinationType );
    }
}