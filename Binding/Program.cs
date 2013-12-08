using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Binding
{
    class TargetClass
    {
        public String Title { get; set; }
    }

    class SourceClass : INotifyPropertyChanged
    {
        public String Text {
            get { return text; }
            set {
                if ( value != text ) {
                    text = value;
                    raisePropertyChanged( "Text" );
                }
            }
        }

//        private void raisePropertyChanged( String propertyName ) {
//            foreach ( IPropertyChangedListener listener in listeners ) {
//                listener.propertyChanged( propertyName );
//            }
//        }

//        private List<IPropertyChangedListener> listeners = new List< IPropertyChangedListener >();
        private string text;
//
//        public void addPropertyChangedListener( IPropertyChangedListener listener ) {
//            listeners.Add( listener );
//        }
//
//        public void removePropertyChangedListener( IPropertyChangedListener listener ) {
//            listeners.Remove( listener );
//        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void raisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if ( handler != null ) handler( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }

    class Program
    {
        static void Main(string[] args) {
            SourceClass source = new SourceClass(  );
            TargetClass target  = new TargetClass(  );
            BindingBase binding = new BindingBase( target, "Title", source, "Text", BindingMode.OneWay );
            binding.bind(  );
            source.Text = "Text!";
        }
    }
}
