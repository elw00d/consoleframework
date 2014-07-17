using System;
using System.Collections.Generic;
using System.ComponentModel;
using ConsoleFramework;
using ConsoleFramework.Controls;
using ConsoleFramework.Events;

namespace Examples.TreeView
{
    class Program
    {
        public class Context : INotifyPropertyChanged {

            private bool findItemAndRemoveRecursively(IList<TreeItem> items, TreeItem item) {
                if (items.Contains(item)) {
                    items.Remove(item);
                    return true;
                }
                foreach (TreeItem treeItem in items) {
                    if (findItemAndRemoveRecursively(treeItem.Items, item)) return true;
                }
                return false;
            }

            public Context() {
                removeCommand = new RelayCommand(o => {
                    if (SelectedItem != null) {
                        findItemAndRemoveRecursively(Items, SelectedItem);
                    }
                }, o => {
                    return SelectedItem != null;
                });
            }

            public IList<TreeItem> Items;

            private TreeItem selectedItem;
            public TreeItem SelectedItem {
                get {
                    return selectedItem;
                }
                set {
                    if (selectedItem != value) {
                        selectedItem = value;
                        raisePropertyChanged("SelectedItem");
                        raisePropertyChanged("SelectedItemTitle");
                        removeCommand.RaiseCanExecuteChanged();
                    }
                }
            }

            public String SelectedItemTitle {
                get {
                    return selectedItem != null ? selectedItem.Title : null;
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void raisePropertyChanged(string propertyName) {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }

            private readonly RelayCommand removeCommand;
            public ICommand RemoveCommand {
                get {
                    return removeCommand;
                }
            }
        }

        public static void Main(string[] args) {
            Context context = new Context();
            WindowsHost windowsHost = (WindowsHost)ConsoleApplication.LoadFromXaml(
                "Examples.TreeView.windows-host.xml", null);
            Window mainWindow = (Window)ConsoleApplication.LoadFromXaml(
                "Examples.TreeView.main.xml", context);
            windowsHost.Show(mainWindow);
            ConsoleFramework.Controls.TreeView tree = mainWindow.FindChildByName<ConsoleFramework.Controls.TreeView>("tree");
            // todo : придумать способ для того, чтобы обходиться без такого костыля
            context.Items = tree.Items;
            ConsoleApplication.Instance.Run(windowsHost);
        }
    }
}
