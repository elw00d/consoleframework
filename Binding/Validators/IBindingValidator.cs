using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binding.Validators
{
    public interface IBindingValidator
    {
        ValidationResult validate( Object value );
    }

    /**
 * Defines the interface that objects that participate binding validation must implement.
 *
 * @author igor.kostromin
 *         26.06.13 17:50
 */
    public interface IBindingValidator<T> : IBindingValidator
    {
        /**
         * Validates value T.
         */
        //ValidationResult validate(T value);
    }

}
