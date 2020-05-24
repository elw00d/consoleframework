using System;
using System.Collections;
using System.Collections.Generic;

namespace Binding.Observables
{
    /// <summary>
    /// Generic implementation of <see cref="IObservableList"/>.
    /// Non-generic IList is implemented to enforce compatibility with
    /// Collection&lt;T&gt; and List&lt;T&gt;.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableList<T> : IObservableList, IList<T>, IList {
        private readonly IList<T> list;

        public ObservableList(IList<T> list) {
            this.list = list;
        }

        public IEnumerator<T> GetEnumerator() {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(T item) {
            int index = list.Count;
            list.Add(item);
            raiseListElementsAdded(index, 1);
        }

        int IList.Add( object value ) {
            int count = this.Count;
            Add( (T) value );
            return count;
        }

        bool IList.Contains( object value ) {
            return Contains( ( T ) value );
        }

        public void Clear() {
            int count = list.Count;
            List<object> removedItems = new List<object>();
            foreach (T item in list) {
                removedItems.Add(item);
            }
            list.Clear();

            raiseListElementsRemoved(0, count, removedItems);
        }

        int IList.IndexOf( object value ) {
            return IndexOf( ( T ) value );
        }

        void IList.Insert( int index, object value ) {
            Insert( index, ( T ) value );
        }

        void IList.Remove( object value ) {
            Remove( ( T ) value );
        }

        public bool Contains(T item) {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item) {
            int index = list.IndexOf(item);
            list.Remove(item);
            if (-1 != index) {
                raiseListElementsRemoved(index, 1, new List<object>() { item });
                return true;
            }
            return false;
        }

        void ICollection.CopyTo( Array array, int index ) {
            ((ICollection) list).CopyTo( array, index );
        }

        public int Count {
            get {
                return list.Count;
            }
        }

        object ICollection.SyncRoot { get { return ((ICollection) list).SyncRoot; } }

        bool ICollection.IsSynchronized { get { return ((ICollection)list).IsSynchronized; } }

        public bool IsReadOnly {
            get {
                return list.IsReadOnly;
            }
        }

        bool IList.IsFixedSize { get { return ((IList) list).IsFixedSize; } }

        public int IndexOf(T item) {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item) {
            list.Insert(index, item);
            raiseListElementsAdded(index, 1);
        }

        public void RemoveAt(int index) {
            T removedItem = list[index];
            list.RemoveAt(index);
            raiseListElementsRemoved(index, 1, new List<object>() { removedItem });
        }

        object IList.this[ int index ] {
            get { return this[ index ]; }
            set { this[ index ] = (T) value; }
        }

        public T this[int index] {
            get {
                return list[index];
            }
            set {
                T removedItem = list[index];
                list[index] = value;
                raiseListElementReplaced(index, new List<object>() { removedItem });
            }
        }

        private void raiseListElementsAdded(int index, int length) {
            if (null != ListChanged) {
                ListChanged.Invoke(this, new ListChangedEventArgs(ListChangedEventType.ItemsInserted, index, length, null));
            }
        }

        private void raiseListElementsRemoved(int index, int length, List<object> removedItems) {
            if (null != ListChanged) {
                ListChanged.Invoke(this, new ListChangedEventArgs(ListChangedEventType.ItemsRemoved, index, length, removedItems));
            }
        }

        private void raiseListElementReplaced(int index, List<object> removedItems) {
            if (null != ListChanged) {
                ListChanged.Invoke(this, new ListChangedEventArgs(ListChangedEventType.ItemReplaced, index, 1, removedItems));
            }
        }

        public event ListChangedHandler ListChanged;
    }
}