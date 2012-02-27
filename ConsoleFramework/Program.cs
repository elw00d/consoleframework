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
                application.Run(panel);
            }
        }
    }
}