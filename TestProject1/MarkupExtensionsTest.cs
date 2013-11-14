using System;
using ConsoleFramework.Xaml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1
{
    [TestClass]
    public class MarkupExtensionsTest
    {
        public class TestExtension : IMarkupExtension
        {
            public TestExtension( ) {
                Property1 = String.Empty;
                Property2 = String.Empty;
                Property3 = String.Empty;
            }

            public TestExtension( String param1 ) {
                Property1 = param1;
                Property2 = String.Empty;
                Property3 = String.Empty;
            }

            public TestExtension( String param1, String param2 ) {
                Property1 = param1;
                Property2 = param2;
                Property3 = String.Empty;
            }

            public String Property1 { get; set; }

            public String Property2 { get; set; }

            public String Property3 { get; set; }

            public object ProvideValue( object context ) {
                return Property1 + "_" + Property2 + "_" + Property3;
            }
        }

        public class TestResolver : IMarkupExtensionsResolver
        {
            public Type Resolve( string name ) {
                return typeof(TestExtension);
            }
        }

        [TestMethod]
        public void TestEscaping() {
            MarkupExtensionsParser parser = new MarkupExtensionsParser(new TestResolver(),
                "{}Just a string{{}}");
            // should be thrown syntax error: markup extension name is empty
            String result = ( string ) parser.ProcessMarkupExtension( null );
            Assert.AreEqual( "Just a string{{}}", result );
        }

        [TestMethod]
        public void TestEscaping2() {
            MarkupExtensionsParser parser = new MarkupExtensionsParser( new TestResolver(  ),
                "{xm:TestExtension Arg1, Arg2, Property3=\\=\\{\\}\\\\sdf}");
            String result = (String)parser.ProcessMarkupExtension(null);
            Assert.AreEqual( result, "Arg1_Arg2_={}\\sdf" );
        }

        [TestMethod]
        public void TestInner( ) {
            MarkupExtensionsParser parser = new MarkupExtensionsParser(new TestResolver(),
                "{xm:TestExtension Arg1, {TestExtension Property1=1}, Property3=\\=\\{\\}\\\\sdf}");
            String result = (String)parser.ProcessMarkupExtension(null);
            Assert.AreEqual(result, "Arg1_1___={}\\sdf");
        }

        [TestMethod]
        public void TestInner2( ) {
            MarkupExtensionsParser parser = new MarkupExtensionsParser(new TestResolver(),
                "{xm:TestExtension Arg1, Property3=\\=\\{\\}\\\\sdf, Property2={TestExtension Property1=1}}");
            String result = (String)parser.ProcessMarkupExtension(null);
            Assert.AreEqual(result, "Arg1_1___={}\\sdf");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSyntaxError1( ) {
            MarkupExtensionsParser parser = new MarkupExtensionsParser(new TestResolver(), "{ }");
            // should be thrown syntax error: markup extension name is empty
            parser.ProcessMarkupExtension(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSyntaxError2( ) {
            MarkupExtensionsParser parser = new MarkupExtensionsParser(new TestResolver(),
                "{TestExtension* }");
            // should be thrown syntax error: whitespace expected after name
            parser.ProcessMarkupExtension(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSyntaxError3( ) {
            MarkupExtensionsParser parser = new MarkupExtensionsParser(new TestResolver(),
                "{TestExtension Property1=1, CtorArg }");
            // should be thrown syntax error: constructor argument cannot be after property assignment
            parser.ProcessMarkupExtension(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSyntaxError4() {
            MarkupExtensionsParser parser = new MarkupExtensionsParser(new TestResolver(),
                "{TestExtension CtorArg, Property1=1 } ");
            // should be thrown syntax error: unexpected characters
            parser.ProcessMarkupExtension(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSyntaxError5() {
            MarkupExtensionsParser parser = new MarkupExtensionsParser(new TestResolver(),
                "{TestExtension CtorArg, Property1=1,}");
            // should be thrown syntax error: member name or string expected
            parser.ProcessMarkupExtension(null);
        }
    }
}
