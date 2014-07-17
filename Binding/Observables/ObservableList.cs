using System;
using System.Collections;

namespace Binding.Observables {
    /// <summary>
    /// Non-generic <see cref="IObservableList"/> implementation.
    /// </summary>
    public class ObservableList : IObservableList, IList {
        private readonly IList list;

        public ObservableList(IList list) {
            this.list = list;
        }

        public IEnumerator GetEnumerator() {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        
        public int Add(Object item) {
            int index = list.Count;
            list.Add(item);

            raiseListElementsAdded(index, 1);
            return index;
        }

        public void Clear() {
            int count = list.Count;
            ArrayList removedItems = new ArrayList(list);
            list.Clear();

            raiseListElementsRemoved(0, count, removedItems);
        }

        public bool Contains(Object item) {
            return list.Contains(item);
        }

        public void CopyTo(object[] array, int arrayIndex) {
            list.CopyTo(array, arrayIndex);
        }

        public void Remove(Object item) {
            int index = list.IndexOf(item);
            list.Remove(item);
            if (-1 != index)
                raiseListElementsRemoved(index, 1, new ArrayList() { item });
        }

        public void CopyTo(Array array, int index) {
            list.CopyTo(array, index);
        }

        public int Count {
            get {
                return list.Count;
            }
        }

        public object SyncRoot {
            get {
                return list.SyncRoot;
            }
        }

        public bool IsSynchronized {
            get {
                return list.IsSynchronized;
            }
        }

        public bool IsReadOnly {
            get {
                return list.IsReadOnly;
            }
        }

        public bool IsFixedSize {
            get {
                return list.IsFixedSize;
            }
        }

        public int IndexOf(Object item) {
            return list.IndexOf(item);
        }

        public void Insert(int index, Object item) {
            list.Insert(index, item);
            raiseListElementsAdded(index, 1);
        }

        public void RemoveAt(int index) {
            object removedItem = list[index];
            list.RemoveAt(index);
            raiseListElementsRemoved(index, 1, new ArrayList() { removedItem });
        }

        public Object this[int index] {
            get {
                return list[index];
            }
            set {
                object removedItem = list[index];
                list[index] = value;
                raiseListElementReplaced(index, new ArrayList() { removedItem });
            }
        }

        private void raiseListElementsAdded(int index, int length) {
            if (null != ListChanged) {
                ListChanged.Invoke(this, new ListChangedEventArgs(ListChangedEventType.ItemsInserted, index, length, null));
            }
        }

        private void raiseListElementsRemoved(int index, int length, ArrayList removedItems) {
            if (null != ListChanged) {
                ListChanged.Invoke(this, new ListChangedEventArgs(ListChangedEventType.ItemsRemoved, index, length, removedItems));
            }
        }

        private void raiseListElementReplaced(int index, ArrayList removedItems) {
            if (null != ListChanged) {
                ListChanged.Invoke(this, new ListChangedEventArgs(ListChangedEventType.ItemReplaced, index, 1, removedItems));
            }
        }

        public event ListChangedHandler ListChanged;
    }
}