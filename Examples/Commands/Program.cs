using System.ComponentModel;
using ConsoleFramework;
using ConsoleFramework.Controls;
using ConsoleFramework.Events;

namespace Examples.Commands
{
    class Program
    {
        /// <summary>
        /// INotifyPropertyChanged is necessary because we are using TwoWay binding
        /// to ButtonEnabled to pass default value true to CheckBox. If Source doesn't
        /// implement INotifyPropertyChange, TwoWay binding will not work.
        /// </summary>
        private sealed class DataContext : INotifyPropertyChanged {
            public DataContext() {
                command = new RelayCommand( 
                    parameter => MessageBox.Show("Information", "Command executed !", result => { }),
                    parameter => ButtonEnabled );
            }

            private bool buttonEnabled = true;
            public bool ButtonEnabled {
                get { return buttonEnabled; }
                set {
                    if ( buttonEnabled != value ) {
                        buttonEnabled = value;
                        command.RaiseCanExecuteChanged( );
                    }
                }
            }

            private readonly RelayCommand command;

            public ICommand MyCommand {
                get {
                    return command;
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        public static void Main(string[] args) {
            DataContext dataContext = new DataContext();
            WindowsHost windowsHost = (WindowsHost)ConsoleApplication.LoadFromXaml(
                "Examples.Commands.windows-host.xml", dataContext);
            Window mainWindow = (Window)ConsoleApplication.LoadFromXaml(
                "Examples.Commands.main.xml", dataContext);
            windowsHost.Show(mainWindow);
            ConsoleApplication.Instance.Run(windowsHost);
        }
    }
}
