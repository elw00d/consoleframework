using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binding.Validators
{
    /**
 * Represents the result of data binding validation.
 *
 * User: igor.kostromin
 * Date: 26.06.13
 * Time: 21:53
 */
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
