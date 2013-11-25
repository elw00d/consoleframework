using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Binding.Observables
{
    /**
 * {@link IObservableList} implementation.
 *
 * @author igor.kostromin
 *         28.06.13 17:11
 */
public class ObservableList<E> : IObservableList<E> {
    private List<E> list;
    private List<IObservableListListener<E>> listeners;

    public ObservableList( List<E> list ) {
        this.list = list;
        listeners = new List< IObservableListListener< E > >();
    }



//    public E get( int index ) {
//        return list[ index ];
//    }
//
//    public int size() {
//        return list.Count;
//    }
//
//    public E set( int index, E element ) {
//        E oldValue = list.set( index, element );
//
//        for ( IObservableListListener listener : listeners ) {
//            listener.listElementReplaced( this, index, oldValue );
//        }
//
//        return oldValue;
//    }
//
//    public void add( int index, E element ) {
//        list.add( index, element );
//        modCount++;
//
//        for ( IObservableListListener listener : listeners ) {
//            listener.listElementsAdded( this, index, 1 );
//        }
//    }
//
//    public E remove( int index ) {
//        E oldValue = list.remove( index );
//        modCount++;
//
//        for ( IObservableListListener listener : listeners ) {
//            listener.listElementsRemoved( this, index,
//                    java.util.Collections.singletonList( oldValue ) );
//        }
//
//        return oldValue;
//    }
//
//    public boolean addAll( Collection<? extends E> c ) {
//        return addAll( size(), c );
//    }
//
//    public boolean addAll( int index, Collection<? extends E> c ) {
//        if ( list.addAll( index, c ) ) {
//            modCount++;
//
//            for ( IObservableListListener listener : listeners ) {
//                listener.listElementsAdded( this, index, c.size() );
//            }
//        }
//
//        return false;
//    }
//
//    public void clear() {
//        List<E> dup = new ArrayList<E>( list );
//        list.clear();
//        modCount++;
//
//        if ( dup.size() != 0 ) {
//            for ( IObservableListListener listener : listeners ) {
//                listener.listElementsRemoved( this, 0, dup );
//            }
//        }
//    }
//
//    public boolean containsAll( Collection<?> c ) {
//        return list.containsAll( c );
//    }
//
//    public <T> T[] toArray( T[] a ) {
//        return list.toArray( a );
//    }
//
//    public Object[] toArray() {
//        return list.toArray();
//    }
//
//    public void addObservableListListener( IObservableListListener listener ) {
//        listeners.add( listener );
//    }
//
//    public void removeObservableListListener(
//            IObservableListListener listener ) {
//        listeners.remove( listener );
//    }
    public void addObservableListListener( IObservableListListener< E > listener ) {
        listeners.Add( listener );
    }

    public void removeObservableListListener( IObservableListListener< E > listener ) {
        listeners.Remove( listener );
    }

    public IEnumerator< E > GetEnumerator( ) {
        return list.GetEnumerator( );
    }

    IEnumerator IEnumerable.GetEnumerator( ) {
        return GetEnumerator( );
    }

    public void Add( E item ) {
        int index = list.Count;
        list.Add( item );

        raiseListElementsAdded( index, 1 );
    }

    public void Clear( ) {
        List<E> deleted = new List< E >(list);
        list.Clear();

        raiseListElementsRemoved( 0, deleted );
    }

    public bool Contains( E item ) {
        return list.Contains( item );
    }

    public void CopyTo( E[ ] array, int arrayIndex ) {
        list.CopyTo( array, arrayIndex );
    }

    public bool Remove( E item ) {
        int index = list.IndexOf(item);
        bool result = list.Remove( item );
        if (-1 != index)
            raiseListElementsRemoved( index, new List< E >() {item} );
        return result;
    }

    public int Count { get; private set; }
    public bool IsReadOnly { get; private set; }

    public int IndexOf( E item ) {
        return list.IndexOf( item );
    }

    public void Insert( int index, E item ) {
        list.Insert( index, item );
        raiseListElementsAdded( index, 1 );
    }

    public void RemoveAt( int index ) {
        E item = list[ index ];
        list.RemoveAt( index );
        raiseListElementsRemoved( index, new List< E >() {item} );
    }

    public E this[ int index ] {
        get { return list[ index ]; }
        set {
            E oldElement = list[index];
            list[ index ] = value;
            raiseListElementReplaced( index, oldElement );
        }
    }

    private void raiseListElementsAdded( int index, int length ) {
        List< IObservableListListener< E > > copy = new List< IObservableListListener< E > >( this.listeners );
        foreach ( var listener in copy ) {
            listener.listElementsAdded( this, index, length );
        }
    }

    private void raiseListElementsRemoved( int index, List< E > oldElements ) {
        List< IObservableListListener< E > > copy = new List< IObservableListListener< E > >( this.listeners );
        foreach ( var listener in copy ) {
            listener.listElementsRemoved( this, index, oldElements );
        }
    }

    private void raiseListElementReplaced( int index, Object oldElement ) {
        List< IObservableListListener< E > > copy = new List< IObservableListListener< E > >( this.listeners );
        foreach ( var listener in copy ) {
            listener.listElementReplaced( this, index, oldElement );
        }
    }
}

}
