using System;
using System.Collections.Generic;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// В свёрнутом состоянии представляет собой однострочный контрол. При разворачивании списка
    /// создаётся всплывающее модальное кастомное окошко и показывается пользователю, причём первая
    /// строчка этого окна - прозрачная и через неё видно сам комбобокс (это нужно для того, чтобы
    /// обрабатывать клики по комбобоксу - при клике на прозрачную область комбобокс должен сворачиваться).
    /// Если этого бы не было, то с учётом того, что модальное окно показывается с флагом
    /// outsideClickWillCloseWindow = true, клик по самому комбобоксу приводил бы к мгновенному закрытию
    /// и открытию комбобокса заново.
    /// </summary>
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
                AddHandler(ActivatedEvent, new EventHandler(OnActivated));
                AddHandler( KeyDownEvent, new KeyEventHandler(OnKeyDown), true );
            }

            private new void OnKeyDown( object sender, KeyEventArgs args ) {
                if (args.wVirtualKeyCode == 0x1B) { // VK_ESCAPE
                    Close();
                } else base.OnKeyDown( sender, args );
            }

            private void OnActivated( object sender, EventArgs eventArgs ) {
            }

            public override void Render(RenderingBuffer buffer)
            {
                ushort borderAttrs = Color.Attr(Color.White, Color.Gray);
                // устанавливаем прозрачными первую строку и первый столбец
                // для столбца дополнительно включена прозрачность для событий мыши
                buffer.SetOpacityRect( 0,0,ActualWidth, 1, 2 );
                buffer.SetOpacityRect( 0, 0, 1, ActualHeight, 6 );
                // background
                buffer.FillRectangle(1, 1, this.ActualWidth-1, this.ActualHeight-1, ' ', borderAttrs);
            }

            public override string ToString() {
                return "ComboBox.PopupWindow";
            }
        }

        private void OnMouseDown( object sender, MouseButtonEventArgs mouseButtonEventArgs ) {
            Window popup = new PopupWindow();
            Point popupCoord = TranslatePoint( this, new Point( 0, 0 ),
                WindowsHost.FindWindowsHostParent(this));
            popup.X = popupCoord.X;
            popup.Y = popupCoord.Y;
            popup.Width = ActualWidth;
            popup.Height = 8; // todo:
            WindowsHost windowsHost = ( ( WindowsHost ) this.Parent.Parent.Parent );
            windowsHost.ShowModal( popup, true );
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
