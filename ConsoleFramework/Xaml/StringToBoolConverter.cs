using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleFramework.Xaml
{
    public interface ITypeConverter<TSource, TDest>
    {
        TDest Convert( TSource value );
    }

    public class StringToBoolConverter : ITypeConverter<String, Boolean>
    {
        public bool Convert( string value ) {
            return bool.Parse( value );
        }
    }
}
