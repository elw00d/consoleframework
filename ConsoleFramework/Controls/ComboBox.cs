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
        /// Создаёт экземпляр комбобокса
        /// </summary>
        /// <param name="shadow">Отображать ли тень</param>
        public ComboBox( bool shadow ) {
            this.shadow = shadow;
            Focusable = true;
            AddHandler( GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotKeyboardFocus) );
            AddHandler( LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnLostKeyboardFocus) );
            AddHandler( MouseDownEvent, new MouseButtonEventHandler(OnMouseDown) );
            AddHandler( KeyDownEvent, new KeyEventHandler(OnKeyDown) );
        }

        private class PopupWindow : Window
        {
            public int IndexSelected;
            private bool shadow;
            private ListBox listbox;
            private ScrollViewer scrollViewer;

            public PopupWindow( IEnumerable< string > items, 
                int selectedItemIndex, bool shadow,
                int? shownItemsCount)
            {
                this.shadow = shadow;
                scrollViewer = new ScrollViewer(  );
                listbox = new ListBox(  );
                listbox.Items.AddRange( items );
                listbox.SelectedItemIndex = selectedItemIndex;
                if ( shownItemsCount != null )
                    listbox.PageSize = shownItemsCount.Value;
                IndexSelected = selectedItemIndex;
                listbox.HorizontalAlignment = HorizontalAlignment.Stretch;
                scrollViewer.HorizontalScrollEnabled = false;
                scrollViewer.HorizontalAlignment = HorizontalAlignment.Stretch;
                scrollViewer.Content = listbox;
                Content = scrollViewer;

                // todo : продумать более удобное API для взаимодействия с ScrollViewer
                // todo : вынести этот код в ScrollableListBox
                listbox.SelectedItemIndexChanged += ListboxOnSelectedItemIndexChanged;

                // if click on the transparent header, close the popup
                AddHandler( MouseDownEvent, new MouseButtonEventHandler(( sender, args ) => {
                    if ( !listbox.RenderSlotRect.Contains( args.GetPosition( this ) ) ) {
                        Close();
                        args.Handled = true;
                    }
                }));

                // if listbox item has been selected
                EventManager.AddHandler( listbox, MouseUpEvent, new MouseButtonEventHandler(
                    ( sender, args ) => {
                        IndexSelected = listbox.SelectedItemIndex;
                        Close();
                    }), true );
                EventManager.AddHandler(listbox, KeyDownEvent, new KeyEventHandler(
                    (sender, args) => {
                        if ( args.wVirtualKeyCode == 0x0D ) { // VK_RETURN
                            IndexSelected = listbox.SelectedItemIndex;
                            Close( );
                        }
                    }), true);
                // todo : cleanup event handlers after popup closing
            }

            private void ListboxOnSelectedItemIndexChanged( object sender, EventArgs e ) {
                int itemIndex = listbox.SelectedItemIndex;
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

            private new void OnKeyDown( object sender, KeyEventArgs args ) {
                if (args.wVirtualKeyCode == 0x1B) { // VK_ESCAPE
                    Close();
                } else base.OnKeyDown( sender, args );
            }

            private void OnActivated( object sender, EventArgs eventArgs ) {
            }

            public override void Render(RenderingBuffer buffer)
            {
                Attr borderAttrs = Colors.Blend(Color.Black, Color.DarkCyan);
                // устанавливаем прозрачными первую строку и первый столбец
                // для столбца дополнительно включена прозрачность для событий мыши

                // background
                buffer.FillRectangle(1, 1, this.ActualWidth - 1, this.ActualHeight - 1, ' ', borderAttrs);

                buffer.SetOpacityRect( 0,0,ActualWidth, 1, 2 );
                buffer.SetOpacityRect( 0, 1, 1, ActualHeight-1, 6 );
                if ( shadow ) {
                    buffer.SetOpacity( 1, ActualHeight - 1, 6 );
                    buffer.SetOpacity( ActualWidth - 1, 0, 2 );
                    buffer.SetOpacityRect( ActualWidth - 1, 1, 1, ActualHeight - 1, 1 );
                    buffer.FillRectangle( ActualWidth - 1, 1, 1, ActualHeight - 1, '\u2588', borderAttrs );
                    buffer.SetOpacityRect( 2, ActualHeight - 1, ActualWidth - 2, 1, 3 );
                    buffer.FillRectangle( 2, ActualHeight - 1, ActualWidth - 2, 1, '\u2580',
                                          Attr.NO_ATTRIBUTES );
                    //buffer.SetPixel( ActualWidth-1,ActualHeight-1, '\u2598' );
                }
            }

            protected override Size MeasureOverride(Size availableSize)
            {
                if (Content == null) return new Size(0, 0);
                if ( shadow ) {
                    // 1 строку и 1 столбец оставляем для прозрачного пространства, остальное занимает ListBox
                    Content.Measure( new Size( availableSize.Width - 2, availableSize.Height - 2 ) );
                    return new Size( Content.DesiredSize.Width + 2, Content.DesiredSize.Height + 2 );
                } else {
                    // 1 строку и 1 столбец оставляем для прозрачного пространства, остальное занимает ListBox
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
                    // При инициализации нужно установить корректные смещения scroll viewer для текущего
                    // выбранного элемента. Но так как работа scroll viewer зависит от установленных
                    // значений ActualWidth / ActualHeight, то это нужно выполнить после выполнения
                    // этапа размещения (arrangement)
                    ListboxOnSelectedItemIndexChanged(this, EventArgs.Empty);
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
            Window popup = new PopupWindow(Items, SelectedItemIndex, shadow,
                ShownItemsCount != null ? ShownItemsCount.Value - 1 : ( int? ) null);
            Point popupCoord = TranslatePoint(this, new Point(0, 0),
                WindowsHost.FindWindowsHostParent(this));
            popup.X = popupCoord.X;
            popup.Y = popupCoord.Y;
            popup.Width = shadow ? ActualWidth+1 : ActualWidth;
            if (Items.Count != 0)
                popup.Height = (ShownItemsCount != null ? ShownItemsCount.Value : Items.Count)
                    + (shadow ? 2 : 1); // 1 строка для прозначного "заголовка"
            else popup.Height = shadow ? 3 : 2;
            WindowsHost windowsHost = VisualTreeHelper.FindClosestParent< WindowsHost >( this );
            windowsHost.ShowModal(popup, true);
            opened = true;
            EventManager.AddHandler(popup, Window.ClosedEvent, new EventHandler(OnPopupClosed));
        }

        private void OnKeyDown( object sender, KeyEventArgs args ) {
            if ( args.wVirtualKeyCode == 0x0D ) { // VK_RETURN
                openPopup(  );
            }
        }

        private void OnMouseDown( object sender, MouseButtonEventArgs mouseButtonEventArgs ) {
            openPopup(  );
        }

        private void OnPopupClosed( object o, EventArgs args ) {
            if (!opened) throw new InvalidOperationException("Assertion failed.");
            opened = false;
            this.SelectedItemIndex = ( ( PopupWindow ) o ).IndexSelected;
            EventManager.RemoveHandler(o, Window.ClosedEvent, new EventHandler(OnPopupClosed));
        }

        private void OnLostKeyboardFocus( object sender, KeyboardFocusChangedEventArgs args ) {
            Invalidate(  );
        }

        private void OnGotKeyboardFocus( object sender, KeyboardFocusChangedEventArgs args ) {
            Invalidate(  );
        }

        private readonly List<String> items = new List< string >();
        public List<String> Items {
            get { return items; }
        }


        public int SelectedItemIndex {
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
        private int selectedItemIndex;

        public static Size EMPTY_SIZE = new Size(3, 1);

        protected override Size MeasureOverride(Size availableSize) {
            if (Items.Count == 0) return EMPTY_SIZE;
            int maxLen = Items.Max(s => s.Length);
            // 1 пиксель слева от надписи, 1 справа, потом стрелка и ещё 1 пустой пиксель
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
                usedForCurrentItem = RenderString(Items[SelectedItemIndex], buffer, 1, 0, ActualWidth - 4, attrs);
            }
            buffer.FillRectangle( 1 + usedForCurrentItem, 0, ActualWidth - (usedForCurrentItem + 1), 1, ' ', attrs );
            if (ActualWidth > 2)
            {
                buffer.SetPixel(ActualWidth - 2, 0, opened ? '^' : 'v', attrs);
            }
        }
    }
}
