using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binding
{
    /**
 * Notifies clients that a property value has changed.
 * All classes participates in data binding scenarios as data source must
 * implement this interface (directly or indirectly using adapter).
 *
 * @author igor.kostromin
 *         26.06.13 15:59
 */
    public interface INotifyPropertyChanged
    {
        /**
         * Subscribes listener to property changed event.
         */
        void addPropertyChangedListener(IPropertyChangedListener listener);

        /**
         * Unsubscribes listener to property changed event.
         */
        void removePropertyChangedListener(IPropertyChangedListener listener);
    }

}
