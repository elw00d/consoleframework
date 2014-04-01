using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Binding.Observables;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using Xaml;

namespace ConsoleFramework.Controls
{
    public interface IItemsSource
    {
        IList< TreeItem > GetItems( );
    }

    [ContentProperty("Items")]
    public class TreeItem
    {
        /// <summary>
        /// Pos in TreeView listbox.
        /// </summary>
        internal int Position;

        internal int Level;

        internal String GetDisplayTitle( ) {
            if ( Items.Count != 0 ) {
                return new string( ' ', Level * 2 ) + ">" + " " + Title;
            } else {
                return new string(' ', Level * 2) + "  " + Title;
            }
        }

        public String Title { get; set; }

        private readonly ObservableList<TreeItem> items = new ObservableList<TreeItem>(
            new List< TreeItem >());

        public IList<TreeItem> Items { get { return items; } }

        public bool HasChildren {
            get { return items.Count != 0; }
        }

        public IItemsSource ItemsSource { get; set; }

        public bool Expanded { get; set; }
    }

    [ContentProperty("Items")]
    public class TreeView : Control
    {
        private readonly ObservableList< TreeItem > items = new ObservableList< TreeItem >(
            new List< TreeItem >( ) );
        
        public IList<TreeItem> Items {
            get { return items; }
        }

        public IItemsSource ItemsSource { get; set; }

        private ListBox listBox;

        public TreeView( ) {
            listBox = new ListBox( );
            this.AddChild( listBox );
            this.items.ListChanged += ( sender, args ) => {
                switch ( args.Type ) {
                    case ListChangedEventType.ItemsInserted:
                        {
                            for ( int i = 0; i < args.Count; i++ ) {
                                TreeItem treeItem = this.items[ i + args.Index ];
                                TreeItem prevItem = null;
                                if (i + args.Index - 1 >= 0)
                                    prevItem = this.items[ i + args.Index - 1 ];
                                treeItem.Position = prevItem != null ? prevItem.Position : 0;
                                listBox.Items.Insert(treeItem.Position, treeItem.GetDisplayTitle() );
                                // todo : make this not necessary
                                listBox.Invalidate(  );
                                treeItemsFlat.Insert( treeItem.Position, treeItem );
                            }
                            break;
                        }
                    default:
                        throw new NotSupportedException();
                }
            };
            this.AddHandler( MouseDownEvent, new MouseEventHandler(( sender, args ) => {
                if ( args.Handled ) {
                    expandCollapse(treeItemsFlat[ listBox.SelectedItemIndex ]);
                }
            }), true );
        }

        private readonly List<TreeItem> treeItemsFlat = new List< TreeItem >();

        private void expandCollapse( TreeItem item ) {
            int index = treeItemsFlat.IndexOf( item );
            if ( item.Expanded ) {
                for ( int i = 0; i < item.Items.Count; i++ ) {
                    TreeItem child = item.Items[ i ];
                    treeItemsFlat.RemoveAt( index + 1 );
                    // Учесть уровень вложенности в title
                    listBox.Items.RemoveAt( index + 1 );
                    child.Position = -1;
                }
                for ( int k = index + 1; k < treeItemsFlat.Count; k++ ) {
                    treeItemsFlat[ k ].Position -= item.Items.Count;
                }
                item.Expanded = false;
            } else {
                for ( int i = 0; i < item.Items.Count; i++ ) {
                    TreeItem child = item.Items[ i ];
                    treeItemsFlat.Insert( i + index + 1, child);
                    child.Position = i + index + 1;
                    child.Level = item.Level + 1;

                    // Учесть уровень вложенности в title
                    listBox.Items.Insert(i + index + 1, child.GetDisplayTitle());
                }
                for ( int k = index + 1 + item.Items.Count; k < treeItemsFlat.Count; k++ ) {
                    treeItemsFlat[ k ].Position += item.Items.Count;
                }
                item.Expanded = true;
            }
            // todo : make this not necessary
            listBox.Invalidate(  );
        }

        protected override Size MeasureOverride( Size availableSize ) {
            listBox.Measure( availableSize );
            return listBox.DesiredSize;
        }

        protected override Size ArrangeOverride( Size finalSize ) {
            listBox.Arrange( new Rect(finalSize) );
            return finalSize;
        }
    }
}
