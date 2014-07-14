using System;
using System.ComponentModel;
using ConsoleFramework;
using ConsoleFramework.Controls;

namespace Examples.TreeView
{
    class Program
    {
        public class Context : INotifyPropertyChanged {
            private string selectedItemTitle;
            public String SelectedItemTitle {
                get {
                    return selectedItemTitle;
                }
                set {
                    if (selectedItemTitle != value) {
                        selectedItemTitle = value;
                        raisePropertyChanged("SelectedItemTitle");
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void raisePropertyChanged(string propertyName) {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
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
            tree.PropertyChanged += (sender, eventArgs) => {
                if (eventArgs.PropertyName == "SelectedItem") {
                    context.SelectedItemTitle = tree.SelectedItem.Title;
                }
            };
            ConsoleApplication.Instance.Run(windowsHost);
        }
    }
}
