using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Binding.Observables;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;

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

    public class MenuItem : MenuItemBase
    {
        private bool expanded;

        public MenuItem( ) {
            Focusable = true;

            AddHandler( MouseDownEvent, new MouseEventHandler(onMouseDown), true );
            AddHandler( MouseMoveEvent, new MouseEventHandler(onMouseMove) );

            AddHandler(GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotKeyboardFocus));
            AddHandler(LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnLostKeyboardFocus));
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

            Window popup = new Window(  );
            WindowsHost windowsHost = VisualTreeHelper.FindClosestParent< WindowsHost >( this );
            popup.Title = "TTTTTTTTT";
            Point point = TranslatePoint( this, new Point( 0, 0 ), windowsHost );
            popup.X = point.X;
            popup.Y = point.Y + 1;
            windowsHost.ShowModal( popup, true );
            expanded = true;
            popup.AddHandler( Window.ClosedEvent, new EventHandler( onPopupClosed) );
        }

        private void onPopupClosed( object sender, EventArgs eventArgs ) {
            assert( expanded );
            expanded = false;
        }

        public string Title { get; set; }

        public string Description { get; set; }

        public MenuItemType Type { get; set; }

        public List< MenuItem > Items;

        protected override Size MeasureOverride(Size availableSize) {
            if (!string.IsNullOrEmpty(Title)) {
                Size minButtonSize = new Size(Title.Length, 1);
                return minButtonSize;
            } else return new Size(8, 1);
        }

        public override void Render(RenderingBuffer buffer) {
            Attr captionAttrs;
            if (HasFocus)
                captionAttrs = Colors.Blend(Color.Black, Color.DarkGreen);
            else
                captionAttrs = Colors.Blend(Color.Black, Color.DarkGray);

            if (null != Title)
                RenderString( Title, buffer, 0, 0, ActualWidth, captionAttrs );
        }
    }

    public class Separator : MenuItemBase
    {
        public Separator( ) {
            Focusable = false;
        }

        protected override Size MeasureOverride(Size availableSize) {
            return new Size(1, 1);
        }

        public override void Render(RenderingBuffer buffer) {
            Attr captionAttrs;
            if (HasFocus)
                captionAttrs = Colors.Blend(Color.Black, Color.DarkGreen);
            else
                captionAttrs = Colors.Blend(Color.Black, Color.DarkGray);

            buffer.FillRectangle( 0, 0, ActualWidth, ActualHeight, '-', captionAttrs );
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
                    case ListChangedEventType.ItemsInserted:
                        for (int i = 0; i < args.Count; i++)
                            stackPanel.Content.Insert(args.Index + i, items[args.Index + i]);
                        break;
                    case ListChangedEventType.ItemsRemoved:
                        for (int i = 0; i < args.Count; i++)
                            stackPanel.Content.RemoveAt(args.Index);
                        break;
                    case ListChangedEventType.ItemReplaced:
                        stackPanel.Content[args.Index] = items[args.Index];
                        break;
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
