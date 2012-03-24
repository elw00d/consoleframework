using System;
using ConsoleFramework.Controls;

namespace ConsoleFramework {
    internal class Program {
        private static void Main(string[] args) {
            using (ConsoleApplication application = ConsoleApplication.Instance) {
                Panel panel = new Panel();
                panel.AddChild(new TextBlock() {
                    Name = "label1",
                    Text = "Label1"
                });
                panel.AddChild(new TextBlock() {
                    Name = "label2",
                    Text = "Label2_____"
                });
                Button button = new Button() {
                    Name = "button1",
                    Caption = "button !"
                };
                panel.AddChild(button);
                //application.Run(panel);
                WindowsHost windowsHost = new WindowsHost()
                                              {
                                                  Name = "WindowsHost"
                                              };
                Window window1 = new Window {
                    X = 10,
                    Y = 6,
                    Height = 100,
                    Width = 100,
                    C = '1',
                    Name = "Window1",
                    Content = panel
                };
                windowsHost.AddWindow(window1);
                windowsHost.AddWindow(new Window() {
                    X = 10,
                    Y = 6,
                    Height = 10,
                    Width = 10,
                    C = '2',
                    Name = "Window2"
                });
                application.Run(windowsHost);
            }
        }
    }
}