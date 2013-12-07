using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Binding;
using Binding.Observables;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.Binding
{
    [TestClass]
    public class CollectionsTest
    {
        class TargetClass
        {
            public TargetClass( ) {
                Items = new List< string >();
            }

            public List<String> Items
            {
                get; set; 
            }
        }

        class SourceClass : INotifyPropertyChanged
        {
            public SourceClass( ) {
                SourceItems = new ObservableList( new List<String>() );
            }

            public ObservableList SourceItems { get; private set; }

            private void raisePropertyChanged(String propertyName)
            {
                foreach (IPropertyChangedListener listener in listeners)
                {
                    listener.propertyChanged(propertyName);
                }
            }

            private List<IPropertyChangedListener> listeners = new List<IPropertyChangedListener>();

            public void addPropertyChangedListener(IPropertyChangedListener listener)
            {
                listeners.Add(listener);
            }

            public void removePropertyChangedListener(IPropertyChangedListener listener)
            {
                listeners.Remove(listener);
            }
        }

        [TestMethod]
        public void TestListBinding()
        {
            SourceClass source = new SourceClass(  );
            TargetClass target = new TargetClass(  );
            BindingBase binding = new BindingBase( target, "Items", source, "SourceItems", BindingMode.OneWay );
            binding.bind(  );
            source.SourceItems.Add( "1" );
            Assert.IsTrue( target.Items[0] == "1" );
            source.SourceItems.Remove( "1" );
            Assert.IsTrue(target.Items.Count == 0);
        }
    }
}
