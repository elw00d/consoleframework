using System.Collections.Generic;
using System.ComponentModel;
using ConsoleFramework;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;
using ConsoleFramework.Events;

namespace Examples.MainMenu
{
    class Program
    {
        // Example of binding menu item to command
        private sealed class DataContext : INotifyPropertyChanged
        {
            public DataContext( ) {
                command = new RelayCommand(
                    parameter => MessageBox.Show( "Information", "Command executed !", result => { } ),
                    parameter => true );
            }
            
            private readonly RelayCommand command;

            public ICommand MyCommand {
                get { return command; }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        public static void Main( string[ ] args ) {
            WindowsHost windowsHost = ( WindowsHost ) ConsoleApplication.LoadFromXaml( "Examples.MainMenu.windows-host.xml", null );
            DataContext dataContext = new DataContext(  );
            Window mainWindow = ( Window ) ConsoleApplication.LoadFromXaml( 
                "Examples.MainMenu.main.xml", dataContext );
            windowsHost.Show( mainWindow );

            // Example of direct subscribing to Click event
            List< Control > menuItems = VisualTreeHelper.FindAllChilds( windowsHost.MainMenu, control => control is MenuItem );
            foreach ( Control menuItem in menuItems ) {
                MenuItem item = ( ( MenuItem ) menuItem );
                if ( item.Title == "Go" ) {
                    item.Click += ( sender, eventArgs ) => {
                        MessageBox.Show( "", "", result => {
                            //
                        } );
                    };
                }
            }
            ConsoleApplication.Instance.Run( windowsHost );
        }
    }
}
