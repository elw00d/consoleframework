using System;

namespace Xaml
{
    /// <summary>
    /// Specifies type of converter to be used to convert from or to current type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct
        | AttributeTargets.Enum, Inherited = true)]
    public class TypeConverterAttribute : Attribute
    {
        public TypeConverterAttribute( ) {
        }

        public TypeConverterAttribute( Type type ) {
            Type = type;
        }

        public Type Type { get; set; }
    }
}
