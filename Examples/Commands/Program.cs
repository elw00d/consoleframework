using System;
using System.ComponentModel;
using ConsoleFramework;
using ConsoleFramework.Controls;
using ConsoleFramework.Events;

namespace Examples.Commands
{
    class Program
    {
        private class MyCommand : ICommand {
            private readonly DataContext dataContext;
            public MyCommand(DataContext dataContext) {
                this.dataContext = dataContext;
                dataContext.PropertyChanged += (sender, args) => {
                    if (args.PropertyName == "ButtonEnabled") {
                        if (CanExecuteChanged != null)
                            CanExecuteChanged.Invoke(this, EventArgs.Empty);
                    }
                };
            }

            public event EventHandler CanExecuteChanged;
            
            public bool CanExecute(object parameter) {
                return dataContext.ButtonEnabled;
            }

            public void Execute(object parameter) {
                MessageBox.Show("", "", result => {
                    //
                });
            }
        }

        private class DataContext : INotifyPropertyChanged {
            private bool buttonEnabled;
            public bool ButtonEnabled {
                get {
                    return buttonEnabled;
                }
                set {
                    if (buttonEnabled != value) {
                        buttonEnabled = value;
                        raisePropertyChanged("ButtonEnabled");
                    }
                }
            }

            public DataContext() {
                myCommand = new MyCommand(this);
            }

            private readonly ICommand myCommand;

            public ICommand MyCommand {
                get {
                    return myCommand;
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void raisePropertyChanged(string propertyName) {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public static void Main(string[] args) {
            WindowsHost windowsHost = new WindowsHost();
            DataContext context = new DataContext();
            Window mainWindow = (Window)ConsoleApplication.LoadFromXaml("Examples.Commands.main.xml", context);
            windowsHost.Show(mainWindow);
            CheckBox checkBox = mainWindow.FindChildByName<CheckBox>("checkbox");
            checkBox.OnClick += (sender, eventArgs) => {
                eventArgs.Handled = true;
            };
            ConsoleApplication.Instance.Run(windowsHost);
        }
    }
}
