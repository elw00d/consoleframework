using System;

namespace Binding.Validators
{
    /// <summary>
    /// Defines the interface that objects that participate binding validation must implement.
    /// </summary>
    public interface IBindingValidator
    {
        /// <summary>
        /// Validates value.
        /// </summary>
        ValidationResult Validate( Object value );
    }
}
