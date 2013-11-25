using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binding.Converters
{
    /**
 * Provides value conversion logic from first class to second and back.
 *
 * @author igor.kostromin
 *         26.06.13 16:37
 */
    public interface IBindingConverter<TFirst, TSecond>
    {
        /**
         * Converts value from TFirst class to TSecond.
         */
        ConversionResult<TSecond> convert(TFirst first);

        /**
         * Converts value from TSecond class to TFirst.
         */
        ConversionResult<TFirst> convertBack(TSecond second);
    }

}
