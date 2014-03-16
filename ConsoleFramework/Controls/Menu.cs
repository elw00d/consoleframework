using System;
using System.Collections.Generic;
using System.Linq;
using Binding.Observables;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;
using Xaml;

namespace ConsoleFramework.Controls
{
    public enum MenuItemType
    {
        Item,
        RootSubmenu,
        Submenu,
        Separator
    }

    public class MenuItemBase : Control
    {
        
    }

    /// <summary>
    /// todo : обработать случай, при котором меню открывается, потом закрывается (popup закеширован)
    /// а после этого набор MenuItems меняется, и нужно в popup их обновить
    /// </summary>
    [ContentProperty("Items")]
    public class MenuItem : MenuItemBase
    {
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MenuItem));

        /// <summary>
        /// Call this method if you have changed menu items set
        /// after menu popup has been shown.
        /// </summary>
        public void ReinitializePopup( ) {
            if (null != popup)
                popup.DisconnectMenuItems(  );
        }

        public event RoutedEventHandler Click {
            add {
                AddHandler(ClickEvent, value);
            }
            remove {
                RemoveHandler(ClickEvent, value);
            }
        }

        private bool _expanded;
        internal bool expanded {
            get { return _expanded; }
            private set {
                if ( _expanded != value ) {
                    _expanded = value;
                    Invalidate();
                }
            }
        }

        private bool disabled;
        public bool Disabled {
            get { return disabled; }
            set {
                if ( disabled != value ) {
                    disabled = value;
                    Focusable = !disabled;
                    Invalidate(  );
                }
            }
        }

        public MenuItem( ) {
            Focusable = true;

            AddHandler( MouseDownEvent, new MouseEventHandler(onMouseDown) );
            AddHandler( MouseMoveEvent, new MouseEventHandler( onMouseMove ) );
            AddHandler( MouseUpEvent, new MouseEventHandler(onMouseUp) );
            AddHandler( KeyDownEvent, new KeyEventHandler(onKeyDown));

            // Stretch by default
            HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        private void onKeyDown(object sender, KeyEventArgs args) {
            if (args.wVirtualKeyCode == VirtualKeys.Return) {
                if (Type == MenuItemType.RootSubmenu || Type == MenuItemType.Submenu)
                    openMenu();
                else if (Type == MenuItemType.Item)
                    RaiseEvent(ClickEvent, new RoutedEventArgs(this, ClickEvent));
                args.Handled = true;
            }
        }

        private void onMouseUp( object sender, MouseEventArgs args ) {
            if ( Type == MenuItemType.Item ) {
                RaiseEvent(ClickEvent, new RoutedEventArgs(this, ClickEvent));
                args.Handled = true;
            }
        }

        private void onMouseMove( object sender, MouseEventArgs args ) {
            // Mouse move opens the submenus only in root level
            if ( !disabled && args.LeftButton == MouseButtonState.Pressed /*&& Parent.Parent is Menu*/ ) {
                openMenu(  );
            }
            args.Handled = true;
        }

        private void onMouseDown( object sender, MouseEventArgs args ) {
            if (!disabled)
                openMenu(  );
            args.Handled = true;
        }

        private Popup popup;

        private void openMenu( ) {
            if ( expanded ) return;

            if ( this.Type == MenuItemType.Submenu || Type == MenuItemType.RootSubmenu ) {
                if (null == popup) {
                    popup = new Popup(this.Items, true, true, this.ActualWidth);
                    popup.AddHandler(Window.ClosedEvent, new EventHandler(onPopupClosed));
                }
                WindowsHost windowsHost = VisualTreeHelper.FindClosestParent<WindowsHost>(this);
                Point point = TranslatePoint(this, new Point(0, 0), windowsHost);
                popup.X = point.X;
                popup.Y = point.Y;
                windowsHost.ShowModal(popup, true);
                expanded = true;
            }
        }

        private void onPopupClosed( object sender, EventArgs eventArgs ) {
            assert( expanded );
            expanded = false;
            //popup = null;
        }

        public string Title { get; set; }

        private string titleRight;
        public string TitleRight {
            get {
                if ( titleRight == null && Type == MenuItemType.Submenu )
                    return "\u25ba"; // ► todo : extract constant
                return titleRight;
            }
            set { titleRight = value; }
        }

        public string Description { get; set; }

        public MenuItemType Type { get; set; }

        private List< MenuItemBase > items = new List< MenuItemBase >();
        
        public List<MenuItemBase> Items {
            get { return items; }
        }

        protected override Size MeasureOverride(Size availableSize) {
            int length = 2;
            if ( !string.IsNullOrEmpty( Title ) ) length += Title.Length;
            if ( !string.IsNullOrEmpty( TitleRight ) ) length += TitleRight.Length;
            if ( !string.IsNullOrEmpty( Title ) && !string.IsNullOrEmpty( TitleRight ) )
                length++;
            return new Size(length, 1);
        }

        public override void Render(RenderingBuffer buffer) {
            Attr captionAttrs;
            if (HasFocus || this.expanded)
                captionAttrs = Colors.Blend(Color.Black, Color.DarkGreen);
            else
                captionAttrs = Colors.Blend(Color.Black, Color.Gray);
            if ( disabled )
                captionAttrs = Colors.Blend( Color.DarkGray, Color.Gray );

            buffer.FillRectangle( 0, 0, ActualWidth, ActualHeight, ' ', captionAttrs );
            if (null != Title)
                RenderString( Title , buffer, 1, 0, ActualWidth, captionAttrs );
            if ( null != TitleRight )
                RenderString( TitleRight, buffer, ActualWidth - TitleRight.Length - 1, 0,
                              TitleRight.Length, captionAttrs );
        }

        internal class Popup : Window
        {
            private readonly bool shadow;
            private readonly bool border;
            private readonly int width; // Размер непрозрачной области
            private Panel panel;

            public static readonly RoutedEvent ControlKeyPressedEvent = EventManager.RegisterRoutedEvent("ControlKeyPressed",
                RoutingStrategy.Bubble, typeof(KeyEventHandler), typeof(MenuItem.Popup));

            /// <summary>
            /// Call this method to remove all menu items that are used as child items.
            /// It is necessary before reuse MenuItems in another Popup instance.
            /// </summary>
            public void DisconnectMenuItems( ) {
                panel.ClearChilds();
            }

            // todo : rename width
            /// <summary>
            /// Первая строчка всплывающего окна - особенная. Она прозрачна с точки зрения
            /// рендеринга полностью. Однако Opacity для событий мыши в ней разные.
            /// Первые width пикселей в ней - непрозрачные для событий мыши, но при клике на них
            /// окно закрывается вызовом Close(). Остальные ActualWidth - width пикселей - прозрачные
            /// для событий мыши, и нажатие мыши в этой области приводит к тому, что окно
            /// WindowsHost закрывает окно как окно с OutsideClickClosesWindow = True.
            /// </summary>
            public Popup( List<MenuItemBase> menuItems, bool shadow, bool border,
                int width) {
                this.width = width;
                this.shadow = shadow;
                this.border = border;
                panel = new Panel();
                panel.Orientation = Orientation.Vertical;
                foreach (MenuItemBase item in menuItems) {
                    panel.AddChild( item );
                }
                Content = panel;
                
                // If click on the transparent header, close the popup
                AddHandler( PreviewMouseDownEvent, new MouseButtonEventHandler(( sender, args ) => {
                    if ( Content != null && !Content.RenderSlotRect.Contains( args.GetPosition( this ) ) ) {
                        Close();
                        if ( new Rect( new Size( width, 1 ) ).Contains( args.GetPosition( this ) ) ) {
                            args.Handled = true;
                        }
                    }
                }));
                
                EventManager.AddHandler(panel, PreviewMouseMoveEvent, new MouseEventHandler(onPanelMouseMove));
            }

            protected override void OnPreviewKeyDown( object sender, KeyEventArgs args ) {
                switch ( args.wVirtualKeyCode ) {
                    case VirtualKeys.Right: {
                            KeyEventArgs newArgs = new KeyEventArgs( this, ControlKeyPressedEvent );
                            newArgs.wVirtualKeyCode = args.wVirtualKeyCode;
                            RaiseEvent(ControlKeyPressedEvent, newArgs);
                            args.Handled = true;
                            break;
                        }
                    case VirtualKeys.Left: {
                            KeyEventArgs newArgs = new KeyEventArgs(this, ControlKeyPressedEvent);
                            newArgs.wVirtualKeyCode = args.wVirtualKeyCode;
                            RaiseEvent(ControlKeyPressedEvent, newArgs);
                            args.Handled = true;
                            break;
                        }
                    case VirtualKeys.Down:
                        ConsoleApplication.Instance.FocusManager.MoveFocusNext();
                        args.Handled = true;
                        break;
                    case VirtualKeys.Up:
                        ConsoleApplication.Instance.FocusManager.MoveFocusPrev();
                        args.Handled = true;
                        break;
                    case VirtualKeys.Escape:
                        Close();
                        args.Handled = true;
                        break;
                }
            }

            private void onPanelMouseMove( object sender, MouseEventArgs e ) {
                if ( e.LeftButton == MouseButtonState.Pressed ) {
                    PassFocusToChildUnderPoint( e );
                }
            }

            protected override void initialize() {
                AddHandler(PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown), true);
            }

            public override void Render(RenderingBuffer buffer)
            {
                Attr borderAttrs = Colors.Blend(Color.Black, Color.Gray);
                // устанавливаем прозрачными первую строку и первый столбец
                // для столбца дополнительно включена прозрачность для событий мыши

                // background
                buffer.FillRectangle(0, 1, this.ActualWidth, this.ActualHeight - 1, ' ',
                    borderAttrs);

                // Первые width пикселей первой строки - прозрачные, но события мыши не пропускают
                // По нажатию на них мы закрываем всплывающее окно вручную
                buffer.SetOpacityRect(0, 0, width, 1, 2);
                // Оставшиеся пиксели первой строки - пропускают события мыши
                // И WindowsHost закроет всплывающее окно автоматически при нажатии или
                // перемещении нажатого курсора над этим местом
                buffer.SetOpacityRect( width, 0, ActualWidth - width, 1, 6 );

                //buffer.SetOpacityRect(0, 1, 1, ActualHeight - 1, 6);
                if (shadow)
                {
                    buffer.SetOpacity(0, ActualHeight - 1, 2 + 4);
                    buffer.SetOpacity(ActualWidth - 1, 1, 2 + 4);
                    buffer.SetOpacityRect(ActualWidth - 1, 2, 1, ActualHeight - 2, 1 + 4);
                    buffer.FillRectangle(ActualWidth - 1, 2, 1, ActualHeight - 2, '\u2588', borderAttrs);
                    buffer.SetOpacityRect(1, ActualHeight - 1, ActualWidth - 1, 1, 3 + 4);
                    buffer.FillRectangle(1, ActualHeight - 1, ActualWidth - 1, 1, '\u2580',
                                          Attr.NO_ATTRIBUTES);
                    //buffer.SetPixel( ActualWidth-1,ActualHeight-1, '\u2598' );
                }

                RenderBorders( buffer, new Point(1, 1), new Point(ActualWidth - 3, ActualHeight - 2),
                    true, borderAttrs);
            }

            protected override Size MeasureOverride(Size availableSize)
            {
                if (Content == null) return new Size(0, 0);
                if ( shadow ) {
                    // 1 строку и 1 столбец оставляем для прозрачного пространства, остальное занимает Content
                    Content.Measure( new Size( availableSize.Width - 3, availableSize.Height - 4 ) );
                    // +2 for left empty space and right
                    return new Size( Content.DesiredSize.Width + 3 + 2, Content.DesiredSize.Height + 4 );
                } else {
                    // 1 строку и 1 столбец оставляем для прозрачного пространства, остальное занимает Content
                    Content.Measure(new Size(availableSize.Width - 2, availableSize.Height - 3));
                    // +2 for left empty space and right
                    return new Size(Content.DesiredSize.Width + 2 + 2, Content.DesiredSize.Height + 3);
                }
            }

            protected override Size ArrangeOverride(Size finalSize) {
                if ( Content != null ) {
                    if ( shadow ) {
                        // 1 pixel from all borders - for popup padding
                        // 1 pixel from top - for transparent region
                        // Additional pixel from right and bottom - for shadow
                        Content.Arrange( new Rect( new Point( 2, 2 ),
                                                   new Size( finalSize.Width - 5, finalSize.Height - 4 ) ) );
                    } else {
                        // 1 pixel from all borders - for popup padding
                        // 1 pixel from top - for transparent region
                        Content.Arrange(new Rect(new Point(2, 2),
                                                   new Size(finalSize.Width - 4, finalSize.Height - 3)));
                    }
                }
                return finalSize;
            }
        }

        internal void Close( ) {
            assert( expanded );
            popup.Close(  );
        }

        internal void Expand( ) {
            openMenu(  );
        }
    }

    /// <summary>
    /// Cannot be added in root menu.
    /// </summary>
    public class Separator : MenuItemBase
    {
        public Separator( ) {
            Focusable = false;

            // Stretch by default
            HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        protected override Size MeasureOverride(Size availableSize) {
            return new Size(1, 1);
        }

        public override void Render(RenderingBuffer buffer) {
            Attr captionAttrs;
            if (HasFocus)
                captionAttrs = Colors.Blend(Color.Black, Color.DarkGreen);
            else
                captionAttrs = Colors.Blend(Color.Black, Color.Gray);

            buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, UnicodeTable.SingleFrameHorizontal, captionAttrs);
        }
    }

    public class Menu : Control
    {
        private readonly ObservableList<MenuItemBase> items = new ObservableList<MenuItemBase>(
            new List<MenuItemBase>());
        public IList< MenuItemBase > Items {
            get { return items; }
        }
        
        public Menu( ) {
            Panel stackPanel = new Panel( );
            stackPanel.Orientation = Orientation.Horizontal;
            this.AddChild( stackPanel );

            // Subscribe to Items change and add to Children them
            this.items.ListChanged += ( sender, args ) => {
                switch ( args.Type ) {
                    case ListChangedEventType.ItemsInserted: {
                        for ( int i = 0; i < args.Count; i++ ) {
                            MenuItemBase item = items[ args.Index + i ];
                            if (item is Separator)
                                throw new InvalidOperationException("Separator cannot be added to root menu.");
                            if (((MenuItem)item).Type == MenuItemType.Submenu)
                                ((MenuItem) item).Type = MenuItemType.RootSubmenu;
                            stackPanel.Content.Insert( args.Index + i, item );
                        }
                        break;
                    }
                    case ListChangedEventType.ItemsRemoved:
                        for (int i = 0; i < args.Count; i++)
                            stackPanel.Content.RemoveAt(args.Index);
                        break;
                    case ListChangedEventType.ItemReplaced: {
                        MenuItemBase item = items[ args.Index ];
                        if (item is Separator)
                            throw new InvalidOperationException("Separator cannot be added to root menu.");
                        if (((MenuItem)item).Type == MenuItemType.Submenu)
                            ((MenuItem)item).Type = MenuItemType.RootSubmenu;
                        stackPanel.Content[args.Index] = item;
                        break;
                    }
                }
            };
            this.IsFocusScope = true;

            this.AddHandler( KeyDownEvent, new KeyEventHandler(onKeyDown) );
            this.AddHandler( PreviewMouseMoveEvent, new MouseEventHandler(onPreviewMouseMove) );
            this.AddHandler( PreviewMouseDownEvent, new MouseEventHandler(onPreviewMouseDown) );
        }

        protected override void OnParentChanged( ) {
            if ( Parent != null ) {
                assert( Parent is WindowsHost );

                // Вешаем на WindowsHost обработчик события MenuItem.ClickEvent,
                // чтобы ловить момент выбора пункта меню в одном из модальных всплывающих окошек
                // Дело в том, что эти окошки не являются дочерними элементами контрола Menu,
                // а напрямую являются дочерними элементами WindowsHost (т.к. именно он создаёт
                // окна). И событие выбора пункта меню из всплывающего окошка может быть поймано 
                // в WindowsHost, но не в Menu. А нам нужно повесить обработчик, который закроет
                // все показанные попапы.
                EventManager.AddHandler( Parent, MenuItem.ClickEvent, new RoutedEventHandler( ( sender, args ) => {
                    List<MenuItem> expandedSubmenus = new List< MenuItem >();
                    MenuItem currentItem = ( MenuItem ) this.Items.SingleOrDefault(
                        item => item is MenuItem && ((MenuItem)item).expanded);
                    while ( null != currentItem ) {
                        expandedSubmenus.Add( currentItem );
                        currentItem = (MenuItem)currentItem.Items.SingleOrDefault(
                            item => item is MenuItem && ((MenuItem)item).expanded);
                    }
                    expandedSubmenus.Reverse( );
                    foreach ( MenuItem expandedSubmenu in expandedSubmenus ) {
                        expandedSubmenu.Close( );
                    }
                } ), true );

                EventManager.AddHandler( Parent, MenuItem.Popup.ControlKeyPressedEvent,
                    new KeyEventHandler(( sender, args ) => {
                        // todo : remove copy paste
                        List<MenuItem> expandedSubmenus = new List< MenuItem >();
                        MenuItem currentItem = ( MenuItem ) this.Items.SingleOrDefault(
                            item => item is MenuItem && ((MenuItem)item).expanded);
                        while ( null != currentItem ) {
                            expandedSubmenus.Add( currentItem );
                            currentItem = (MenuItem)currentItem.Items.SingleOrDefault(
                                item => item is MenuItem && ((MenuItem)item).expanded);
                        }
                        expandedSubmenus.Reverse( );
                        foreach ( MenuItem expandedSubmenu in expandedSubmenus ) {
                            expandedSubmenu.Close( );
                        }
                        //
                        ConsoleApplication.Instance.FocusManager.SetFocusScope( this );
                        if (args.wVirtualKeyCode == VirtualKeys.Right)
                            ConsoleApplication.Instance.FocusManager.MoveFocusNext(  );
                        else if (args.wVirtualKeyCode == VirtualKeys.Left)
                            ConsoleApplication.Instance.FocusManager.MoveFocusPrev();
                        MenuItem focusedItem = (MenuItem)this.Items.SingleOrDefault(
                            item => item is MenuItem && ((MenuItem)item).HasFocus);
                        focusedItem.Expand( );
                    }));
            }
        }

        private void onPreviewMouseMove( object sender, MouseEventArgs args ) {
            if ( args.LeftButton == MouseButtonState.Pressed ) {
                onPreviewMouseDown( sender, args );
            }
        }

        private void onPreviewMouseDown( object sender, MouseEventArgs e ) {
            PassFocusToChildUnderPoint( e );
        }

        private void onKeyDown( object sender, KeyEventArgs args ) {
            if ( args.wVirtualKeyCode == VirtualKeys.Right ) {
                ConsoleApplication.Instance.FocusManager.MoveFocusNext();
                args.Handled = true;
            }
            if ( args.wVirtualKeyCode == VirtualKeys.Left ) {
                ConsoleApplication.Instance.FocusManager.MoveFocusPrev();
                args.Handled = true;
            }
        }

        protected override Size MeasureOverride( Size availableSize ) {
            this.Children[0].Measure(availableSize);
            return this.Children[ 0 ].DesiredSize;
        }

        protected override Size ArrangeOverride( Size finalSize ) {
            this.Children[0].Arrange( new Rect(new Point(0, 0), finalSize) );
            return finalSize;
        }

        public override void Render( RenderingBuffer buffer ) {
            Attr attr = Colors.Blend( Color.Black, Color.Gray );
            buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, ' ', attr);
        }
    }
}
