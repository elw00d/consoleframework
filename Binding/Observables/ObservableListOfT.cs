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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        int IList.Add( object value ) {
            int count = this.Count;
            Add( (T) value );
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool IList.Contains( object value ) {
            return Contains( ( T ) value );
        }

        public void Clear() {
            int count = list.Count;
            list.Clear();

            raiseListElementsRemoved(0, count);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        int IList.IndexOf( object value ) {
            return IndexOf( ( T ) value );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        void IList.Insert( int index, object value ) {
            Insert( index, ( T ) value );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
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
                raiseListElementsRemoved(index, 1);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        void ICollection.CopyTo( Array array, int index ) {
            ((ICollection) list).CopyTo( array, index );
        }

        public int Count {
            get {
                return list.Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        object ICollection.SyncRoot { get { return ((ICollection) list).SyncRoot; } }

        /// <summary>
        /// 
        /// </summary>
        bool ICollection.IsSynchronized { get { return ((ICollection)list).IsSynchronized; } }

        public bool IsReadOnly {
            get {
                return list.IsReadOnly;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        bool IList.IsFixedSize { get { return ((IList) list).IsFixedSize; } }

        public int IndexOf(T item) {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item) {
            list.Insert(index, item);
            raiseListElementsAdded(index, 1);
        }

        public void RemoveAt(int index) {
            list.RemoveAt(index);
            raiseListElementsRemoved(index, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        object IList.this[ int index ] {
            get { return this[ index ]; }
            set { this[ index ] = (T) value; }
        }

        public T this[int index] {
            get {
                return list[index];
            }
            set {
                list[index] = value;
                raiseListElementReplaced(index);
            }
        }

        private void raiseListElementsAdded(int index, int length) {
            if (null != ListChanged) {
                ListChanged.Invoke(this, new ListChangedEventArgs(ListChangedEventType.ItemsInserted, index, length));
            }
        }

        private void raiseListElementsRemoved(int index, int length) {
            if (null != ListChanged) {
                ListChanged.Invoke(this, new ListChangedEventArgs(ListChangedEventType.ItemsRemoved, index, length));
            }
        }

        private void raiseListElementReplaced(int index) {
            if (null != ListChanged) {
                ListChanged.Invoke(this, new ListChangedEventArgs(ListChangedEventType.ItemReplaced, index, 1));
            }
        }

        public event ListChangedHandler ListChanged;
    }
}
