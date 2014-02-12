using System;
using System.Collections;

namespace Binding.Observables
{
    public delegate void ListChangedHandler(object sender, ListChangedEventArgs args);

    public enum ListChangedEventType {
        ItemsInserted,
        ItemsRemoved,
        ItemReplaced
    }

    public class ListChangedEventArgs : EventArgs {
        public ListChangedEventArgs(ListChangedEventType type, int index, int n) {
            this.Type = type;
            this.Index = index;
            this.N = n;
        }

        public readonly ListChangedEventType Type;
        public readonly int Index;
        public readonly int N;
    }
//
//    /**
// * @author igor.kostromin
// *         28.06.13 17:10
// */
//    public interface IObservableListListener
//    {
//        /**
//         * Notification that elements have been added to the list.
//         *
//         * @param list the {@code ObservableList} that has changed
//         * @param index the index the elements were added to
//         * @param length the number of elements that were added
//         */
//        void listElementsAdded(IObservableList list, int index, int length);
//
//        /**
//         * Notification that elements have been removed from the list.
//         *
//         * @param list the {@code ObservableList} that has changed
//         * @param index the starting index the elements were removed from
//         * @param oldElements a list containing the elements that were removed.
//         */
//        void listElementsRemoved(IObservableList list, int index,
//            IList oldElements);
//
//        /**
//         * Notification that an element has been replaced by another in the list.
//         *
//         * @param list the {@code ObservableList} that has changed
//         * @param index the index of the element that was replaced
//         * @param oldElement the element at the index before the change
//         */
//        void listElementReplaced(IObservableList list, int index,
//            Object oldElement);
//
//    }

}
