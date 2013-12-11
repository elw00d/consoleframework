using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ConsoleFramework;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;
using ConsoleFramework.Events;

namespace PhonesBook
{
    class Program
    {
        static void Main(string[] args) {
            using (ConsoleApplication application = ConsoleApplication.Instance) {
                Panel panelName = new Panel();
                panelName.Name = "panel1";
                panelName.Orientation = Orientation.Horizontal;
                panelName.HorizontalAlignment = HorizontalAlignment.Left;
                panelName.VerticalAlignment = VerticalAlignment.Stretch;
                panelName.AddChild(new TextBlock() {
                    Name = "labelName",
                    Text = "    Имя",
                    TabOrder = 2,
                    //Margin = new Thickness(1, 2, 1, 0)
                });
                TextBox textBox = new TextBox() {
                    Name = "textBoxName",
                    TabOrder = 1,
//                    Size = 30,
                    Margin = new Thickness(1, 0, 1, 0)
                };
                panelName.AddChild(textBox);

                Panel panelPhone = new Panel();
                panelPhone.Name = "panelPhone";
                panelPhone.Orientation = Orientation.Horizontal;
                panelPhone.HorizontalAlignment = HorizontalAlignment.Left;
                panelPhone.VerticalAlignment = VerticalAlignment.Stretch;
                panelPhone.Margin = new Thickness(0, 1, 0,0);
                panelPhone.AddChild(new TextBlock() {
                    Name = "labelName",
                    Text = "Телефон",
                    //Margin = new Thickness(1, 2, 1, 0)
                });
                TextBox textBoxPhone = new TextBox() {
//                    Size = 15,
                    Margin = new Thickness(1, 0, 1, 0)
                };
                panelPhone.AddChild(textBoxPhone);

                Panel panelMain = new Panel() {
                    Name = "panelMain",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Margin = new Thickness(1)
                };
                panelMain.AddChild(panelName);
                panelMain.AddChild(panelPhone);
                


                Button button = new Button() {
                    Name = "button1",
                    Caption = "Добавить",
                    Margin = new Thickness(1),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                panelMain.AddChild(button);
                button.OnClick += (sender, eventArgs) => {
                    int x = 3;
                };
                
                WindowsHost windowsHost = new WindowsHost() {
                    Name = "WindowsHost"
                };
                Window window1 = new Window {
                    X = 3,
                    Y = 4,
                    Height = 20,
                    Name = "mainWindow",
                    Title = "Телефоны",
                    Content = panelMain
                };
                windowsHost.Show(new Window() {
                    X = 30,
                    Y = 6,
                    MinHeight = 10,
                    MinWidth = 10,
                    Name = "Window2",
                    Title = "Управление",
                    Content = new TextBlock() {
                        Text = "window2 window2",
                        Name = "Label_window2"
                    }
                });

                window1.ChildToFocus = "textBoxName";
                windowsHost.Show(window1);
                //panelName.IsFocusScope = true;
                //application.FocusManager.SetFocus(textBox);
                //application.FocusManager.SetFocus(null);
                application.Run(windowsHost);
            }
        }
    }
}
