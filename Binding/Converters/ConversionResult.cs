using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binding.Converters
{
    /**
 * Represents value conversion result.
 *
 * User: igor.kostromin
 * Date: 26.06.13
 * Time: 21:46
 */
    public class ConversionResult
    {
        public Object value;
        public bool success;
        public String failReason;

        public ConversionResult(Object value)
        {
            this.value = value;
            this.success = true;
        }

        public ConversionResult(bool success, String failReason)
        {
            this.success = success;
            this.failReason = failReason;
        }
    }

}
