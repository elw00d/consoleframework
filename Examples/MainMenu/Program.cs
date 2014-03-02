using ConsoleFramework;
using ConsoleFramework.Controls;

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
            ConsoleApplication.Instance.Run( windowsHost );
        }
    }
}
