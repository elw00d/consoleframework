using ConsoleFramework;
using ConsoleFramework.Controls;

namespace Examples.RadioButtons
{
    class Program
    {
        public static void Main( string[ ] args ) {
            WindowsHost windowsHost = new WindowsHost( );
            Window mainWindow = ( Window ) ConsoleApplication.LoadFromXaml( "Examples.RadioButtons.main.xml", null );
            windowsHost.Show( mainWindow );

            var button = mainWindow.FindChildByName<Button>("btn");
            button.OnClick += (sender, eventArgs) =>
            {
                var radioGroup = mainWindow.FindChildByName<RadioGroup>("radio");
                if (radioGroup.SelectedItem == null)
                    MessageBox.Show("", "Not selected yet", result => { });
                else
                    MessageBox.Show("", radioGroup.SelectedItem.Caption, result => { });
            };

            ConsoleApplication.Instance.Run( windowsHost );
        }
    }
}
