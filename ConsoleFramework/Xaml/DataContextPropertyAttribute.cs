using System;

namespace Xaml {
    /// <summary>
    /// Attribute to specify name of data context property of any type to be bound using XAML.
    /// If no attribute found at type, "DataContext" property will be used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class DataContextPropertyAttribute : Attribute {
        public DataContextPropertyAttribute(string name) {
            Name = name;
        }

        public string Name { get; }
    }
}
