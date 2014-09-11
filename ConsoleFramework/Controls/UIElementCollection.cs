using System;
using System.Collections;
using System.Collections.Generic;
using Binding.Observables;

namespace ConsoleFramework.Controls
{
    public partial class Control {
        public delegate void ControlAddedEventHandler(Control control);
        public delegate void ControlRemovedEventHandler(Control control);

        public class UIElementCollection : IList {
            private readonly IList list = new ObservableList<Control>(new List<Control>());
            private readonly Control parent;

            public event ControlAddedEventHandler ControlAdded;
            public event ControlAddedEventHandler ControlRemoved;

            public UIElementCollection(Control parent) {
                this.parent = parent;
                ObservableList<Control> observableList = new ObservableList<Control>(new List<Control>());
                this.list = observableList;
                observableList.ListChanged += onListChanged;
            }

            private void onListChanged(object sender, ListChangedEventArgs args) {
                switch (args.Type) {
                    case ListChangedEventType.ItemsInserted: {
                        for (int i = 0; i < args.Count; i++) {
                            var control = (Control) list[args.Index + i];
                            parent.InsertChildAt(args.Index + i, control);
                            if (ControlAdded != null) ControlAdded.Invoke(control);
                        }
                        break;
                    }
                    case ListChangedEventType.ItemsRemoved:
                        for (int i = 0; i < args.Count; i++) {
                            Control control = parent.Children[args.Index];
                            parent.RemoveChild(control);
                            if (ControlRemoved != null) ControlRemoved.Invoke(control);
                        }
                        break;
                    case ListChangedEventType.ItemReplaced: {
                        var removedControl = parent.Children[args.Index];
                        parent.RemoveChild(removedControl);
                        if (ControlRemoved != null) ControlRemoved.Invoke(removedControl);

                        var addedControl = (Control) list[args.Index];
                        parent.InsertChildAt(args.Index, addedControl);
                        if (ControlAdded != null) ControlAdded.Invoke(addedControl);
                        break;
                    }
                }
            }

            public IEnumerator GetEnumerator() {
                return list.GetEnumerator();
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

            public int Add(object value) {
                return list.Add(value);
            }

            public bool Contains(object value) {
                return list.Contains(value);
            }

            public void Clear() {
                list.Clear();
            }

            public int IndexOf(object value) {
                return list.IndexOf(value);
            }

            public void Insert(int index, object value) {
                list.Insert(index, value);
            }

            public void Remove(object value) {
                list.Remove(value);
            }

            public void RemoveAt(int index) {
                list.RemoveAt(index);
            }

            public object this[int index] {
                get {
                    return list[index];
                }
                set {
                    list[index] = value;
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
        }
    }
}
