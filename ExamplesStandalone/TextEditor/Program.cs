using ConsoleFramework;
using ConsoleFramework.Controls;

namespace Examples.TextEditor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WindowsHost windowsHost = new WindowsHost();
            Window mainWindow = (Window)ConsoleApplication.LoadFromXaml("TextEditor.main.xml", null);
            windowsHost.Show(mainWindow);
            ConsoleApplication.Instance.Run(windowsHost);
        }
    }
}
