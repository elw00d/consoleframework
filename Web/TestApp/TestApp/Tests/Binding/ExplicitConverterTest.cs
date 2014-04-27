using System;
using System.ComponentModel;
using System.Globalization;
using Binding;
using Binding.Converters;
using TestApp;

//using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.Binding
{
//    [TestClass]
    public class ExplicitConverterTest
    {
        class TargetClass : INotifyPropertyChanged
        {
            private string text;
            public String Text {
                get { return text; }
                set {
                    if ( text != value ) {
                        text = value;
                        raisePropertyChanged( "Text" );
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void raisePropertyChanged( string propertyName ) {
                PropertyChangedEventHandler handler = PropertyChanged;
                if ( handler != null ) handler( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        class SourceClass : INotifyPropertyChanged
        {
            public double Val
            {
                get { return sourceInt; }
                set
                {
                    if ( !Equals( value, sourceInt ) )
                    {
                        sourceInt = value;
                        raisePropertyChanged("Val");
                    }
                }
            }

            private double sourceInt;

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void raisePropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        class DoubleToStringConverter : IBindingConverter
        {
            public Type FirstType {
                get { return typeof ( double ); }
            }

            public Type SecondType {
                get { return typeof ( String ); }
            }

            public ConversionResult Convert( object first ) {
                return new ConversionResult( (( double ) first).ToString( 
                    // todo : revert after JSIL issues will have been fixed
                    /*CultureInfo.InvariantCulture*/ ) );
            }

            public ConversionResult ConvertBack( object second ) {
                String s = ( string ) second;
                if (string.IsNullOrEmpty( s ))
                    return new ConversionResult( false, "String is null or empty" );
                double result;
                // todo : revert after JSIL issues will have been fixed
                if ( double.TryParse( s, /*NumberStyles.Any, CultureInfo.InvariantCulture,*/ out result ) ) {
                    return new ConversionResult( result );
                }
                return new ConversionResult( false, "Conversion failed" );
            }
        }

//        [TestMethod]
        public void TestMethod1()
        {
            TargetClass target = new TargetClass(  );
            SourceClass source = new SourceClass(  );
            BindingBase binding = new BindingBase( target, "Text", source, "Val" );
            binding.Converter = new DoubleToStringConverter(  );
            binding.Bind(  );
            source.Val = 3.0f;
            Assert.IsTrue( target.Text == "3" );
            target.Text = "0.5";
            Assert.IsTrue( source.Val == 0.5 );
        }
    }
}
