using System;
using Binding.Converters;

namespace Binding
{
    public class ReversedConverter : IBindingConverter {

        IBindingConverter converter;

        public ReversedConverter(IBindingConverter converter) {
            this.converter = converter;
        }

        public Type getFirstClazz( ) {
            return converter.getSecondClazz( );
        }

        public Type getSecondClazz( ) {
            return converter.getFirstClazz( );
        }

        public ConversionResult convert(object tFirst) {
            return converter.convertBack(tFirst);
        }

        public ConversionResult convertBack(object tSecond) {
            return converter.convert(tSecond);
        }
    }
}