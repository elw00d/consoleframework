using System.Collections.Generic;
using System.ComponentModel;
using Xaml;
using Xunit;

namespace Tests.Xaml {
    public class TestModel : INotifyPropertyChanged {
        public string Title { get; set; }

        public TestModel SubModel { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    [ContentProperty("Content")]
    [DataContextProperty("CustomDataContext")]
    public class TestObject {
        public object CustomDataContext { get; set; }

        public object Content { get; set; }
    }

    public class XamlDataContextTest {
        [Fact]
        public void TestNestedDataContext() {
            TestModel rootContext = new TestModel() {
                Title = "Root",
                SubModel = new TestModel() {
                    Title = "Nested"
                }
            };
            var obj = XamlParser.CreateFromXaml<TestObject>(
                @"
<test:TestObject xmlns:test=""clr-namespace:Tests.Xaml;assembly=Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null""
        xmlns:x=""http://consoleframework.org/xaml.xsd""
        CustomDataContext=""{Binding Path=SubModel, Mode=OneTime}"">
    <test:TestObject Content=""{Binding Path=Title, Mode=OneTime}""/>
</test:TestObject>
",
                rootContext,
                new List<string> {
                    "clr-namespace:Xaml;assembly=ConsoleFramework",
                    "clr-namespace:ConsoleFramework.Xaml;assembly=ConsoleFramework"
                });
            // Value is bound from parent data context instead of root context
            Assert.Equal("Nested", ((TestObject) obj.Content).Content);
        }
    }
}
