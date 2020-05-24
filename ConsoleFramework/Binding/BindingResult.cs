using System;

namespace Binding
{
    /// <summary>
    /// Represents result of one synchronization operation from Target to Source.
    /// If hasConversionError is true, message will represent conversion error message.
    /// If hasValidationError is true, message will represent validation error message.
    /// Both hasConversionError and hasValidationError cannot be set to true.
    /// </summary>
    public class BindingResult
    {
        public bool hasError;
        public bool hasConversionError;
        public bool hasValidationError;
        public String message;

        public BindingResult(bool hasError)
        {
            this.hasError = hasError;
        }

        public BindingResult(bool hasConversionError, bool hasValidationError, String message)
        {
            this.hasConversionError = hasConversionError;
            this.hasValidationError = hasValidationError;
            this.hasError = hasConversionError || hasValidationError;
            this.message = message;
        }
    }

}
