using System;

namespace Xaml
{
    /// <summary>
    /// Позволяет получить тип по имени. Имя может содержать аргументы-типы, например
    /// ConsoleFramework.Xaml.TestClass`1[System.String]
    /// </summary>
    [MarkupExtension("Type")]
    class TypeMarkupExtension : IMarkupExtension
    {
        public TypeMarkupExtension( ) {
        }

        public TypeMarkupExtension( string name ) {
            Name = name;
        }

        public String Name { get; set; }

        public object ProvideValue( IMarkupExtensionContext context ) {
            return Type.GetType( Name );
        }
    }
}
