using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binding
{
    /**
 * Determines how data will flow - from Source to Target,
 * from Target to Source or both.
 *
 * @author igor.kostromin
 *         26.06.13 16:09
 */
    public enum BindingMode
    {
        /**
         * Data will be synchronized one time in bind() call from Source to Target.
         */
        OneTime,
        /**
         * Data is synchronized in two-way mode. When Source property is changed, it will update the Target property,
         * when Target property is changed, it will update the Source.
         */
        TwoWay,
        /**
         * Data is synchronized from Source to Target only.
         */
        OneWay,
        /**
         * Data is synchronized from Target to Source only.
         */
        OneWayToSource,
        /**
         * Mode is determined by adapter. By default it is TwoWay mode (if no adapter is found).
         */
        Default
    }

}
