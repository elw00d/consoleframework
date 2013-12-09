using System;
using Binding.Converters;

namespace Binding
{
    public class ReversedConverter : IBindingConverter {

        IBindingConverter converter;

        public ReversedConverter(IBindingConverter converter) {
            this.converter = converter;
        }

        public Type FirstType {
            get { return converter.SecondType; }
        }

        public Type SecondType {
            get { return converter.FirstType; }
        }

        public ConversionResult Convert(object tFirst) {
            return converter.ConvertBack(tFirst);
        }

        public ConversionResult ConvertBack(object tSecond) {
            return converter.Convert(tSecond);
        }
    }
}