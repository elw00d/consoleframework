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
    public interface IBindingConverter
    {
        /**
     * Returns class object for TFirst class.
     */
        Type FirstType { get; }

        /**
         * Returns class object for TSecond class.
         */
        Type SecondType { get; }

        /**
         * Converts value from TFirst class to TSecond.
         */
        ConversionResult Convert(Object first);

        /**
         * Converts value from TSecond class to TFirst.
         */
        ConversionResult ConvertBack(Object second);
    }

}
