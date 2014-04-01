using System.ComponentModel;
using ConsoleFramework;
using ConsoleFramework.Controls;
using ConsoleFramework.Events;

namespace Examples.TreeView
{
    class Program
    {
        public static void Main(string[] args) {
            WindowsHost windowsHost = (WindowsHost)ConsoleApplication.LoadFromXaml(
                "Examples.TreeView.windows-host.xml", null);
            Window mainWindow = (Window)ConsoleApplication.LoadFromXaml(
                "Examples.TreeView.main.xml", null);
            windowsHost.Show(mainWindow);
            ConsoleApplication.Instance.Run(windowsHost);
        }
    }
}
