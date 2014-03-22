using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Класс, служащий хост-панелью для набора перекрывающихся окон.
    /// Хранит в себе список окон в порядке их Z-Order и отрисовывает рамки,
    /// управляет их перемещением.
    /// </summary>
    public class WindowsHost : Control
    {
        private Menu mainMenu;
        public Menu MainMenu
        {
            get { return mainMenu; }
            set {
                if ( mainMenu != value ) {
                    if ( mainMenu != null ) {
                        RemoveChild( mainMenu );
                    }
                    if ( value != null ) {
                        InsertChildAt(0, value);
                    }
                    mainMenu = value;
                }
            }
        }

        public WindowsHost() {
            AddHandler(PreviewMouseDownEvent, new MouseButtonEventHandler(onPreviewMouseDown), true);
            AddHandler( PreviewMouseMoveEvent, new MouseEventHandler(onPreviewMouseMove), true );
            AddHandler(PreviewMouseUpEvent, new MouseEventHandler(onPreviewMouseUp), true);
            AddHandler( MouseMoveEvent, new MouseEventHandler(( sender, args ) => {
                //Debugger.Log( 1, "", "WindowHost.MouseMove\n" );
            }) );
            AddHandler( PreviewKeyDownEvent, new KeyEventHandler(onPreviewKeyDown) );
        }

        private void onPreviewKeyDown( object sender, KeyEventArgs args ) {
            if ( mainMenu != null ) {
                // todo : cache gestures map and provide method to refresh gestures
                Dictionary<KeyGesture, MenuItem> map = new Dictionary<KeyGesture, MenuItem>();
                foreach ( MenuItemBase itemBase in mainMenu.Items ) {
                    if ( itemBase is MenuItem ) {
                        getGestures( (MenuItem) itemBase, map );
                    }
                }

                KeyGesture match = map.Keys.FirstOrDefault( gesture => gesture.Matches( args ) );
                if ( match != null ) {
                    // todo : activate matches menu item
                    MenuItem menuItem = map[ match ];
                    List<MenuItem> path = new List< MenuItem >();
                    MenuItem currentItem = menuItem;
                    while ( currentItem != null ) {
                        path.Add( currentItem );
                        currentItem = currentItem.ParentItem;
                    }
                    path.Reverse( );

                    // todo : comment this
                    int i = 0;
                    Action action = null;
                    action = new Action(() => {
                        if (i < path.Count) {
                            MenuItem item = path[i];
                            item.Invalidate();
                            EventHandler handler = null;
                            handler = (o, eventArgs) => {
                                item.Expand();
                                item.LayoutRevalidated -= handler;
                                i++;
                                if (i < path.Count) {
                                    action();
                                }
                            };
                            item.LayoutRevalidated += handler;
                        }
                    });
                    action();
                    
                    args.Handled = true;
                }
            }
        }

        private void getGestures( MenuItem item, Dictionary< KeyGesture, MenuItem > map ) {
            if (item.Gesture != null)
                map.Add( item.Gesture, item );
            if ( item.Type == MenuItemType.RootSubmenu ||
                 item.Type == MenuItemType.Submenu ) {
                foreach ( MenuItemBase itemBase in item.Items ) {
                    if ( itemBase is MenuItem ) {
                        getGestures( ( MenuItem ) itemBase, map );
                    }
                }
            }
        }

        protected override Size MeasureOverride(Size availableSize) {
            int windowsStartIndex = 0;
            if ( mainMenu != null ) {
                assert( Children[ 0 ] == mainMenu );
                mainMenu.Measure( new Size(availableSize.Width, 1) );
                windowsStartIndex++;
            }

            // Дочерние окна могут занимать сколько угодно пространства,
            // но при заданных Width/Height их размеры будут учтены
            // системой размещения автоматически
            for ( int index = windowsStartIndex; index < Children.Count; index++ ) {
                Control control = Children[ index ];
                Window window = ( Window ) control;
                window.Measure( new Size( int.MaxValue, int.MaxValue ) );
            }
            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize) {
            int windowsStartIndex = 0;
            if ( mainMenu != null ) {
                assert( Children[ 0 ] == mainMenu );
                mainMenu.Arrange( new Rect(0, 0, finalSize.Width, 1) );
                windowsStartIndex++;
            }
            // сколько дочерние окна хотели - столько и получают
            for ( int index = windowsStartIndex; index < Children.Count; index++ ) {
                Control control = Children[ index ];
                Window window = ( Window ) control;
                int x;
                if ( window.X.HasValue ) {
                    x = window.X.Value;
                } else {
                    x = ( finalSize.Width - window.DesiredSize.Width )/2;
                }
                int y;
                if ( window.Y.HasValue ) {
                    y = window.Y.Value;
                } else {
                    y = ( finalSize.Height - window.DesiredSize.Height )/2;
                }
                window.Arrange( new Rect( x, y, window.DesiredSize.Width, window.DesiredSize.Height ) );
            }
            return finalSize;
        }

        public override void Render(RenderingBuffer buffer)
        {
            buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, ' ', Attr.BACKGROUND_BLUE);
        }

        /// <summary>
        /// Делает указанное окно активным. Если оно до этого не было активным, то
        /// по Z-индексу оно будет перемещено на самый верх, и получит клавиатурный фокус ввода.
        /// </summary>
        private void activateWindow(Window window) {
            //int index = Children.FindIndex(0, control => control == window);
            int index = -1;
            for ( int i = 0; i < Children.Count; i++ ) {
                Control child = Children[ i ];
                if ( child == window ) {
                    index = i;
                    break;
                }
            }
            if (-1 == index)
                throw new InvalidOperationException("Assertion failed.");
            //
            Control oldTopWindow = Children[Children.Count - 1];
            for (int i = index; i < Children.Count - 1; i++) {
                SwapChildsZOrder( i, i + 1 );
                //Children[i] = Children[i + 1];
            }
            //Children[Children.Count - 1] = window;
            
            // If need to change top window
            if (oldTopWindow != window)
            {
                oldTopWindow.RaiseEvent( Window.DeactivatedEvent, new RoutedEventArgs( oldTopWindow, Window.DeactivatedEvent ) );
                window.RaiseEvent(Window.ActivatedEvent, new RoutedEventArgs(window, Window.ActivatedEvent));
            }
            // If need to change focus (it is not only when need to change top window)
            // It may be need to change focus from menu to window, for example
            if ( ConsoleApplication.Instance.FocusManager.CurrentScope != window ) {
                initializeFocusOnActivatedWindow( window );
            }
        }
        
        private bool isTopWindowModal( ) {
            int windowsStartIndex = 0;
            if ( mainMenu != null ) {
                assert( Children[ 0 ] == mainMenu );
                windowsStartIndex++;
            }

            if ( Children.Count == windowsStartIndex ) return false;
            return windowInfos[ (Window) Children[ Children.Count - 1 ] ].Modal;
        }

        private void onPreviewMouseMove(object sender, MouseEventArgs args) {
            onPreviewMouseEvents(sender, args, 2);
        }

        private void onPreviewMouseDown(object sender, MouseEventArgs args) {
            onPreviewMouseEvents(sender, args, 0);
        }

        private void onPreviewMouseUp(object sender, MouseEventArgs args) {
            onPreviewMouseEvents(sender, args, 1);
        }

        /// <summary>
        /// Обработчик отвечает за вывод на передний план неактивных окон, на которые нажали мышкой,
        /// и за обработку мыши, когда имеется модальное окно - в этом случае обработчик не пропускает
        /// события, которые идут мимо модального окна, дальше по дереву (Tunneling) - устанавливая
        /// Handled в True, либо закрывает модальное окно, если оно было показано с флагом
        /// OutsideClickClosesWindow.
        /// eventType = 0 - PreviewMouseDown
        /// eventType = 1 - PreviewMouseUp
        /// eventType = 2 - PreviewMouseMove
        /// </summary>
        private void onPreviewMouseEvents(object sender, MouseEventArgs args, int eventType) {
            bool handle = false;
            check:
            if ( isTopWindowModal( ) ) {
                Window modalWindow = ( Window ) Children[ Children.Count - 1 ];
                Window windowClicked = VisualTreeHelper.FindClosestParent<Window>((Control)args.Source);
                if ( windowClicked != modalWindow ) {
                    if ( windowInfos[ modalWindow ].OutsideClickClosesWindow
                        && (eventType == 0 || eventType == 2 && args.LeftButton == MouseButtonState.Pressed) ) {
                        // закрываем текущее модальное окно
                        CloseWindow( modalWindow );

                        // далее обрабатываем событие как обычно
                        handle = true;

                        // Если дальше снова модальное окно, проверку нужно повторить, и закрыть
                        // его тоже, и так далее. Можно отрефакторить как вызов подпрограммы
                        // вида while (closeTopModalWindowIfNeed()) ;
                        goto check;
                    } else {
                        // прекращаем распространение события (правда, контролы, подписавшиеся с флагом
                        // handledEventsToo, получат его в любом случае) и генерацию соответствующего
                        // парного не-preview события
                        args.Handled = true;
                    }
                }
            } else {
                handle = true;
            }
            if (handle && (eventType == 0 || eventType == 2 && args.LeftButton == MouseButtonState.Pressed)) {
                Window windowClicked = VisualTreeHelper.FindClosestParent< Window >( ( Control ) args.Source );
                if ( null != windowClicked ) {
                    activateWindow( windowClicked );
                } else {
                    Menu menu = VisualTreeHelper.FindClosestParent< Menu >( ( Control ) args.Source );
                    if ( null != menu ) {
                        activateMenu(  );
                    }
                }
            }
        }

        private void activateMenu( ) {
            assert( mainMenu != null );
            if (ConsoleApplication.Instance.FocusManager.CurrentScope != mainMenu)
                ConsoleApplication.Instance.FocusManager.SetFocusScope( mainMenu );
        }

        private void initializeFocusOnActivatedWindow( Window window ) {
            ConsoleApplication.Instance.FocusManager.SetFocusScope(window);
            // todo : add window.ChildToFocus support again
//
//            bool reinitFocus = false;
//            if ( window.StoredFocus != null ) {
//                // проверяем, не удалён ли StoredFocus и является ли он Visible & Focusable
//                if ( !VisualTreeHelper.IsConnectedToRoot( window.StoredFocus ) ) {
//                    // todo : log warn about disconnected control
//                    reinitFocus = true;
//                } else if ( window.StoredFocus.Visibility != Visibility.Visible ) {
//                    // todo : log warn about invizible control to be focused
//                    reinitFocus = true;
//                }
//                else if ( !window.StoredFocus.Focusable ) {
//                    // todo : log warn
//                    reinitFocus = true;
//                } else {
//                    ConsoleApplication.Instance.FocusManager.SetFocus( window, window.StoredFocus );
//                }
//            } else {
//                reinitFocus = true;
//            }
//            //
//            if ( reinitFocus ) {
//                if ( window.ChildToFocus != null ) {
//                    Control child = VisualTreeHelper.FindChildByName( window, window.ChildToFocus );
//                    ConsoleApplication.Instance.FocusManager.SetFocus( child );
//                } else {
//                    ConsoleApplication.Instance.FocusManager.SetFocusScope( window );
//                }
//            }
        }

        private class WindowInfo
        {
            public readonly bool Modal;
            public readonly bool OutsideClickClosesWindow;

            public WindowInfo( bool modal, bool outsideClickClosesWindow ) {
                Modal = modal;
                OutsideClickClosesWindow = outsideClickClosesWindow;
            }
        }

        private readonly Dictionary<Window, WindowInfo> windowInfos = new Dictionary< Window, WindowInfo >();

        /// <summary>
        /// Adds window to window host children and shows it as modal window.
        /// </summary>
        public void ShowModal( Window window, bool outsideClickWillCloseWindow = false ) {
            showCore( window, true, outsideClickWillCloseWindow );
        }

        /// <summary>
        /// Adds window to window host children and shows it.
        /// </summary>
        public void Show(Window window) {
            showCore( window, false, false );
        }

        private Window getTopWindow( ) {
            int windowsStartIndex = 0;
            if ( mainMenu != null ) {
                assert( Children[ 0 ] == mainMenu );
                windowsStartIndex++;
            }
            if ( Children.Count > windowsStartIndex ) {
                return ( Window ) Children[ Children.Count - 1 ];
            }
            return null;
        }

        private void showCore( Window window, bool modal, bool outsideClickWillCloseWindow ) {
            Control topWindow = getTopWindow(  );
            if ( null != topWindow ) {
                topWindow.RaiseEvent( Window.DeactivatedEvent,
                                        new RoutedEventArgs( topWindow, Window.DeactivatedEvent ) );
            }

            AddChild(window);
            window.RaiseEvent( Window.ActivatedEvent, new RoutedEventArgs( window, Window.ActivatedEvent ) );
            initializeFocusOnActivatedWindow(window);
            windowInfos.Add( window, new WindowInfo( modal, outsideClickWillCloseWindow ) );
        }

        /// <summary>
        /// Removes window from window host.
        /// </summary>
        public void CloseWindow(Window window) {
            windowInfos.Remove( window );
            window.RaiseEvent( Window.DeactivatedEvent, new RoutedEventArgs( window, Window.DeactivatedEvent ) );
            RemoveChild(window);
            window.RaiseEvent( Window.ClosedEvent, new RoutedEventArgs( window, Window.ClosedEvent ) );
            // после удаления окна активизировать то, которое было активным до него
            IList<Control> childrenOrderedByZIndex = GetChildrenOrderedByZIndex();

            int windowsStartIndex = 0;
            if ( mainMenu != null ) {
                assert( Children[ 0 ] == mainMenu );
                windowsStartIndex++;
            }

            if ( childrenOrderedByZIndex.Count > windowsStartIndex ) {
                Window topWindow = ( Window ) childrenOrderedByZIndex[ childrenOrderedByZIndex.Count - 1 ];
                topWindow.RaiseEvent( Window.ActivatedEvent, new RoutedEventArgs( topWindow, Window.ActivatedEvent ) );
                initializeFocusOnActivatedWindow(topWindow);
                Invalidate();
            }
        }

        /// <summary>
        /// Утилитный метод, позволяющий для любого контрола определить ближайший WindowsHost,
        /// в котором он размещается.
        /// </summary>
        public static WindowsHost FindWindowsHostParent( Control control ) {
            Control tmp = control;
            while ( tmp != null && !( tmp is WindowsHost ) ) {
                tmp = tmp.Parent;
            }
            return ( WindowsHost ) ( tmp );
        }
    }
}
