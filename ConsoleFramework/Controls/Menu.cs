using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        Submenu,
        Separator
    }

    public class MenuItemBase : Control
    {
        
    }

    [ContentProperty("Items")]
    public class MenuItem : MenuItemBase
    {
        private bool _expanded;
        private bool expanded {
            get { return _expanded; }
            set {
                if ( _expanded != value ) {
                    _expanded = value;
                    Invalidate();
                }
            }
        }

        public MenuItem( ) {
            Focusable = true;

            AddHandler( MouseDownEvent, new MouseEventHandler(onMouseDown) );
            AddHandler( MouseMoveEvent, new MouseEventHandler(onMouseMove) );

            AddHandler(GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotKeyboardFocus));
            AddHandler(LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnLostKeyboardFocus));

            // Stretch by default
            HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        // todo : make Focusable controls to be invalidated automatically
        private void OnLostKeyboardFocus( object sender, KeyboardFocusChangedEventArgs args ) {
            Invalidate(  );
        }

        private void OnGotKeyboardFocus( object sender, KeyboardFocusChangedEventArgs args ) {
            Invalidate(  );
        }

        private void onMouseMove( object sender, MouseEventArgs args ) {
            if ( args.LeftButton == MouseButtonState.Pressed ) {
                openMenu(  );
            }
        }

        private void onMouseDown( object sender, MouseEventArgs args ) {
            openMenu(  );
        }

        private void openMenu( ) {
            if ( expanded ) return;

            if ( this.Type == MenuItemType.Submenu ) {
                Popup popup = new Popup( this.Items, true, true );
                WindowsHost windowsHost = VisualTreeHelper.FindClosestParent< WindowsHost >( this );
                popup.Title = "TTTTTTTTT";
                Point point = TranslatePoint( this, new Point( 0, 0 ), windowsHost );
                popup.X = point.X;
                popup.Y = point.Y;
                windowsHost.ShowModal( popup, true );
                expanded = true;
                popup.AddHandler( Window.ClosedEvent, new EventHandler( onPopupClosed ) );
            } else {
                //todo
            }
        }

        private void onPopupClosed( object sender, EventArgs eventArgs ) {
            assert( expanded );
            expanded = false;
        }

        public string Title { get; set; }

        public string Description { get; set; }

        public MenuItemType Type { get; set; }

        private List< MenuItemBase > items = new List< MenuItemBase >();
        public List<MenuItemBase> Items {
            get { return items; }
        }

        protected override Size MeasureOverride(Size availableSize) {
            if (!string.IsNullOrEmpty(Title)) {
                Size minButtonSize = new Size(Title.Length + 2, 1);
                return minButtonSize;
            } else return new Size(8, 1);
        }

        public override void Render(RenderingBuffer buffer) {
            Attr captionAttrs;
            if (HasFocus || this.expanded)
                captionAttrs = Colors.Blend(Color.Black, Color.DarkGreen);
            else
                captionAttrs = Colors.Blend(Color.Black, Color.Gray);

            buffer.FillRectangle( 0, 0, ActualWidth, ActualHeight, ' ', captionAttrs );
            if (null != Title)
                RenderString( " " + Title + " ", buffer, 0, 0, ActualWidth, captionAttrs );
        }

        private class Popup : Window
        {
            private readonly bool shadow;
            private readonly bool border;

            public Popup( List<MenuItemBase> menuItems, bool shadow, bool border )
            {
                this.shadow = shadow;
                this.border = border;
                Panel panel = new Panel();
                panel.Orientation = Orientation.Vertical;
                foreach (MenuItemBase item in menuItems) {
                    panel.AddChild( item );
                }
                //panel.Margin = new Thickness(1);
                Content = panel;
                
                // if click on the transparent header, close the popup
                AddHandler( MouseDownEvent, new MouseButtonEventHandler(( sender, args ) => {
                    if ( Content != null && !Content.RenderSlotRect.Contains( args.GetPosition( this ) ) ) {
                        Close();
                        args.Handled = true;
                    }
                }));

                // todo : cleanup event handlers after popup closing
                AddHandler( ClosedEvent, new EventHandler(( sender, args ) => {
                    panel.ClearChilds(  );
                }) );
            }

            protected override void initialize()
            {
                AddHandler(KeyDownEvent, new KeyEventHandler(OnKeyDown), true);
            }

            private new void OnKeyDown(object sender, KeyEventArgs args)
            {
                if (args.wVirtualKeyCode == 0x1B)
                { // VK_ESCAPE
                    Close();
                }
                else base.OnKeyDown(sender, args);
            }

            public override void Render(RenderingBuffer buffer)
            {
                Attr borderAttrs = Colors.Blend(Color.Black, Color.Gray);
                // устанавливаем прозрачными первую строку и первый столбец
                // для столбца дополнительно включена прозрачность для событий мыши

                // background
                buffer.FillRectangle(0, 1, this.ActualWidth, this.ActualHeight - 1, ' ',
                    borderAttrs);

                buffer.SetOpacityRect(0, 0, ActualWidth, 1, 2);
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

        // todo : to BindingBase
//        public static void ApplyChanges<T>(IList<T> destList, ObservableList<T> srcList, ListChangedEventArgs args) {
//            switch (args.Type) {
//                case ListChangedEventType.ItemsInserted: {
//                        for (int i = 0; i < args.Count; i++) {
//                            MenuItemBase item = items[args.Index + i];
//                            if (item is Separator)
//                                throw new InvalidOperationException("Separator cannot be added to root menu.");
//                            stackPanel.Content.Insert(args.Index + i, item);
//                        }
//                        break;
//                    }
//                case ListChangedEventType.ItemsRemoved:
//                    for (int i = 0; i < args.Count; i++)
//                        stackPanel.Content.RemoveAt(args.Index);
//                    break;
//                case ListChangedEventType.ItemReplaced: {
//                        MenuItemBase item = items[args.Index];
//                        if (item is Separator)
//                            throw new InvalidOperationException("Separator cannot be added to root menu.");
//                        stackPanel.Content[args.Index] = item;
//                        break;
//                    }
//            }
//        }

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
                            stackPanel.Content.Insert( args.Index + i, item );
                        }
                        break;
                    }
                    case ListChangedEventType.ItemsRemoved: // todo : test
                        for (int i = 0; i < args.Count; i++)
                            stackPanel.Content.RemoveAt(args.Index);
                        break;
                    case ListChangedEventType.ItemReplaced: { // todo : test
                        MenuItemBase item = items[ args.Index ];
                        if (item is Separator)
                            throw new InvalidOperationException("Separator cannot be added to root menu.");

                        stackPanel.Content[args.Index] = item;
                        break;
                    }
                }
            };
            this.IsFocusScope = true;

            this.AddHandler( KeyDownEvent, new KeyEventHandler(onKeyDown) );
            this.AddHandler( PreviewMouseDownEvent, new MouseEventHandler(onPreviewMouseDown) );
        }

        // todo: remove copy-paste from Window
        private void onPreviewMouseDown( object sender, MouseEventArgs e ) {
            Control tofocus = null;
            Control parent = this;
            Control hitTested = null;
            do
            {
                Point position = e.GetPosition(parent);
                hitTested = parent.GetTopChildAtPoint(position);
                if (null != hitTested)
                {
                    parent = hitTested;
                    if (hitTested.Visibility == Visibility.Visible && hitTested.Focusable)
                    {
                        tofocus = hitTested;
                    }
                }
            } while (hitTested != null);
            if (tofocus != null)
            {
                ConsoleApplication.Instance.FocusManager.SetFocus(this, tofocus);
            }
        }

        private void onKeyDown( object sender, KeyEventArgs args ) {
            if ( args.wVirtualKeyCode == 0x27 ) // VK_RIGHT
            {
                ConsoleApplication.Instance.FocusManager.MoveFocusNext();
                args.Handled = true;
            }
            if ( args.wVirtualKeyCode == 0x25 ) // VK_LEFT
            {
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
            buffer.FillRectangle( 0, 0, ActualWidth, ActualHeight, '-', Attr.FOREGROUND_GREEN );
        }
    }
}
