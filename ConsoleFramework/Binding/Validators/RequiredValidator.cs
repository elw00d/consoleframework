using System;

namespace Binding.Validators
{
    /// <summary>
    /// Validator checks the value is not null or empty (if value string).
    /// </summary>
    public class RequiredValidator : IBindingValidator {
        public ValidationResult Validate(Object value) {
            if (value == null || value is String && ((String) value).Length == 0)
                return new ValidationResult(false, "Value is required");
            return new ValidationResult(true);
        }
    }
}
