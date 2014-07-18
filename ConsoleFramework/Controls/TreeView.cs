using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Binding.Observables;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using Xaml;
using ListChangedEventArgs = Binding.Observables.ListChangedEventArgs;

namespace ConsoleFramework.Controls
{
    public interface IItemsSource
    {
        IList< TreeItem > GetItems( );
    }

    [ContentProperty("Items")]
    public class TreeItem : INotifyPropertyChanged
    {
        /// <summary>
        /// Pos in TreeView listbox.
        /// </summary>
        internal int Position;

        internal int Level;

        internal String DisplayTitle {
            get {
                if (Items.Count != 0)
                    return string.Format("{0}{1} {2}", new string(' ', Level*2),
                        (Expanded ? UnicodeTable.ArrowDown : UnicodeTable.ArrowRight), Title);
                return string.Format("{0}{1}", new string(' ', (Level+1)*2), Title);
            }
        }

        // todo : call listBox.Invalidate() if item is visible now
        private string title;
        public String Title {
            get {
                return title;
            }
            set {
                if (title != value) {
                    title = value;
                    raisePropertyChanged("Title");
                    raisePropertyChanged("DisplayTitle");
                }
            }
        }

        private bool disabled;
        public bool Disabled {
            get { return disabled; }
            set {
                if (disabled != value) {
                    disabled = value;
                    raisePropertyChanged("Disabled");
                }
            }
        }

        internal readonly ObservableList<TreeItem> items = new ObservableList<TreeItem>(new List< TreeItem >());

        public IList<TreeItem> Items { get { return items; } }

        public bool HasChildren {
            get { return items.Count != 0; }
        }

        public IItemsSource ItemsSource { get; set; }

        internal bool expanded;
        public bool Expanded {
            get {
                return expanded;
            }
            set {
                if (expanded != value) {
                    expanded = value;
                    raisePropertyChanged("Expanded");
                    raisePropertyChanged("DisplayTitle");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void raisePropertyChanged(string propertyName) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
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

        private readonly ListBox listBox;

        public TreeItem SelectedItem {
            get {
                if (treeItemsFlat.Count == 0) return null;
                return treeItemsFlat[listBox.SelectedItemIndex];
            }
        }

        public TreeView( ) {
            listBox = new ListBox( );
            listBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            listBox.VerticalAlignment = VerticalAlignment.Stretch;
            
            // Stretch by default too
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;

            this.AddChild( listBox );
            this.items.ListChanged += ItemsOnListChanged;

            this.AddHandler( MouseDownEvent, new MouseEventHandler(( sender, args ) => {
                if ( args.Handled ) {
                    expandCollapse(treeItemsFlat[ listBox.SelectedItemIndex ]);
                }
            }), true );

            listBox.SelectedItemIndexChanged += (sender, args) => {
                this.RaisePropertyChanged("SelectedItem");
            };
        }

        private void subscribeToItem(TreeItem item, ListChangedHandler handler) {
            item.items.ListChanged += handler;
            item.PropertyChanged += itemOnPropertyChanged;
            foreach (TreeItem child in item.items) {
                subscribeToItem(child, handler);
            }
        }

        private void unsubscribeFromItem(TreeItem item, ListChangedHandler handler) {
            item.items.ListChanged -= handler;
            item.PropertyChanged -= itemOnPropertyChanged;
            foreach (TreeItem child in item.items) {
                unsubscribeFromItem(child, handler);
            }
        }

        private void itemOnPropertyChanged(object sender, PropertyChangedEventArgs args) {
            TreeItem senderItem = (TreeItem) sender;
            if (args.PropertyName == "DisplayTitle") {
                if (senderItem.Position >= 0) {
                    listBox.Items[senderItem.Position] = senderItem.DisplayTitle;
                }
            }
            if (args.PropertyName == "Disabled") {
                if (senderItem.Position >= 0) {
                    if (senderItem.Disabled)
                        listBox.DisabledItemsIndexes.Add(senderItem.Position);
                    else
                        listBox.DisabledItemsIndexes.Remove(senderItem.Position);
                }
            }
            if (args.PropertyName == "Expanded") {
                if (senderItem.Position >= 0) {
                    if (senderItem.Expanded)
                        expand(senderItem);
                    else
                        collapse(senderItem);
                }
            }
        }

        private void onItemInserted(int pos) {
            TreeItem treeItem = items[pos];
            TreeItem prevItem = null;
            if (pos - 1 >= 0)
                prevItem = this.items[pos - 1];
            treeItem.Position = prevItem != null ? prevItem.Position : 0;
            for (int j = treeItem.Position; j < treeItemsFlat.Count; j++) {
                treeItemsFlat[j].Position++;
            }
            treeItemsFlat.Insert(treeItem.Position, treeItem);
            listBox.Items.Insert(treeItem.Position, treeItem.DisplayTitle);
            if (treeItem.Disabled)
                listBox.DisabledItemsIndexes.Add(treeItem.Position);

            // Handle modification of inner list recursively
            subscribeToItem(treeItem, ItemsOnListChanged);
            if (treeItem.Position <= listBox.SelectedItemIndex)
                RaisePropertyChanged("SelectedItem");
        }

        private void onItemRemoved(TreeItem treeItem) {
            if (treeItem.Expanded) collapse(treeItem);
            treeItemsFlat.RemoveAt(treeItem.Position);
            listBox.Items.RemoveAt(treeItem.Position);
            for (int j = treeItem.Position; j < treeItemsFlat.Count; j++)
                treeItemsFlat[j].Position--;

            // Cleanup event handler recursively
            unsubscribeFromItem(treeItem, ItemsOnListChanged);

            if (listBox.SelectedItemIndex >= treeItem.Position)
                RaisePropertyChanged("SelectedItem");
        }

        private void ItemsOnListChanged(object sender, ListChangedEventArgs args) {
            switch (args.Type) {
                case ListChangedEventType.ItemsInserted: {
                    for (int i = 0; i < args.Count; i++)
                        onItemInserted(i + args.Index);
                    break;
                }
                case ListChangedEventType.ItemsRemoved: {
                    foreach (TreeItem treeItem in args.RemovedItems.Cast<TreeItem>())
                        onItemRemoved(treeItem);
                    break;
                }
                case ListChangedEventType.ItemReplaced: {
                    onItemRemoved((TreeItem) args.RemovedItems[0]);
                    onItemInserted(args.Index);
                    break;
                }
            }
        }

        /// <summary>
        /// Flat list of tree items in order corresponding to actual listbox content.
        /// </summary>
        private readonly List<TreeItem> treeItemsFlat = new List< TreeItem >();

        private void expand(TreeItem item) {
            int index = treeItemsFlat.IndexOf(item);
            for (int i = 0; i < item.Items.Count; i++) {
                TreeItem child = item.Items[i];
                treeItemsFlat.Insert(i + index + 1, child);
                child.Position = i + index + 1;
                child.Level = item.Level + 1;

                // Учесть уровень вложенности в title
                listBox.Items.Insert(i + index + 1, child.DisplayTitle);
                if (child.Disabled) listBox.DisabledItemsIndexes.Add(i + index + 1);
            }
            for (int k = index + 1 + item.Items.Count; k < treeItemsFlat.Count; k++) {
                treeItemsFlat[k].Position += item.Items.Count;
            }
        }

        private void collapse(TreeItem item) {
            int index = treeItemsFlat.IndexOf(item);
            foreach (TreeItem child in item.Items) {
                treeItemsFlat.RemoveAt(index + 1);
                if (child.Disabled) listBox.DisabledItemsIndexes.Remove(index + 1);
                listBox.Items.RemoveAt(index + 1);
                child.Position = -1;
            }
            for (int k = index + 1; k < treeItemsFlat.Count; k++) {
                treeItemsFlat[k].Position -= item.Items.Count;
            }
        }

        private void expandCollapse( TreeItem item ) {
            int index = treeItemsFlat.IndexOf(item);
            if ( item.Expanded ) {
                // Children are collapsed but with Expanded state saved
                foreach (TreeItem child in item.Items.Where(child => child.Expanded)) {
                    collapse(child);
                }

                collapse(item);
                item.expanded = false;
                // Need to update item string (because Expanded status has been changed)
                listBox.Items[index] = item.DisplayTitle;
            } else {
                expand(item);
                item.expanded = true;
                // Need to update item string (because Expanded status has been changed)
                listBox.Items[index] = item.DisplayTitle;

                // Children are expanded too according to their Expanded stored state
                foreach (TreeItem child in item.Items.Where(child => child.Expanded)) {
                    expand(child);
                }
            }
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
