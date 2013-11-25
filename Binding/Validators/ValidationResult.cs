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
        public bool valid;
        public String message;

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
