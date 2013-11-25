using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binding
{
    /**
 * Listener of data synchonization from Target to Source.
 * It is not called if data flows from Source to Target (because there are no validation).
 *
 * @author igor.kostromin
 *         27.06.13 12:56
 */
    public interface IBindingResultListener
    {
        /**
         * Called when Target has updated the Source.
         */
        void onBinding(BindingResult result);
    }

}
