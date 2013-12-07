using System;
using System.Collections;
using System.Collections.Generic;

namespace Binding.Observables
{
    /**
 * {@link IObservableList} implementation.
 *
 * @author igor.kostromin
 *         28.06.13 17:11
 */
public class ObservableList : IObservableList {
    private IList list;
    private List<IObservableListListener> listeners;

    public ObservableList( IList list ) {
        this.list = list;
        listeners = new List< IObservableListListener >();
    }

    public void addObservableListListener( IObservableListListener listener ) {
        listeners.Add( listener );
    }

    public void removeObservableListListener( IObservableListListener listener ) {
        listeners.Remove( listener );
    }

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
        ArrayList deleted = new ArrayList(list);
        list.Clear();

        raiseListElementsRemoved( 0, deleted );
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
            raiseListElementsRemoved( index, new ArrayList() {item} );
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
        Object item = list[ index ];
        list.RemoveAt( index );
        raiseListElementsRemoved( index, new ArrayList() {item} );
    }

    public Object this[ int index ] {
        get { return list[ index ]; }
        set {
            Object oldElement = list[index];
            list[ index ] = value;
            raiseListElementReplaced( index, oldElement );
        }
    }

    private void raiseListElementsAdded( int index, int length ) {
        List< IObservableListListener> copy = new List< IObservableListListener>( this.listeners );
        foreach ( var listener in copy ) {
            listener.listElementsAdded( this, index, length );
        }
    }

    private void raiseListElementsRemoved( int index, IList oldElements ) {
        List< IObservableListListener > copy = new List< IObservableListListener>( this.listeners );
        foreach ( var listener in copy ) {
            listener.listElementsRemoved( this, index, oldElements );
        }
    }

    private void raiseListElementReplaced( int index, Object oldElement ) {
        List< IObservableListListener> copy = new List< IObservableListListener>( this.listeners );
        foreach ( var listener in copy ) {
            listener.listElementReplaced( this, index, oldElement );
        }
    }
}

}
