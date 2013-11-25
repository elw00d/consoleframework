using System;
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
public interface IObservableList<T> : IList<T> {
    /**
     * Adds a listener that is notified when the list changes.
     *
     * @param listener the listener to add
     */
    void addObservableListListener(IObservableListListener<T> listener);

    /**
     * Removes a listener.
     *
     * @param listener the listener to remove
     */
    void removeObservableListListener(IObservableListListener<T> listener);
}
}
