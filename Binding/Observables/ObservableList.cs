using System;
using System.Collections;
using System.Collections.Generic;

namespace Binding.Observables
{
    /**
 * {@link IObservableList} implementation.
 *todo : implement methods like RemoveRange
     *todo : throw exception in reordering methods like Reverse or Sort
 * @author igor.kostromin
 *         28.06.13 17:11
 */
public class ObservableList : IObservableList {
    private IList list;
//    private List<IObservableListListener> listeners;

    public ObservableList( IList list ) {
        this.list = list;
//        listeners = new List< IObservableListListener >();
    }

//    public void addObservableListListener( IObservableListListener listener ) {
//        listeners.Add( listener );
//    }
//
//    public void removeObservableListListener( IObservableListListener listener ) {
//        listeners.Remove( listener );
//    }

    public IEnumerator GetEnumerator( ) {
        return list.GetEnumerator( );
    }

    IEnumerator IEnumerable.GetEnumerator( ) {
        return GetEnumerator( );
    }

    public int Add( Object item ) {
        int index = list.Count;
        list.Add( item );

        raiseListElementsAdded( index, 1 );
        return index;
    }

    public void Clear( ) {
        int count = list.Count;
        list.Clear();

        raiseListElementsRemoved( 0, count );
    }

    public bool Contains( Object item ) {
        return list.Contains( item );
    }

    public void CopyTo( object[ ] array, int arrayIndex ) {
        list.CopyTo( array, arrayIndex );
    }

    public void Remove( Object item ) {
        int index = list.IndexOf(item);
        /*bool result = */list.Remove( item );
        if (-1 != index)
            raiseListElementsRemoved( index, 1 );
        //return result;
    }

    public void CopyTo( Array array, int index ) {
        list.CopyTo( array, index );
    }

    public int Count { get { return list.Count; } }

    public object SyncRoot { get { return list.SyncRoot; } }
    public bool IsSynchronized { get { return list.IsSynchronized; } }
    public bool IsReadOnly { get { return list.IsReadOnly; } }
    public bool IsFixedSize { get { return list.IsFixedSize; } }

    public int IndexOf( Object item ) {
        return list.IndexOf( item );
    }

    public void Insert( int index, Object item ) {
        list.Insert( index, item );
        raiseListElementsAdded( index, 1 );
    }

    public void RemoveAt( int index ) {
        list.RemoveAt( index );
        raiseListElementsRemoved( index, 1 );
    }

    public Object this[ int index ] {
        get { return list[ index ]; }
        set {
            list[ index ] = value;
            raiseListElementReplaced( index );
        }
    }

    private void raiseListElementsAdded( int index, int length ) {
        if (null != ListChanged) {
            ListChanged.Invoke(this, new ListChangedEventArgs(ListChangedEventType.ItemsInserted, index, length));
        }
//        List< IObservableListListener> copy = new List< IObservableListListener>( this.listeners );
//        foreach ( var listener in copy ) {
//            listener.listElementsAdded( this, index, length );
//        }
    }

    private void raiseListElementsRemoved( int index, int length ) {
        if (null != ListChanged) {
            ListChanged.Invoke(this, new ListChangedEventArgs(ListChangedEventType.ItemsRemoved, index, length));
        }
//        List< IObservableListListener > copy = new List< IObservableListListener>( this.listeners );
//        foreach ( var listener in copy ) {
//            listener.listElementsRemoved( this, index, oldElements );
//        }
    }

    private void raiseListElementReplaced( int index ) {
        if (null != ListChanged) {
            ListChanged.Invoke(this, new ListChangedEventArgs(ListChangedEventType.ItemReplaced, index, 1));
        }
//        List< IObservableListListener> copy = new List< IObservableListListener>( this.listeners );
//        foreach ( var listener in copy ) {
//            listener.listElementReplaced( this, index, oldElement );
//        }
    }

    public event ListChangedHandler ListChanged;
}

}
