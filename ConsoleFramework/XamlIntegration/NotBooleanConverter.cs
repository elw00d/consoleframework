using System;
using Binding.Converters;

namespace ConsoleFramework.Xaml
{
    public class NotBooleanConverter : IBindingConverter
    {
        public Type FirstType {
            get {
                return typeof(bool);
            }
        }

        public Type SecondType {
            get {
                return typeof(bool);
            }
        }

        public ConversionResult Convert(object first) {
            return new ConversionResult(!(bool) first);
        }

        public ConversionResult ConvertBack(object second) {
            return new ConversionResult(!(bool) second);
        }
    }
}
