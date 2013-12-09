using System;

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
        private readonly Object value;
        private readonly bool success;
        private readonly String failReason;

        public object Value {
            get { return value; }
        }

        public bool Success {
            get { return success; }
        }

        public string FailReason {
            get { return failReason; }
        }

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
