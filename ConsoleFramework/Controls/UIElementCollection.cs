using System;
using System.Collections;
using System.Collections.Generic;
using Binding.Observables;

namespace ConsoleFramework.Controls
{
    public partial class Control {
        public class UIElementCollection : IList {
            private readonly IList list = new ObservableList<Control>(new List<Control>());
            private readonly Control parent;

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
                            parent.InsertChildAt(args.Index + i, (Control) list[args.Index + i]);
                        }
                        break;
                    }
                    case ListChangedEventType.ItemsRemoved:
                        for (int i = 0; i < args.Count; i++) {
                            parent.RemoveChild(parent.Children[args.Index]);
                        }
                        break;
                    case ListChangedEventType.ItemReplaced: {
                        parent.RemoveChild(parent.Children[args.Index]);
                        parent.InsertChildAt(args.Index, (Control) list[args.Index]);
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
