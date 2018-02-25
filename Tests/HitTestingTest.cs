using ConsoleFramework.Controls;
using Xunit;

namespace Tests
{
    public class HitTestingTest
    {
        class TestControl : Control
        {
            public new void AddChild( Control control ) {
                base.AddChild( control );
            }
        }

        [Fact]
        public void TestControlsDoesntLinkedToCanvas() {
            TestControl a = new TestControl();
            TestControl b = new TestControl();
            Assert.Null(Control.FindCommonAncestor(a, b));
        }
        
        [Fact]
        public void TestRootCanvasIsCommonAncestor() {
            TestControl a = new TestControl();
            TestControl b = new TestControl();
            TestControl aa = new TestControl();
            a.AddChild(aa);
            TestControl bb = new TestControl();
            b.AddChild(bb);
            Control commonAncestor = Control.FindCommonAncestor(aa, bb);
            Control commonAncestor2 = Control.FindCommonAncestor(bb, aa);
            Assert.Null(commonAncestor);
            Assert.Null(commonAncestor2);
        }

        [Fact]
        public void TestSelfIsCommonAncestor() {
            Control a = new Control();
            Control commonAncestor = Control.FindCommonAncestor(a, a);
            Assert.Equal(commonAncestor, a);
        }

        [Fact]
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
            Assert.Equal(Control.FindCommonAncestor(a, b), ancestor);
            Assert.Equal(Control.FindCommonAncestor(aa, b), ancestor);
            TestControl bb = new TestControl() { Name = "bb" };
            b.AddChild( bb );
            Assert.Equal(Control.FindCommonAncestor(aa, bb), ancestor);
            //
            Assert.Equal(Control.FindCommonAncestor(a, aa), a);
            Assert.Equal(Control.FindCommonAncestor(aa, ancestor), ancestor);
            Assert.Equal(Control.FindCommonAncestor(b, bb), b);
            Assert.Equal(Control.FindCommonAncestor(bb, ancestor), ancestor);
            //
            Assert.Equal(Control.FindCommonAncestor(aaa, ancestor), ancestor);
        }
    }
}
