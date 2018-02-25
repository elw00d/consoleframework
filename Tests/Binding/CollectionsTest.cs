using System;
using System.ComponentModel;
using System.Collections.Generic;
using Binding;
using Binding.Observables;
using Xunit;

namespace TestProject1.Binding
{
    public class CollectionsTest
    {
        class TargetClass
        {
            public TargetClass()
            {
                Items = new List<string>();
            }

            public List<String> Items { get; set; }
        }

        class SourceClass : INotifyPropertyChanged
        {
            public SourceClass()
            {
                SourceItems = new ObservableList<String>(new List<String>());
            }

            public ObservableList<String> SourceItems { get; private set; }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void raisePropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        [Fact]
        public void TestListBinding()
        {
            SourceClass source = new SourceClass();
            TargetClass target = new TargetClass();
            BindingBase binding = new BindingBase(target, "Items", source, "SourceItems", BindingMode.OneWay);
            binding.Bind();
            source.SourceItems.Add("1");
            Assert.True(target.Items[0] == "1");
            source.SourceItems.Add("2");
            Assert.True(target.Items[0] == "1");
            Assert.True(target.Items[1] == "2");
            source.SourceItems.Remove("1");
            Assert.True(target.Items.Count == 1);
            Assert.True(target.Items[0] == "2");
        }

        [Fact]
        public void TestListBinding2()
        {
            SourceClass source = new SourceClass();
            TargetClass target = new TargetClass();
            BindingBase binding = new BindingBase(target, "Items", source, "SourceItems", BindingMode.OneWay);
            source.SourceItems.Add("1");
            binding.Bind();
            Assert.True(target.Items[0] == "1");
            source.SourceItems.Remove("1");
            Assert.True(target.Items.Count == 0);
        }
    }
}