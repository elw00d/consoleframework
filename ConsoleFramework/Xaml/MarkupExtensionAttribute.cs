using System;

namespace Xaml
{
    /// <summary>
    /// Attribute marks the markup extensions and specifies the names
    /// by which they will be available in XAML.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class MarkupExtensionAttribute : Attribute
    {
        public MarkupExtensionAttribute( ) {
        }

        public MarkupExtensionAttribute( string name ) {
            Name = name;
        }

        public String Name { get; set; }
    }
}
