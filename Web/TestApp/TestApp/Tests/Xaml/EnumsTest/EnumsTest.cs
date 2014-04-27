using System.Collections.Generic;
using System.IO;
using System.Reflection;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestApp;
using Xaml;

namespace TestProject1.Xaml.EnumsTest
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

//    [TestClass]
    public class EnumsTest
    {
        private string loadResource(string resourceName) {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream)) {
                return reader.ReadToEnd();
            }
        }

//        [TestMethod]
        public void test() {
            string xaml = loadResource("TestProject1.Xaml.EnumsTest.ObjectToCreate.xml");
            ObjectToCreate createdObject = XamlParser.CreateFromXaml<ObjectToCreate>(xaml, null, new List<string>() {
                "clr-namespace:TestProject1.Xaml.EnumsTest;assembly=TestProject1"
            });
            Assert.IsTrue(createdObject.MyEnum == MyEnumeration.Variant2);
        }
    }
}
