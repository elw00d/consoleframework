using System;

namespace Binding.Validators
{
    /// <summary>
    /// Represents the result of data binding validation.
    /// </summary>
    public class ValidationResult
    {
        private readonly bool valid;
        private readonly String message;

        public bool Valid {
            get { return valid; }
        }

        public string Message {
            get { return message; }
        }

        public ValidationResult(bool valid)
        {
            this.valid = valid;
        }

        public ValidationResult(bool valid, String message)
        {
            this.valid = valid;
            this.message = message;
        }
    }

}
