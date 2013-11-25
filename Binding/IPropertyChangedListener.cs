using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binding
{
    /**
 * Listener of property changed event.
 *
 * @author igor.kostromin
 *         26.06.13 16:00
 */
    public interface IPropertyChangedListener
    {
        void propertyChanged(String propertyName);
    }
}
