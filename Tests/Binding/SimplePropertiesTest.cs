using System;
using System.ComponentModel;
using Binding;
using Xunit;

namespace TestProject1.Binding
{
    public class SimplePropertiesTest
    {
        class TargetClass
        {
            public String Title { get; set; }
            public int TargetInt { get; set; }
            public String TargetStr { get; set; }
        }

        class SourceClass : INotifyPropertyChanged
        {
            public String Text
            {
                get { return text; }
                set
                {
                    if (value != text)
                    {
                        text = value;
                        raisePropertyChanged("Text");
                    }
                }
            }

            public String SourceStr
            {
                get { return sourceStr; }
                set
                {
                    if (value != sourceStr)
                    {
                        sourceStr = value;
                        raisePropertyChanged("SourceStr");
                    }
                }
            }

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

            private string text;
            private string sourceStr;
            private int sourceInt;

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void raisePropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if ( handler != null ) handler( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        [Fact]
        public void TestString()
        {
            SourceClass source = new SourceClass();
            TargetClass target = new TargetClass();
            BindingBase binding = new BindingBase(target, "Title", source, "Text", BindingMode.OneWay);
            binding.Bind();
            source.Text = "Text!";
            Assert.Equal( target.Title, source.Text );
        }

        [Fact]
        public void TestConversion( ) {
            SourceClass source = new SourceClass();
            TargetClass target = new TargetClass();
            BindingBase binding = new BindingBase(target, "TargetInt", source, "SourceStr", BindingMode.OneWay);
            BindingBase binding2 = new BindingBase(target, "TargetStr", source, "SourceInt", BindingMode.OneWay);
            binding.Bind();
            binding2.Bind(  );
            source.SourceInt = 5;
            source.SourceStr = "4";
            Assert.Equal(target.TargetInt, 4);
            Assert.Equal(target.TargetStr, "5");
        }

        [Fact]
        public void TestValidation( ) {
            SourceClass source = new SourceClass();
            TargetClass target = new TargetClass();
            BindingBase binding = new BindingBase(target, "TargetStr", source, "SourceInt", BindingMode.OneWay);
            binding.UpdateSourceIfBindingFails = false;
            binding.Bind();
            target.TargetInt = 1;
            source.SourceStr = "invalid int";
            Assert.True( target.TargetInt == 1 );
        }
    }
}
