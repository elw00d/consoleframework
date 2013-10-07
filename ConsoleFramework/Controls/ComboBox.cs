using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private class PopupWindow : Window
        {
            public PopupWindow( ) {
                AddChild( new TextBlock()
                    {
                        Text = "Item 1"
                    } );
                AddChild(new TextBlock()
                {
                    Text = "Item 2"
                });
                //Focusable = true;
            }

            protected override void initialize( ) {
                AddHandler(Window.ActivatedEvent, new EventHandler(OnActivated));
                AddHandler( Control.KeyDownEvent, new KeyEventHandler(OnKeyDown), true );
                AddHandler( MouseMoveEvent, new MouseEventHandler(( sender, args ) => {
                    //args.Handled = true;
                }) );
                AddHandler( MouseDownEvent, new MouseEventHandler(( sender, args ) => {
                    Point position = args.GetPosition( this );
                    if ( !new Rect( new Size( this.ActualWidth, this.ActualHeight ) ).Contains( position ) ) {
                        ConsoleApplication.Instance.EndCaptureInput(this);
                        getWindowsHost().RemoveWindow(this);
                    }
                } ) );
            }

            private new void OnKeyDown( object sender, KeyEventArgs args ) {
                if (args.wVirtualKeyCode == 0x1B) { // VK_ESCAPE
                    ConsoleApplication.Instance.EndCaptureInput( this );
                    getWindowsHost(  ).RemoveWindow( this );
                } else base.OnKeyDown( sender, args );
            }

            private void OnActivated( object sender, EventArgs eventArgs ) {
                ConsoleApplication.Instance.BeginCaptureInput(this);
            }

            public override void Render(RenderingBuffer buffer)
            {
                ushort borderAttrs = Color.Attr(Color.White, Color.Gray);
                // background
                buffer.FillRectangle(0, 0, this.ActualWidth, this.ActualHeight, ' ', borderAttrs);
            }

            public override string ToString() {
                return "ComboBox.PopupWindow";
            }
        }

        private void OnMouseDown( object sender, MouseButtonEventArgs mouseButtonEventArgs ) {
            Window popup = new PopupWindow();
            popup.X = 0;
            popup.Y = 0;
            popup.Width = 15;
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
