using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xaml;

namespace TestProject1.Xaml.TypeExtensionTest
{
    public class ObjectToCreate {
        public Type Type {
            get;
            set;
        }
    }

    [TestClass]
    public class Test
    {
        private string loadResource(string resourceName) {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream)) {
                return reader.ReadToEnd();
            }
        }

        [TestMethod]
        public void test() {
            string xaml = loadResource("TestProject1.Xaml.TypeExtensionTest.object.xml");
            ObjectToCreate createdObject = XamlParser.CreateFromXaml<ObjectToCreate>(xaml, null, new List<string>() {
                "clr-namespace:TestProject1.Xaml.TypeExtensionTest;assembly=TestProject1"
            });
            Assert.IsTrue(createdObject.Type == typeof(ObjectToCreate));
        }
    }
}
