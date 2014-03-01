using ConsoleFramework;
using ConsoleFramework.Controls;

namespace Examples.CheckBoxes
{
    class Program
    {
        public static void Main( string[ ] args ) {
            WindowsHost windowsHost = new WindowsHost( );
            Window mainWindow = ( Window ) ConsoleApplication.LoadFromXaml( "Examples.CheckBoxes.main.xml", null );
            windowsHost.Show( mainWindow );
            CheckBox checkBox = mainWindow.FindChildByName<CheckBox>("checkbox");
            checkBox.OnClick += ( sender, eventArgs ) => {
                eventArgs.Handled = true;
            };
            ConsoleApplication.Instance.Run( windowsHost );
        }
    }
}
