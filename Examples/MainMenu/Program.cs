using System.Collections.Generic;
using ConsoleFramework;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;

namespace Examples.MainMenu
{
    class Program
    {
        public static void Main( string[ ] args ) {
            WindowsHost windowsHost = ( WindowsHost ) ConsoleApplication.LoadFromXaml( "Examples.MainMenu.windows-host.xml", null );
            Window mainWindow = ( Window ) ConsoleApplication.LoadFromXaml( "Examples.MainMenu.main.xml", null );
            windowsHost.Show( mainWindow );
            CheckBox checkBox = mainWindow.FindChildByName<CheckBox>("checkbox");
            checkBox.OnClick += ( sender, eventArgs ) => {
                eventArgs.Handled = true;
            };
            List< Control > menuItems = VisualTreeHelper.FindAllChilds( windowsHost.MainMenu, control => control is MenuItem );
            foreach ( Control menuItem in menuItems ) {
                ( ( MenuItem ) menuItem ).Click += ( sender, eventArgs ) => {
                    MessageBox.Show( "", "", result => {
                        //
                    } );
                };
            }
            ConsoleApplication.Instance.Run( windowsHost );
        }
    }
}
