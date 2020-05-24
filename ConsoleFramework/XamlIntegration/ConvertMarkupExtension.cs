using System;
using System.Reflection;
using Binding.Converters;
using Xaml;

namespace ConsoleFramework.Xaml
{
    /// <summary>
    /// Converts Value to property type using specified converter.
    /// </summary>
    [MarkupExtension("Convert")]
    public class ConvertMarkupExtension : IMarkupExtension
    {
        /// <summary>
        /// Converter to be used.
        /// </summary>
        public IBindingConverter Converter { get; set; }

        /// <summary>
        /// Value to convert. String or any object (if created using nested markup extension).
        /// </summary>
        public object Value { get; set; }

        public object ProvideValue( IMarkupExtensionContext context ) {
            if (null == Converter)
                throw new InvalidOperationException("Converter is null");
            if ( null == Value )
                return null;
            PropertyInfo propertyInfo = context.Object.GetType( ).GetProperty( context.PropertyName );
            Type propertyType = propertyInfo.PropertyType;
            Type valueType = Value.GetType( );
            Type firstType = Converter.FirstType;
            Type secondType = Converter.SecondType;
            if ( firstType.IsAssignableFrom( propertyType ) &&
                 secondType.IsAssignableFrom( valueType ) ) {
                ConversionResult conversionResult = Converter.ConvertBack( Value );
                if ( !conversionResult.Success)
                    throw new InvalidOperationException(string.Format(
                        "Cannot convert value : {0}", conversionResult.FailReason));
                return conversionResult.Value;
            } else if ( firstType.IsAssignableFrom( valueType )
                        && secondType.IsAssignableFrom( propertyType ) ) {
                ConversionResult conversionResult = Converter.Convert( Value );
                if ( !conversionResult.Success )
                    throw new InvalidOperationException( string.Format(
                        "Cannot convert value : {0}", conversionResult.FailReason ) );
                return conversionResult.Value;
            } else {
                throw new InvalidOperationException(
                    string.Format("Cannot use specified converter to convert {0} to {1}",
                    valueType.Name, propertyType.Name));
            }
        }
    }
}
