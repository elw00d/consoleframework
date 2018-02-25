using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xaml;
using Xunit;

namespace Tests.Xaml.TypeExtensionTest
{
    public class ObjectToCreate {
        public Type Type {
            get;
            set;
        }
    }

    public class Test
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
            string xaml = loadResource("Tests.Xaml.TypeExtensionTest.object.xml");
            ObjectToCreate createdObject = XamlParser.CreateFromXaml<ObjectToCreate>(xaml, null, new List<string>() {
                "clr-namespace:Tests.Xaml.TypeExtensionTest;assembly=Tests"
            });
            Assert.True(createdObject.Type == typeof(ObjectToCreate));
        }
    }
}
