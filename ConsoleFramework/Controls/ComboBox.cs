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
        private readonly bool shadow;

        public ComboBox( ) : this(true) {
        }

        /// <summary>
        /// Creates combobox instance.
        /// </summary>
        /// <param name="shadow">Display shadow or not</param>
        public ComboBox( bool shadow ) {
            this.shadow = shadow;
            Focusable = true;
            AddHandler( MouseDownEvent, new MouseButtonEventHandler(OnMouseDown) );
            AddHandler( KeyDownEvent, new KeyEventHandler(OnKeyDown) );
        }

        private class PopupWindow : Window
        {
            public int? IndexSelected;
            private readonly bool shadow;
            private readonly ListBox listbox;
            private readonly ScrollViewer scrollViewer;

            public PopupWindow( IEnumerable< string > items, 
                int selectedItemIndex, bool shadow,
                int? shownItemsCount)
            {
                this.shadow = shadow;
                scrollViewer = new ScrollViewer(  );
                listbox = new ListBox(  );
                foreach (string item in items) listbox.Items.Add(item);
//                listbox.Items.AddRange( items );
                listbox.SelectedItemIndex = selectedItemIndex;
                if ( shownItemsCount != null )
                    listbox.PageSize = shownItemsCount.Value;
                IndexSelected = selectedItemIndex;
                listbox.HorizontalAlignment = HorizontalAlignment.Stretch;
                scrollViewer.HorizontalScrollEnabled = false;
                scrollViewer.HorizontalAlignment = HorizontalAlignment.Stretch;
                scrollViewer.Content = listbox;
                Content = scrollViewer;

                // If click on the transparent header, close the popup
                AddHandler( MouseDownEvent, new MouseButtonEventHandler(( sender, args ) => {
                    if ( !scrollViewer.RenderSlotRect.Contains( args.GetPosition( this ) ) ) {
                        Close();
                        args.Handled = true;
                    }
                }));

                // If listbox item has been selected
                EventManager.AddHandler( listbox, MouseUpEvent, new MouseButtonEventHandler(
                    ( sender, args ) => {
                        IndexSelected = listbox.SelectedItemIndex;
                        Close();
                    }), true );
                EventManager.AddHandler(listbox, KeyDownEvent, new KeyEventHandler(
                    (sender, args) => {
                        if ( args.wVirtualKeyCode == VirtualKeys.Return ) {
                            IndexSelected = listbox.SelectedItemIndex;
                            Close( );
                        }
                    }), true);
                // todo : cleanup event handlers after popup closing
            }

            private void initListBoxScrollingPos( ) {
                int itemIndex = listbox.SelectedItemIndex ?? 0;
                int firstVisibleItemIndex = scrollViewer.DeltaY;
                int lastVisibleItemIndex = firstVisibleItemIndex + scrollViewer.ActualHeight -
                                            ( scrollViewer.HorizontalScrollVisible ? 1 : 0 ) - 1;
                if ( itemIndex > lastVisibleItemIndex ) {
                    scrollViewer.ScrollContent( ScrollViewer.Direction.Up, itemIndex - lastVisibleItemIndex );
                } else if ( itemIndex < firstVisibleItemIndex ) {
                    scrollViewer.ScrollContent( ScrollViewer.Direction.Down, firstVisibleItemIndex - itemIndex );
                }
            }

            protected override void initialize( ) {
                AddHandler(ActivatedEvent, new EventHandler(OnActivated));
                AddHandler( KeyDownEvent, new KeyEventHandler(OnKeyDown), true );
            }

            private void OnKeyDown( object sender, KeyEventArgs args ) {
                if (args.wVirtualKeyCode == VirtualKeys.Escape) {
                    Close();
                } else base.OnPreviewKeyDown( sender, args );
            }

            private void OnActivated( object sender, EventArgs eventArgs ) {
            }

            public override void Render(RenderingBuffer buffer)
            {
                Attr borderAttrs = Colors.Blend(Color.Black, Color.DarkCyan);

                // Background
                buffer.FillRectangle(1, 1, this.ActualWidth - 1, this.ActualHeight - 1, ' ', borderAttrs);

                // First row and first column are transparent
                // Column is also transparent for mouse events
                buffer.SetOpacityRect( 0,0,ActualWidth, 1, 2 );
                buffer.SetOpacityRect( 0, 1, 1, ActualHeight-1, 6 );
                if ( shadow ) {
                    buffer.SetOpacity( 1, ActualHeight - 1, 2+4 );
                    buffer.SetOpacity( ActualWidth - 1, 0, 2+4 );
                    buffer.SetOpacityRect( ActualWidth - 1, 1, 1, ActualHeight - 1, 1+4 );
                    buffer.FillRectangle(ActualWidth - 1, 1, 1, ActualHeight - 1, UnicodeTable.FullBlock, borderAttrs);
                    buffer.SetOpacityRect( 2, ActualHeight - 1, ActualWidth - 2, 1, 3+4 );
                    buffer.FillRectangle(2, ActualHeight - 1, ActualWidth - 2, 1, UnicodeTable.UpperHalfBlock,
                                          Attr.NO_ATTRIBUTES );
                }
            }

            protected override Size MeasureOverride(Size availableSize)
            {
                if (Content == null) return new Size(0, 0);
                if ( shadow ) {
                    // 1 row and 1 column - reserved for transparent space, remaining - for ListBox
                    Content.Measure( new Size( availableSize.Width - 2, availableSize.Height - 2 ) );
                    return new Size( Content.DesiredSize.Width + 2, Content.DesiredSize.Height + 2 );
                } else {
                    // 1 row and 1 column - reserved for transparent space, remaining - for ListBox
                    Content.Measure(new Size(availableSize.Width - 1, availableSize.Height - 1));
                    return new Size(Content.DesiredSize.Width + 1, Content.DesiredSize.Height + 1);
                }
            }

            protected override Size ArrangeOverride(Size finalSize) {
                if ( Content != null ) {
                    if ( shadow ) {
                        Content.Arrange( new Rect( new Point( 1, 1 ),
                                                   new Size( finalSize.Width - 2, finalSize.Height - 2 ) ) );
                    } else {
                        Content.Arrange(new Rect(new Point(1, 1),
                                                   new Size(finalSize.Width - 1, finalSize.Height - 1)));
                    }

                    // When initializing we need to correctly assign offsets to ScrollViewer for
                    // currently selected item. Because ScrollViewer depends of ActualWidth / ActualHeight
                    // of Content, we need to do this after arrangement has finished.
                    initListBoxScrollingPos( );
                }
                return finalSize;
            }

            public override string ToString() {
                return "ComboBox.PopupWindow";
            }
        }

        private bool opened {
            get {
                return m_opened;
            }
            set {
                m_opened = value;
                Invalidate(  );
            }
        }

        public int? ShownItemsCount { get; set; }

        private void openPopup( ) {
            if (opened) throw new InvalidOperationException("Assertion failed.");
            Window popup = new PopupWindow(Items, SelectedItemIndex ?? 0, shadow,
                ShownItemsCount != null ? ShownItemsCount.Value - 1 : ( int? ) null);
            Point popupCoord = TranslatePoint(this, new Point(0, 0),
                VisualTreeHelper.FindClosestParent<WindowsHost>( this ));
            popup.X = popupCoord.X;
            popup.Y = popupCoord.Y;
            popup.Width = shadow ? ActualWidth+1 : ActualWidth;
            if (Items.Count != 0)
                popup.Height = (ShownItemsCount != null ? ShownItemsCount.Value : Items.Count)
                    + (shadow ? 2 : 1); // 1 row for transparent "header"
            else popup.Height = shadow ? 3 : 2;
            WindowsHost windowsHost = VisualTreeHelper.FindClosestParent< WindowsHost >( this );
            windowsHost.ShowModal(popup, true);
            opened = true;
            EventManager.AddHandler(popup, Window.ClosedEvent, new EventHandler(OnPopupClosed));
        }

        private void OnKeyDown( object sender, KeyEventArgs args ) {
            if ( args.wVirtualKeyCode == VirtualKeys.Return ) {
                openPopup(  );
            }
        }

        private void OnMouseDown( object sender, MouseButtonEventArgs mouseButtonEventArgs ) {
            if ( !opened ) 
                openPopup(  );
        }

        private void OnPopupClosed( object o, EventArgs args ) {
            if (!opened) throw new InvalidOperationException("Assertion failed.");
            opened = false;
            this.SelectedItemIndex = ( ( PopupWindow ) o ).IndexSelected;
            EventManager.RemoveHandler(o, Window.ClosedEvent, new EventHandler(OnPopupClosed));
        }

        private readonly List<String> items = new List< string >();
        public List<String> Items {
            get { return items; }
        }


        public int? SelectedItemIndex {
            get { return selectedItemIndex; }
            set {
                if ( selectedItemIndex != value ) {
                    selectedItemIndex = value;
                    Invalidate(  );
                    RaisePropertyChanged( "SelectedItemIndex" );
                }
            }
        }

        private bool m_opened;
        private int? selectedItemIndex;

        public static Size EMPTY_SIZE = new Size(3, 1);

        protected override Size MeasureOverride(Size availableSize) {
            if (Items.Count == 0) return EMPTY_SIZE;
            int maxLen = Items.Max(s => s.Length);
            // 1 pixel from left, 1 from right, then arrow and 1 more empty pixel
            Size size = new Size(Math.Min(maxLen + 4, availableSize.Width), 1);
            return size;
        }

        public override void Render(RenderingBuffer buffer) {
            Attr attrs;
            if ( HasFocus ) {
                attrs = Colors.Blend(Color.White, Color.DarkGreen);
            } else attrs = Colors.Blend( Color.Black, Color.DarkCyan );

            buffer.SetPixel( 0, 0, ' ', attrs );
            int usedForCurrentItem = 0;
            if ( Items.Count != 0 && ActualWidth > 4 ) {
                usedForCurrentItem = RenderString(Items[SelectedItemIndex ?? 0], buffer, 1, 0, ActualWidth - 4, attrs);
            }
            buffer.FillRectangle( 1 + usedForCurrentItem, 0, ActualWidth - (usedForCurrentItem + 1), 1, ' ', attrs );
            if (ActualWidth > 2)
            {
                buffer.SetPixel(ActualWidth - 2, 0, opened ? '^' : 'v', attrs);
            }
        }
    }
}
