using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
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

        public sealed class MainMenuWindowHost : WindowsHost
        {
            public IList<Window> Windows => Children.OfType<Window>().ToList().AsReadOnly();
        }

        public static void UpdateWindowsMenu(MainMenuWindowHost windowsHost)
        {
            var mnuWindows = windowsHost.MainMenu.Items
                .Cast<MenuItem>()
                .Single(x => x.Title == "_Windows");
            mnuWindows.Items.Clear();
            var windowSubMenus = windowsHost
                .Windows
                .Select(x =>
                {
                    var mi = new MenuItem { Title = x.Title };
                    mi.Click += (sender, eventArgs) =>
                    {
                        windowsHost.ActivateWindow(x);
                    };
                    return mi;
                });
            foreach (var windowSubMenu in windowSubMenus)
            {
                mnuWindows.Items.Add(windowSubMenu);
            }
        }

        public static void Main( string[ ] args ) {
            MainMenuWindowHost windowsHost = ( MainMenuWindowHost ) ConsoleApplication.LoadFromXaml( "Examples.MainMenu.windows-host.xml", null );
            DataContext dataContext = new DataContext(  );
            Window mainWindow = ( Window ) ConsoleApplication.LoadFromXaml( "Examples.MainMenu.main.xml", dataContext );
            windowsHost.Show( mainWindow );
            Window otherWindow = ( Window ) ConsoleApplication.LoadFromXaml( "Examples.MainMenu.main.xml", dataContext );
            otherWindow.Title = "Other Window";
            windowsHost.Show( otherWindow );

            UpdateWindowsMenu(windowsHost);
            
            foreach (var window in windowsHost.Windows)
            {
                window.Closed += (sender, eventArgs) =>
                {
                    UpdateWindowsMenu(windowsHost);
                };
            }

            // Example of direct subscribing to Click event
            List< Control > menuItems = VisualTreeHelper.FindAllChilds( windowsHost.MainMenu, control => control is MenuItem );
            foreach ( Control menuItem in menuItems ) {
                MenuItem item = ( ( MenuItem ) menuItem );
                if ( item.Title == "Go" ) {
                    item.Click += ( sender, eventArgs ) => {
                        MessageBox.Show( "Go", "Some text", result => { } );
                    };
                }
            }
            ConsoleApplication.Instance.Run( windowsHost );
        }
    }
}
