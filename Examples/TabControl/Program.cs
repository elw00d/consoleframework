using ConsoleFramework;
using ConsoleFramework.Controls;

namespace Examples.TabControl
{
    class Program
    {
        public static void Main( string[ ] args ) {
            WindowsHost windowsHost = new WindowsHost( );
            Window mainWindow = ( Window ) ConsoleApplication.LoadFromXaml( "Examples.TabControl.main.xml", null );
            windowsHost.Show( mainWindow );
            ConsoleApplication.Instance.Run( windowsHost );
        }
    }
}