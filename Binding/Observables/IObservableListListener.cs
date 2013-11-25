using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binding.Observables
{
    /**
 * @author igor.kostromin
 *         28.06.13 17:10
 */
    public interface IObservableListListener<T>
    {
        /**
         * Notification that elements have been added to the list.
         *
         * @param list the {@code ObservableList} that has changed
         * @param index the index the elements were added to
         * @param length the number of elements that were added
         */
        void listElementsAdded(IObservableList<T> list, int index, int length);

        /**
         * Notification that elements have been removed from the list.
         *
         * @param list the {@code ObservableList} that has changed
         * @param index the starting index the elements were removed from
         * @param oldElements a list containing the elements that were removed.
         */
        void listElementsRemoved(IObservableList<T> list, int index,
            List<T> oldElements);

        /**
         * Notification that an element has been replaced by another in the list.
         *
         * @param list the {@code ObservableList} that has changed
         * @param index the index of the element that was replaced
         * @param oldElement the element at the index before the change
         */
        void listElementReplaced(IObservableList<T> list, int index,
            Object oldElement);

    }

}
