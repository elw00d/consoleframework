using System;
using System.Collections.Generic;
using Binding;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.Binding
{
    [TestClass]
    public class ValidationTest
    {
        class TargetClass : INotifyPropertyChanged
        {
            public String TargetStr {
                get { return targetStr; }
                set {
                    if ( targetStr != value ) {
                        targetStr = value;
                        raisePropertyChanged( "TargetStr" );
                    }
                }
            }

            private void raisePropertyChanged(String propertyName)
            {
                foreach (IPropertyChangedListener listener in listeners)
                {
                    listener.propertyChanged(propertyName);
                }
            }

            private List<IPropertyChangedListener> listeners = new List<IPropertyChangedListener>();
            private string targetStr;

            public void addPropertyChangedListener(IPropertyChangedListener listener)
            {
                listeners.Add(listener);
            }

            public void removePropertyChangedListener(IPropertyChangedListener listener)
            {
                listeners.Remove(listener);
            }
        }

        class SourceClass : INotifyPropertyChanged
        {
            public int SourceInt
            {
                get { return sourceInt; }
                set
                {
                    if (value != sourceInt)
                    {
                        sourceInt = value;
                        raisePropertyChanged("SourceInt");
                    }
                }
            }

            private void raisePropertyChanged(String propertyName)
            {
                foreach (IPropertyChangedListener listener in listeners)
                {
                    listener.propertyChanged(propertyName);
                }
            }

            private List<IPropertyChangedListener> listeners = new List<IPropertyChangedListener>();
            private int sourceInt;

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
        public void TestMethod1()
        {
            SourceClass source = new SourceClass(  );
            TargetClass target = new TargetClass(  );
            BindingBase binding = new BindingBase( target, "TargetStr", source, "SourceInt" );
            BindingResult lastResult = null;
            binding.OnBinding += result => {
                lastResult = result;
            };
            binding.bind(  );
            target.TargetStr = "5";
            Assert.IsTrue( source.SourceInt == 5 );
            target.TargetStr = "invalid int";
            Assert.IsTrue(source.SourceInt == 0);
            Assert.IsTrue( lastResult.hasConversionError );
        }
    }
}
