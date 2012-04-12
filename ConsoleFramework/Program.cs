using System;
using System.Diagnostics;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;
using ConsoleFramework.Native;

namespace ConsoleFramework {
    internal class Program {
        private static void Main(string[] args) {
            using (ConsoleApplication application = ConsoleApplication.Instance) {
                Panel panel = new Panel();
                panel.Name = "panel1";
                panel.HorizontalAlignment =  HorizontalAlignment.Center;
                panel.VerticalAlignment = VerticalAlignment.Stretch;
                panel.AddChild(new TextBlock() {
                    Name = "label1",
                    Text = "Label1",
                    Margin = new Thickness(1,2,1,0)
                });
                panel.AddChild(new TextBlock() {
                    Name = "label2",
                    Text = "Label2_____",
                    HorizontalAlignment = HorizontalAlignment.Right
                });
                panel.AddChild(new TextBox() {
                    Width = 10, Height = 1
                });
                Button button = new Button() {
                    Name = "button1",
                    Caption = "Button!",
                    Margin = new Thickness(1),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                button.OnClick += (sender, eventArgs) => {
                    Debug.WriteLine("Click");
                };
                panel.AddChild(button);
                //application.Run(panel);
                WindowsHost windowsHost = new WindowsHost()
                                              {
                                                  Name = "WindowsHost"
                                              };
                Window window1 = new Window {
                    X = 3,
                    Y = 4,
                    //MinHeight = 100,
                    //MaxWidth = 30,
                    Width = 10,
                    Height = 20,
                    C = '1',
                    Name = "Window1",
                    Title = "Window1",
                    Content = panel
                };
                windowsHost.AddWindow(window1);
                windowsHost.AddWindow(new Window() {
                    X = 10,
                    Y = 6,
                    MinHeight = 10,
                    MinWidth = 10,
                    C = '2',
                    Name = "Window2",
                    Title = "Очень длинное название окна",
                    Content = new TextBlock() {
                        Text = "window2 window2",
                        Name = "Label_window2"
                    }
                });
                application.Run(windowsHost);
            }
        }
    }
}