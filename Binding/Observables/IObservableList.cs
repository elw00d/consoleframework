using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binding.Observables
{
    /**
 * A {@link List} that notifies listeners of changes.
 *
 * @author igor.kostromin
 *         28.06.13 17:09
 */
public interface IObservableList : IList {
    /**
     * Adds a listener that is notified when the list changes.
     *
     * @param listener the listener to add
     */
    void addObservableListListener(IObservableListListener listener);

    /**
     * Removes a listener.
     *
     * @param listener the listener to remove
     */
    void removeObservableListListener(IObservableListListener listener);
}
}
