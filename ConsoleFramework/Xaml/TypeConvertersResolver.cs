using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleFramework.Xaml
{
    public class TypeConvertersResolver
    {
        private readonly List< String > namespaces = new List< string >();

        public TypeConvertersResolver( List<String> defaultNamespaces  ) {
            this.namespaces.AddRange( defaultNamespaces );
        }

        public void AddNamespace( String ns ) {
            this.namespaces.Add( ns );
        }

//        public bool NeedConvert( Type dest, Type src ) {
//            
//        }
//
//        public Object Convert( Type dest, Type src, Object obj ) {
//            
//        }
    }
}
