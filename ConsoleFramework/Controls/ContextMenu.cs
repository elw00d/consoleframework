using System;
using System.Collections.Generic;
using System.Linq;
using Binding.Observables;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using Xaml;

namespace ConsoleFramework.Controls
{
    [ContentProperty( "Items" )]
    public class ContextMenu
    {
        private readonly ObservableList< MenuItemBase > items = new ObservableList< MenuItemBase >(
            new List< MenuItemBase >( ) );

        public IList< MenuItemBase > Items {
            get { return items; }
        }

        private MenuItem.Popup popup;
        private bool expanded;

        private bool popupShadow = true;
        public bool PopupShadow {
            get { return popupShadow; }
            set { popupShadow = value; }
        }

        /// <summary>
        /// Forces all open submenus to be closed.
        /// </summary>
        public void CloseAllSubmenus( ) {
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
        }

        private WindowsHost windowsHost;
        private RoutedEventHandler windowsHostClick;
        private KeyEventHandler windowsHostControlKeyPressed;

        public void OpenMenu( WindowsHost windowsHost, Point point ) {
            if ( expanded ) return;

            // Вешаем на WindowsHost обработчик события MenuItem.ClickEvent,
            // чтобы ловить момент выбора пункта меню в одном из модальных всплывающих окошек
            // Дело в том, что эти окошки не являются дочерними элементами контрола Menu,
            // а напрямую являются дочерними элементами WindowsHost (т.к. именно он создаёт
            // окна). И событие выбора пункта меню из всплывающего окошка может быть поймано 
            // в WindowsHost, но не в Menu. А нам нужно повесить обработчик, который закроет
            // все показанные попапы.
            EventManager.AddHandler( windowsHost, MenuItem.ClickEvent,
                windowsHostClick = ( sender, args ) => {
                    CloseAllSubmenus( );
                    popup.Close(  );
                }, true );

            EventManager.AddHandler( windowsHost, MenuItem.Popup.ControlKeyPressedEvent,
                windowsHostControlKeyPressed = ( sender, args ) => {
                    CloseAllSubmenus( );
                    //
                    //ConsoleApplication.Instance.FocusManager.SetFocusScope(this);
                    if ( args.wVirtualKeyCode == VirtualKeys.Right )
                        ConsoleApplication.Instance.FocusManager.MoveFocusNext( );
                    else if ( args.wVirtualKeyCode == VirtualKeys.Left )
                        ConsoleApplication.Instance.FocusManager.MoveFocusPrev( );
                    MenuItem focusedItem = ( MenuItem ) this.Items.SingleOrDefault(
                        item => item is MenuItem && item.HasFocus );
                    focusedItem.Expand( );
                } );

            if ( null == popup ) {
                popup = new MenuItem.Popup( this.Items, this.popupShadow, 0 );
                popup.AddHandler( Window.ClosedEvent, new EventHandler( onPopupClosed ) );
            }
            popup.X = point.X;
            popup.Y = point.Y;
            windowsHost.ShowModal( popup, true );
            expanded = true;
            this.windowsHost = windowsHost;
        }

        private void onPopupClosed( object sender, EventArgs eventArgs ) {
            if (!expanded) throw new InvalidOperationException("This shouldn't happen");
            expanded = false;
            EventManager.RemoveHandler( windowsHost, MenuItem.ClickEvent, windowsHostClick );
            EventManager.RemoveHandler( windowsHost, MenuItem.Popup.ControlKeyPressedEvent,
                windowsHostControlKeyPressed );
        }
    }
}