using System;
using System.Collections.Generic;
using Binding.Observables;
using ConsoleFramework.Core;
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

        public void OpenMenu( WindowsHost windowsHost, Point point ) {
            if ( expanded ) return;

            if ( null == popup ) {
                popup = new MenuItem.Popup( this.Items, this.popupShadow, 0 );
                popup.AddHandler( Window.ClosedEvent, new EventHandler( onPopupClosed ) );
            }
            popup.X = point.X;
            popup.Y = point.Y;
            windowsHost.ShowModal( popup, true );
            expanded = true;
        }

        private void onPopupClosed( object sender, EventArgs eventArgs ) {
            if (!expanded) throw new InvalidOperationException("This shouldn't happen");
            expanded = false;
        }
    }
}