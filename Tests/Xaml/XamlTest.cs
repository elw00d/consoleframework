using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Binding.Observables;
using Xaml;
using Xunit;

namespace Tests.Xaml
{
    public class XamlTest
    {
        public class XamlObject
        {
            private readonly int x;
            public int X { get{return x;} }

            public XamlObject( int x ) {
                this.x = x;
            }

            public String StrProp { get; set; }

            public XamlObject Content { get; set; }

            private List<String>  items = new List< string >();
            public List< String > Items {
                get {return items;}
            }
        }

        public class ItemsDonor
        {
            private IObservableList items = new ObservableList( new List< string >() );
            public IObservableList Items
            {
                get { return items; }
            }
        }

        /// <summary>
        /// Just a loading object without default ctor using standard ObjectFactory.
        /// </summary>
        [Fact]
        public void TestXamlObject1()
        {
            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "Tests.Xaml.XamlObject1.xml";
            XamlObject createdFromXaml;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                createdFromXaml = XamlParser.CreateFromXaml<XamlObject>(result, null, new List<string>());
            }
            Assert.True( createdFromXaml.X == 5 );
            Assert.True(createdFromXaml.StrProp == "str");
            Assert.True(createdFromXaml.Content.X == 10);
        }
    }
}
