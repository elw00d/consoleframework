using ConsoleFramework.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1
{
    [TestClass]
    public class HitTestingTest
    {
        class TestControl : Control
        {
            public new void AddChild( Control control ) {
                base.AddChild( control );
            }
        }

        [TestMethod]
        public void TestControlsDoesntLinkedToCanvas() {
            TestControl a = new TestControl();
            TestControl b = new TestControl();
            Assert.IsNull(Control.FindCommonAncestor(a, b));
        }
        
        [TestMethod]
        public void TestRootCanvasIsCommonAncestor() {
            TestControl a = new TestControl();
            TestControl b = new TestControl();
            TestControl aa = new TestControl();
            a.AddChild(aa);
            TestControl bb = new TestControl();
            b.AddChild(bb);
            Control commonAncestor = Control.FindCommonAncestor(aa, bb);
            Control commonAncestor2 = Control.FindCommonAncestor(bb, aa);
            Assert.IsNull(commonAncestor);
            Assert.IsNull(commonAncestor2);
        }

        [TestMethod]
        public void TestSelfIsCommonAncestor() {
            Control a = new Control();
            Control commonAncestor = Control.FindCommonAncestor(a, a);
            Assert.AreEqual(commonAncestor, a);
        }

        [TestMethod]
        public void TestNormalSituation() {
            //
            TestControl x = new TestControl() { Name = "x" };
            TestControl ancestor = new TestControl() { Name = "ancestor" };
            x.AddChild( ancestor );
            TestControl a = new TestControl() { Name = "a" };
            ancestor.AddChild( a );
            TestControl aa = new TestControl() { Name = "aa" };
            a.AddChild( aa );
            TestControl aaa = new TestControl() { Name = "aaa" };
            aa.AddChild( aaa );
            TestControl b = new TestControl() { Name = "b" };
            ancestor.AddChild( b );
            Assert.AreEqual(Control.FindCommonAncestor(a, b), ancestor);
            Assert.AreEqual(Control.FindCommonAncestor(aa, b), ancestor);
            TestControl bb = new TestControl() { Name = "bb" };
            b.AddChild( bb );
            Assert.AreEqual(Control.FindCommonAncestor(aa, bb), ancestor);
            //
            Assert.AreEqual(Control.FindCommonAncestor(a, aa), a);
            Assert.AreEqual(Control.FindCommonAncestor(aa, ancestor), ancestor);
            Assert.AreEqual(Control.FindCommonAncestor(b, bb), b);
            Assert.AreEqual(Control.FindCommonAncestor(bb, ancestor), ancestor);
            //
            Assert.AreEqual(Control.FindCommonAncestor(aaa, ancestor), ancestor);
        }
    }
}
