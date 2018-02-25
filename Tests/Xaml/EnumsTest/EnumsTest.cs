using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xaml;
using Xunit;

namespace Tests.Xaml.EnumsTest
{
    public enum MyEnumeration {
        Variant1,
        Variant2
    }

    public class ObjectToCreate {
        public MyEnumeration MyEnum {
            get;
            set;
        }
    }

    public class EnumsTest
    {
        private string loadResource(string resourceName) {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream)) {
                return reader.ReadToEnd();
            }
        }

        [Fact]
        public void test() {
            string xaml = loadResource("Tests.Xaml.EnumsTest.ObjectToCreate.xml");
            ObjectToCreate createdObject = XamlParser.CreateFromXaml<ObjectToCreate>(xaml, null, new List<string>() {
                "clr-namespace:Tests.Xaml.EnumsTest;assembly=Tests"
            });
            Assert.True(createdObject.MyEnum == MyEnumeration.Variant2);
        }
    }
}
