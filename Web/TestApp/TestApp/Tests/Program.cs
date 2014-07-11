using System;
using TestProject1.Binding;
using TestProject1.Xaml;

namespace TestApp.Tests
{
    public class Program
    {
        public static void Main( string[ ] args ) {
            Console.WriteLine("Starting tests..");

            AdapterTest adapterTest = new AdapterTest(  );
            adapterTest.TestMethod1(  );

            CollectionRebindTest collectionRebindTest = new CollectionRebindTest(  );
            collectionRebindTest.TestListRebind(  );

            CollectionsTest collectionsTest = new CollectionsTest(  );
            collectionsTest.TestListBinding(  );
            collectionsTest.TestListBinding2(  );

//            ExplicitConverterTest explicitConverterTest = new ExplicitConverterTest(  );
//            explicitConverterTest.TestMethod1(  );

            SimplePropertiesTest simplePropertiesTest = new SimplePropertiesTest(  );
            simplePropertiesTest.TestConversion(  );
            simplePropertiesTest.TestString(  );
            simplePropertiesTest.TestValidation(  );

            ValidationTest validationTest = new ValidationTest(  );
            validationTest.TestMethod1(  );

            Console.WriteLine("Tests completed successfully.");

            Console.WriteLine("Starting XAML tests..");
            XamlTest xamlTest = new XamlTest(  );
            xamlTest.TestXamlObject1(  );
            Console.WriteLine("XAML tests completed successfully.");
        }
    }
}
