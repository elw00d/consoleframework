using System;
using System.ComponentModel;
using Binding;
using Binding.Converters;
using Xaml;

namespace ConsoleFramework.Xaml
{
    [MarkupExtension("Binding")]
    class BindingMarkupExtension : IMarkupExtension
    {
        public BindingMarkupExtension() {
        }

        public BindingMarkupExtension(string path) {
            Path = path;
        }

        public String Path { get; set; }

        public String Mode { get; set; }

        public Object Source { get; set; }

        /// <summary>
        /// Converter to be used.
        /// </summary>
        public IBindingConverter Converter { get; set; }

        public object ProvideValue(IMarkupExtensionContext context) {
            Object realSource = Source ?? context.DataContext;
            if ( null != realSource && !( realSource is INotifyPropertyChanged ) ) {
                throw new ArgumentException("Source must be INotifyPropertyChanged to use bindings");
            }
            if (null != realSource) {
                BindingMode mode = BindingMode.Default;
                if ( Path != null ) {
                    Type enumType = typeof ( BindingMode );
                    string[ ] enumNames = enumType.GetEnumNames( );
                    for ( int i = 0, len = enumNames.Length; i < len; i++ ) {
                        if ( enumNames[ i ] == Mode ) {
                            mode = ( BindingMode ) Enum.ToObject( enumType, enumType.GetEnumValues( ).GetValue( i ) );
                            break;
                        }
                    }
                }
                BindingBase binding = new BindingBase( context.Object, context.PropertyName,
                    (INotifyPropertyChanged) realSource, Path, mode);
                if ( Converter != null )
                    binding.Converter = Converter;
                binding.Bind(  );
                // mb return actual property value ?
                return null;
            }
            return null;
        }
    }
}
