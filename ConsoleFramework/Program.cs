using ConsoleFramework.Controls;

namespace ConsoleFramework {
    internal class Program {
        private static void Main(string[] args) {
            using (ConsoleApplication application = ConsoleApplication.Instance) {
                application.Run(new BackgroundControl());
            }
        }
    }
}