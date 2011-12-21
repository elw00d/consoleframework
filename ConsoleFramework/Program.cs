using ConsoleFramework.Controls;

namespace ConsoleFramework {
    internal class Program {
        private static void Main(string[] args) {
            using (ConsoleApplication application = ConsoleApplication.Instance) {
                Panel panel = new Panel();
                panel.AddChild(new TextBlock() {
                    Text = "Label1"
                });
                panel.AddChild(new TextBlock() {
                    Text = "Label2"
                });
                application.Run(panel);
            }
        }
    }
}