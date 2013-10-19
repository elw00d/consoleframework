using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Список элементов с возможностью выбрать один из них.
    /// </summary>
    public class ListBox : Control
    {
        public readonly List< string > Items = new List< string >();
        private int selectedItemIndex;

        public event EventHandler SelectedItemIndexChanged;

        public int SelectedItemIndex {
            get { return selectedItemIndex; }
            set {
                if ( selectedItemIndex != value ) {
                    selectedItemIndex = value;
                    if (null != SelectedItemIndexChanged)
                        SelectedItemIndexChanged( this, EventArgs.Empty);
                }
            }
        }

        public ListBox( ) {
            Focusable = true;
            AddHandler( KeyDownEvent, new KeyEventHandler(OnKeyDown) );
            AddHandler( MouseDownEvent, new MouseButtonEventHandler(OnMouseDown) );
            AddHandler( MouseMoveEvent, new MouseEventHandler(OnMouseMove));
        }

        private void OnMouseMove( object sender, MouseEventArgs args ) {
            if ( args.LeftButton == MouseButtonState.Pressed ) {
                int index = args.GetPosition( this ).Y;
                if ( SelectedItemIndex != index ) {
                    SelectedItemIndex = index;
                    Invalidate(  );
                }
            }
            args.Handled = true;
        }

        private void OnMouseDown( object sender, MouseButtonEventArgs args ) {
            int index = args.GetPosition( this ).Y;
            if ( SelectedItemIndex != index ) {
                SelectedItemIndex = index;
                Invalidate(  );
            }
            args.Handled = true;
        }

        private void OnKeyDown( object sender, KeyEventArgs args ) {
            if ( Items.Count == 0 ) {
                args.Handled = true;
                return;
            }
            if (args.wVirtualKeyCode == 0x26) { // VK_UP
                if ( SelectedItemIndex == 0 )
                    SelectedItemIndex = Items.Count - 1;
                else {
                    SelectedItemIndex--;
                }
                Invalidate(  );
            }
            if (args.wVirtualKeyCode == 0x28) { // VK_DOWN
                SelectedItemIndex = (SelectedItemIndex + 1) % Items.Count;
                Invalidate(  );
            }
            args.Handled = true;
        }

        protected override Size MeasureOverride(Size availableSize) {
            // если maxLen < availableSize.Width, то возвращается maxLen
            // если maxLen > availableSize.Width, возвращаем availableSize.Width,
            // а содержимое не влезающих строк будет выведено с многоточием
            if (Items.Count == 0) return new Size(0, 0);
            int maxLen = Items.Max( s => s.Length );
            // 1 пиксель слева и 1 справа
            Size size = new Size(Math.Min( maxLen + 2, availableSize.Width ), Items.Count);
            return size;
        }

        public override void Render(RenderingBuffer buffer)
        {
            Attr selectedAttr = Colors.Blend(Color.White, Color.DarkGreen);
            Attr attr = Colors.Blend(Color.Black, Color.DarkCyan);
            for ( int y = 0; y < ActualHeight; y++ ) {
                string item = y < Items.Count ? Items[ y ] : null;

                if ( item != null ) {
                    Attr currentAttr = SelectedItemIndex == y ? selectedAttr : attr;

                    buffer.SetPixel( 0, y, ' ', currentAttr );
                    if ( ActualWidth > 1 ) {
                        // минус 2 потому что у нас есть по пустому пикселю слева и справа
                        int rendered = RenderString( item, buffer, 1, y, ActualWidth - 2, currentAttr );
                        buffer.FillRectangle( 1 + rendered, y, ActualWidth - (1 + rendered), 1, ' ',
                            currentAttr);
                    }
                } else {
                    buffer.FillRectangle(0, y, ActualWidth, 1, ' ', attr);
                }
            }
        }
    }
}
