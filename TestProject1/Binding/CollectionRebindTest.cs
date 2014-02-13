using System;
using System.ComponentModel;
using System.Collections.Generic;
using Binding;
using Binding.Observables;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.Binding
{
    [TestClass]
    public class CollectionRebindTest
    {
        class TargetClass
        {
            public TargetClass() {
                Items = new List<string>();
            }

            public List<String> Items {
                get;
                set;
            }
        }

        class SourceClass : INotifyPropertyChanged
        {
            public SourceClass() {
                SourceItems = new ObservableList(new List<String>());
                // Rebind collection after first change
                // This first change should not affect target list
                SourceItems.ListChanged += (sender, args) => {
                    SourceItems = new ObservableList(new List<String>());
                    raisePropertyChanged("SourceItems");
                };
            }

            public ObservableList SourceItems { get; private set; }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void raisePropertyChanged(string propertyName) {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        [TestMethod]
        public void TestListRebind() {
            SourceClass source = new SourceClass();
            TargetClass target = new TargetClass();
            BindingBase binding = new BindingBase(target, "Items", source, "SourceItems", BindingMode.OneWay);
            binding.Bind();
            source.SourceItems.Add("1");
            // First change should not affect target list
            Assert.IsTrue(target.Items.Count == 0);
            source.SourceItems.Add("1");
            Assert.IsTrue(target.Items[0] == "1");
            source.SourceItems.Remove("1");
            Assert.IsTrue(target.Items.Count == 0);
        }
    }
}
