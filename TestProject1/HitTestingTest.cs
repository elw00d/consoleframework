using System;
using ConsoleFramework;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1
{
    [TestClass]
    public class HitTestingTest
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestControlsDoesntLinkedToCanvas() {
            Control a = new Control();
            Control b = new Control();
            Control.FindCommonAncestor(a, b);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestControlsParentDoesntLinkedToCanvas() {
            Control a = new Control(new PhysicalCanvas(0, 0, IntPtr.Zero));
            Control b = new Control();
            Control aa = new Control(a);
            Control bb = new Control(b);
            Control.FindCommonAncestor(aa, bb);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestControlsParentDoesntLinkedToCanvas2() {
            Control a = new Control(new PhysicalCanvas(0, 0, IntPtr.Zero));
            Control b = new Control();
            Control aa = new Control(a);
            Control bb = new Control(b);
            Control.FindCommonAncestor(bb, aa);
        }

        [TestMethod]
        public void TestRootCanvasIsCommonAncestor() {
            PhysicalCanvas physicalCanvas = new PhysicalCanvas(0, 0, IntPtr.Zero);
            Control a = new Control(physicalCanvas);
            Control b = new Control(physicalCanvas);
            Control aa = new Control(a);
            Control bb = new Control(b);
            Control commonAncestor = Control.FindCommonAncestor(aa, bb);
            Control commonAncestor2 = Control.FindCommonAncestor(bb, aa);
            Assert.IsNull(commonAncestor);
            Assert.IsNull(commonAncestor2);
        }

        [TestMethod]
        public void TestSelfIsCommonAncestor() {
            PhysicalCanvas physicalCanvas = new PhysicalCanvas(0, 0, IntPtr.Zero);
            Control a = new Control(physicalCanvas);
            Control commonAncestor = Control.FindCommonAncestor(a, a);
            Assert.AreEqual(commonAncestor, a);
        }

        [TestMethod]
        public void TestNormalSituation() {
            //
            PhysicalCanvas physicalCanvas = new PhysicalCanvas(0, 0, IntPtr.Zero);
            Control x = new Control(physicalCanvas) { Name = "x" };
            Control ancestor = new Control(x) { Name = "ancestor"};
            Control a = new Control(ancestor) { Name = "a" };
            Control aa = new Control(a) { Name = "aa" };
            Control aaa = new Control(aa) { Name = "aaa" };
            Control b = new Control(ancestor) { Name = "b" };
            Assert.AreEqual(Control.FindCommonAncestor(a, b), ancestor);
            Assert.AreEqual(Control.FindCommonAncestor(aa, b), ancestor);
            Control bb = new Control(b) { Name = "bb" };
            Assert.AreEqual(Control.FindCommonAncestor(aa, bb), ancestor);
            //
            Assert.AreEqual(Control.FindCommonAncestor(a, aa), a);
            Assert.AreEqual(Control.FindCommonAncestor(aa, ancestor), ancestor);
            Assert.AreEqual(Control.FindCommonAncestor(b, bb), b);
            Assert.AreEqual(Control.FindCommonAncestor(bb, ancestor), ancestor);
            //
            Assert.AreEqual(Control.FindCommonAncestor(aaa, ancestor), ancestor);
        }

        /// <summary>
        /// todo : repair this test after finish rendering
        /// </summary>
        [TestMethod]
        [Ignore]
        public void TestPointTranslation() {
            PhysicalCanvas physicalCanvas = new PhysicalCanvas(80, 25, IntPtr.Zero);
            Panel panel = new Panel(physicalCanvas) { Name = "panel" };
            Control textblock1 = new TextBlock() { Name = "textblock1", Text = "ff"};
            Control textblock2 = new TextBlock() {Name = "textblock2", Text = "fff"};
            panel.AddChild(textblock1);
            panel.AddChild(textblock2);
            panel.Arrange(new Rect(0, 0, 80, 25));
            //
            Assert.AreEqual(new Point(0, 0), Control.TranslatePoint(panel, new Point(0, 0), null));
            Assert.AreEqual(new Point(0, 0), Control.TranslatePoint(null, new Point(0, 0), textblock1));
            Assert.AreEqual(new Point(0, -12), Control.TranslatePoint(null, new Point(0, 0), textblock2));
            Assert.AreEqual(new Point(0, -12), Control.TranslatePoint(textblock1, new Point(0, 0), textblock2));
            Assert.AreEqual(new Point(10, 12 + 5), Control.TranslatePoint(textblock2, new Point(10, 5), panel));
        }
    }
}
