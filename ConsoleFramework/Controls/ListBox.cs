using System;
using System.Collections.Generic;
using System.Linq;
using Binding.Observables;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Список элементов с возможностью выбрать один из них.
    /// todo : дополнить поддержку disabledItemsIndexes: при изменении их должно выполняться
    /// todo : Invalidate(), и они должны быть ненажимаемыми и невыбираемыми
    /// </summary>
    public class ListBox : Control
    {
        /// <summary>
        /// Количество элементов, которое пропускается при обработке нажатий PgUp и PgDown.
        /// По умолчанию null, и нажатие PgUp и PgDown эквивалентно нажатию Home и End.
        /// </summary>
        public int? PageSize { get; set; }

        private readonly ObservableList<string> items = new ObservableList<string>(new List<string>());
        public IList<String>  Items {
            get { return items; }
        }
        
        private int selectedItemIndex;

        public event EventHandler SelectedItemIndexChanged;

        private readonly List<int> disabledItemsIndexes = new List<int>(); 
        public List<int> DisabledItemsIndexes {
            get {
                return disabledItemsIndexes;
            }
        } 

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
            AddHandler( MouseWheelEvent, new MouseWheelEventHandler(onMouseWheel) );
            this.items.ListChanged += (sender, args) => {
                // Shift indexes of disabled items
                switch (args.Type) {
                    case ListChangedEventType.ItemsInserted: {
                        List<int> disabledIndexesCopy = new List<int>(DisabledItemsIndexes);
                        foreach (int index in disabledIndexesCopy) {
                            if (index >= args.Index) {
                                this.DisabledItemsIndexes.Remove(index);
                                this.DisabledItemsIndexes.Add(index + args.Count);
                            }
                        }
                        break;
                    }
                    case ListChangedEventType.ItemsRemoved: {
                        List<int> disabledIndexesCopy = new List<int>(DisabledItemsIndexes);
                        foreach (int index in disabledIndexesCopy) {
                            if (index >= args.Index) {
                                this.DisabledItemsIndexes.Remove(index);
                                this.DisabledItemsIndexes.Add(index - args.Count);
                            }
                        }
                        break;
                    }
                    case ListChangedEventType.ItemReplaced: {
                        // Nothing to do
                        break;
                    }
                    default:
                        throw new NotSupportedException();
                }
                Invalidate();
            };
        }

        private void onMouseWheel( object sender, MouseWheelEventArgs args ) {
            if ( args.Delta > 0) {
                pageUpCore( 2 );
            } else {
                pageDownCore( 2 );
            }
            args.Handled = true;
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

        private void pageUpCore( int? pageSize ) {
            if ( pageSize == null ) {
                if ( SelectedItemIndex != 0 ) {
                    SelectedItemIndex = 0;
                    Invalidate( );
                }
            } else {
                if ( SelectedItemIndex != 0 ) {
                    SelectedItemIndex = Math.Max(0, SelectedItemIndex - pageSize.Value);
                    Invalidate(  );
                }
            }
        }

        private void pageDownCore( int? pageSize ) {
            if ( pageSize == null ) {
                if ( SelectedItemIndex != items.Count - 1 ) {
                    SelectedItemIndex = items.Count - 1;
                    Invalidate( );
                }
            } else {
                if ( SelectedItemIndex != items.Count - 1 ) {
                    SelectedItemIndex = Math.Min(items.Count - 1, SelectedItemIndex + pageSize.Value);
                    Invalidate(  );
                }
            }
        }

        private void OnKeyDown( object sender, KeyEventArgs args ) {
            if ( items.Count == 0 ) {
                args.Handled = true;
                return;
            }
            if ( args.wVirtualKeyCode == VirtualKeys.PageUp ) {
                pageUpCore( PageSize );
            }
            if ( args.wVirtualKeyCode == VirtualKeys.PageDown ) {
                pageDownCore( PageSize );
            }
            if (args.wVirtualKeyCode == VirtualKeys.Up) {
                if ( SelectedItemIndex == 0 )
                    SelectedItemIndex = items.Count - 1;
                else {
                    SelectedItemIndex--;
                }
                Invalidate(  );
            }
            if (args.wVirtualKeyCode == VirtualKeys.Down) {
                SelectedItemIndex = (SelectedItemIndex + 1) % items.Count;
                Invalidate(  );
            }
            args.Handled = true;
        }

        protected override Size MeasureOverride(Size availableSize) {
            // если maxLen < availableSize.Width, то возвращается maxLen
            // если maxLen > availableSize.Width, возвращаем availableSize.Width,
            // а содержимое не влезающих строк будет выведено с многоточием
            if (items.Count == 0) return new Size(0, 0);
            int maxLen = items.Max( s => s.Length );
            // 1 пиксель слева и 1 справа
            Size size = new Size(Math.Min( maxLen + 2, availableSize.Width ), items.Count);
            return size;
        }

        public override void Render(RenderingBuffer buffer)
        {
            Attr selectedAttr = Colors.Blend(Color.White, Color.DarkGreen);
            Attr attr = Colors.Blend(Color.Black, Color.DarkCyan);
            Attr disabledAttr = Colors.Blend(Color.Gray, Color.DarkCyan);
            for ( int y = 0; y < ActualHeight; y++ ) {
                string item = y < items.Count ? items[ y ] : null;

                if ( item != null ) {
                    Attr currentAttr = disabledItemsIndexes.Contains(y) ? disabledAttr :
                        (SelectedItemIndex == y ? selectedAttr : attr);

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
