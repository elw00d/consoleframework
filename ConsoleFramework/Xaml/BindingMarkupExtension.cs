using System;
using System.ComponentModel;
using Binding;

namespace ConsoleFramework.Xaml
{
    class BindingMarkupExtension : IMarkupExtension
    {
        public String Path { get; set; }

        public String Mode { get; set; }

        public object ProvideValue(IMarkupExtensionContext context) {
            if ( null != context.DataContext && context.DataContext is INotifyPropertyChanged) {
                BindingMode mode = BindingMode.Default;
                if ( Path != null ) {
                    Type enumType = typeof ( BindingMode );
                    string[ ] enumNames = enumType.GetEnumNames( );
                    for ( int i = 0, len = enumNames.Length; i < len; i++ ) {
                        if ( enumNames[ i ] == Mode ) {
                            mode = ( BindingMode ) Enum.ToObject( enumType, enumType.GetEnumValues( ).GetValue( i ) );
                        }
                    }
                }
                BindingBase binding = new BindingBase( context.Object, context.PropertyName,
                    (INotifyPropertyChanged) context.DataContext, Path, mode);
                binding.Bind(  );
                // mb return actual property value ?
                return null;
            }
            return null;
        }
    }
}
