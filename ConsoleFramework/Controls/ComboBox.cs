using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    public class ComboBox : Control
    {
        public ComboBox( ) {
            Focusable = true;
            AddHandler( GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotKeyboardFocus) );
            AddHandler( LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnLostKeyboardFocus) );
            AddHandler( MouseDownEvent, new MouseButtonEventHandler(OnMouseDown) );
        }

        private void OnMouseDown( object sender, MouseButtonEventArgs mouseButtonEventArgs ) {
            Window popup = new Window(  );
            popup.X = 0;
            popup.Y = 0;
            popup.Width = 10;
            popup.Height = 8;
            WindowsHost windowsHost = ( ( WindowsHost ) this.Parent.Parent.Parent );
            windowsHost.AddWindow( popup );
        }

        private void OnLostKeyboardFocus( object sender, KeyboardFocusChangedEventArgs args ) {
            Invalidate(  );
        }

        private void OnGotKeyboardFocus( object sender, KeyboardFocusChangedEventArgs args ) {
            Invalidate(  );
        }

        public List<String> Items { get; set; }

        protected override Size MeasureOverride(Size availableSize) {
            int w;
            if ( availableSize.Width == Int32.MaxValue )
                w = Int32.MaxValue - 1;
            else {
                w = availableSize.Width;
            }
            return new Size(w, 1);
        }

        public override void Render(RenderingBuffer buffer) {
            ushort attrs;
            if ( HasFocus ) {
                attrs = Color.Attr(Color.White, Color.DarkGreen);
            } else attrs = Color.Attr( Color.Black, Color.DarkCyan );

            for ( int i = 0; i < ActualWidth - 2; i++ ) {
                buffer.SetPixel( i, 0, ' ' , ( CHAR_ATTRIBUTES ) attrs );
            }
            if ( ActualWidth > 2 ) {
                buffer.SetPixel( ActualWidth - 2, 0, 'v', ( CHAR_ATTRIBUTES ) attrs );
            }
            if ( ActualWidth > 1 ) {
                buffer.SetPixel( ActualWidth - 1, 0, ' ', ( CHAR_ATTRIBUTES ) attrs );
            }
        }
    }
}
