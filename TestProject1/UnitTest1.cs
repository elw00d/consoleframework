using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using ConsoleFramework;
using ConsoleFramework.Native;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            FrameworkElement a = new Window();
            a.X = 2;
            a.Y = 2;
            a.Width = 4;
            a.Height = 4;
            FrameworkElement b = new Window();
            b.X = 4;
            b.Y = 5;
            b.Width = 6;
            b.Height = 5;
            SMALL_RECT? overlappingRegion = ConsoleDispatcher.getOverlappingRegion(a, b);
            Console.WriteLine("test");
        }
    }
}
