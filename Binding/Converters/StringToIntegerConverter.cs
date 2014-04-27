using System;
using System.Globalization;

namespace Binding.Converters
{
    /// <summary>
    /// Converter between String and Integer.
    /// </summary>
    public class StringToIntegerConverter : IBindingConverter {
        public Type FirstType {
            get { return typeof ( String ); }
        }

        public Type SecondType {
            get { return typeof ( int ); }
        }

        public ConversionResult Convert(Object s) {
            try {
                if (s == null) return new ConversionResult( false, "String is null");
                int value = int.Parse(( string ) s);
                return new ConversionResult(value);
            } catch (FormatException e) {
                return new ConversionResult(false, "Incorrect number");
            }
        }

        public ConversionResult ConvertBack(Object integer) {
            return new ConversionResult(((int)integer).ToString(/*CultureInfo.InvariantCulture*/) );
        }
    }

}
