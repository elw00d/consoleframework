using System;
using Xaml;
using Xunit;

namespace Tests.Xaml
{
    public class MarkupExtensionsTest
    {
        public class TestExtension : IMarkupExtension
        {
            public TestExtension()
            {
                Property1 = String.Empty;
                Property2 = String.Empty;
                Property3 = String.Empty;
            }

            public TestExtension(String param1)
            {
                Property1 = param1;
                Property2 = String.Empty;
                Property3 = String.Empty;
            }

            public TestExtension(String param1, String param2)
            {
                Property1 = param1;
                Property2 = param2;
                Property3 = String.Empty;
            }

            public String Property1 { get; set; }

            public String Property2 { get; set; }

            public String Property3 { get; set; }

            public object ProvideValue(IMarkupExtensionContext context)
            {
                return Property1 + "_" + Property2 + "_" + Property3;
            }
        }

        public class TestResolver : IMarkupExtensionsResolver
        {
            public Type Resolve(string name)
            {
                return typeof(TestExtension);
            }
        }

        [Fact]
        public void TestEscaping()
        {
            MarkupExtensionsParser parser = new MarkupExtensionsParser(new TestResolver(),
                "{}Just a string{{}}");
            // should be thrown syntax error: markup extension name is empty
            Assert.Throws<InvalidOperationException>(() => {
                parser.ProcessMarkupExtension(null);
            });
        }

        [Fact]
        public void TestEscaping2()
        {
            MarkupExtensionsParser parser = new MarkupExtensionsParser(new TestResolver(),
                @"{xm:TestExtension Arg1, Arg2, Property3=\=\{\}\\sdf}");
            String result = (String) parser.ProcessMarkupExtension(null);
            Assert.Equal(result, @"Arg1_Arg2_={}\sdf");
        }

        [Fact]
        public void TestInner()
        {
            MarkupExtensionsParser parser = new MarkupExtensionsParser(new TestResolver(),
                @"{xm:TestExtension Arg1, {TestExtension Property1=1}, Property3=\=\{\}\\sdf}");
            String result = (String) parser.ProcessMarkupExtension(null);
            Assert.Equal(result, @"Arg1_1___={}\sdf");
        }

        [Fact]
        public void TestInner2()
        {
            MarkupExtensionsParser parser = new MarkupExtensionsParser(new TestResolver(),
                @"{xm:TestExtension Arg1, Property3=\=\{\}\\sdf, Property2={TestExtension Property1=1}}");
            String result = (String) parser.ProcessMarkupExtension(null);
            Assert.Equal(result, @"Arg1_1___={}\sdf");
        }

        [Fact]
        public void TestSyntaxError1()
        {
            MarkupExtensionsParser parser = new MarkupExtensionsParser(new TestResolver(), "{ }");
            // should be thrown syntax error: markup extension name is empty
            Assert.Throws<InvalidOperationException>(() => {
                parser.ProcessMarkupExtension(null);
            });
        }

        [Fact]
        public void TestSyntaxError2()
        {
            MarkupExtensionsParser parser = new MarkupExtensionsParser(new TestResolver(),
                "{TestExtension* }");
            // should be thrown syntax error: whitespace expected after name
            Assert.Throws<InvalidOperationException>(() => {
                parser.ProcessMarkupExtension(null);
            });
        }

        [Fact]
        public void TestSyntaxError3()
        {
            MarkupExtensionsParser parser = new MarkupExtensionsParser(new TestResolver(),
                "{TestExtension Property1=1, CtorArg }");
            // should be thrown syntax error: constructor argument cannot be after property assignment
            Assert.Throws<InvalidOperationException>(() => {
                parser.ProcessMarkupExtension(null);
            });
        }

        [Fact]
        public void TestSyntaxError4()
        {
            MarkupExtensionsParser parser = new MarkupExtensionsParser(new TestResolver(),
                "{TestExtension CtorArg, Property1=1 } ");
            // should be thrown syntax error: unexpected characters
            Assert.Throws<InvalidOperationException>(() => {
                parser.ProcessMarkupExtension(null);
            });
        }

        [Fact]
        public void TestSyntaxError5()
        {
            MarkupExtensionsParser parser = new MarkupExtensionsParser(new TestResolver(),
                "{TestExtension CtorArg, Property1=1,}");
            // should be thrown syntax error: member name or string expected
            Assert.Throws<InvalidOperationException>(() => {
                parser.ProcessMarkupExtension(null);
            });
        }
    }
}