using System;
using System.Collections.Generic;
using System.Diagnostics;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Класс, служащий хост-панелью для набора перекрывающихся окон.
    /// Хранит в себе список окон в порядке их Z-Order и отрисовывает рамки,
    /// управляет их перемещением.
    /// </summary>
    public class WindowsHost : Control
    {
        public WindowsHost() {
            AddHandler(PreviewMouseDownEvent, new MouseButtonEventHandler(WindowsHost_MouseDownPreview), true);
            AddHandler( MouseMoveEvent, new MouseEventHandler(( sender, args ) => {
                //Debugger.Log( 1, "", "WindowHost.MouseMove\n" );
            }) );
        }

        public static readonly Size MaxWindowSize = new Size(500, 500);

        protected override Size MeasureOverride(Size availableSize)
        {
            // дочерние окна могут занимать сколько угодно пространства,
            // но не более того, что предусмотрено константой MaxWindowSize
            foreach (Control control in Children)
            {
                Window window = (Window) control;
                window.Measure(MaxWindowSize);
            }
            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // сколько дочерние окна хотели - столько и получают
            foreach (Control control in Children)
            {
                Window window = (Window) control;
                window.Arrange(new Rect(window.X, window.Y, window.DesiredSize.Width, window.DesiredSize.Height));
            }
            return finalSize;
        }

        public override void Render(RenderingBuffer buffer)
        {
            buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, ' ', CHAR_ATTRIBUTES.BACKGROUND_BLUE);
        }

        /// <summary>
        /// Делает указанное окно активным. Если оно до этого не было активным, то
        /// по Z-индексу оно будет перемещено на самый верх, и получит клавиатурный фокус ввода.
        /// </summary>
        private void activateWindow(Window window) {
            int index = Children.FindIndex(0, control => control == window);
            if (-1 == index)
                throw new InvalidOperationException("Assertion failed.");
            //
            Control oldTopWindow = Children[Children.Count - 1];
            for (int i = index; i < Children.Count - 1; i++) {
                Children[i] = Children[i + 1];
            }
            Children[Children.Count - 1] = window;
            
            if (oldTopWindow != window)
            {
                oldTopWindow.RaiseEvent( Window.DeactivatedEvent, new RoutedEventArgs( oldTopWindow, Window.DeactivatedEvent ) );
                window.RaiseEvent(Window.ActivatedEvent, new RoutedEventArgs(window, Window.ActivatedEvent));
                initializeFocusOnActivatedWindow( window );
                Invalidate();
            }
        }
        
        public void WindowsHost_MouseDownPreview(object sender, MouseButtonEventArgs args) {
            Point position = args.GetPosition(this);
            List<Control> childrenOrderedByZIndex = GetChildrenOrderedByZIndex();
            for (int i = childrenOrderedByZIndex.Count - 1; i >= 0; i--) {
                Control topChild = childrenOrderedByZIndex[i];
                if (topChild.RenderSlotRect.Contains(position)) {
                    activateWindow((Window)topChild);
                    break;
                }
            }
        }

        private void initializeFocusOnActivatedWindow( Window window ) {
            bool reinitFocus = false;
            if ( window.StoredFocus != null ) {
                // проверяем, не удалён ли StoredFocus и является ли он Visible & Focusable
                if ( !VisualTreeHelper.IsConnectedToRoot( window.StoredFocus ) ) {
                    // todo : log warn about disconnected control
                    reinitFocus = true;
                } else if ( window.StoredFocus.Visibility != Visibility.Visible ) {
                    // todo : log warn about invizible control to be focused
                    reinitFocus = true;
                }
                else if ( !window.StoredFocus.Focusable ) {
                    // todo : log warn
                    reinitFocus = true;
                } else {
                    ConsoleApplication.Instance.FocusManager.SetFocus( window, window.StoredFocus );
                }
            } else {
                reinitFocus = true;
            }
            //
            if ( reinitFocus ) {
                if ( window.ChildToFocus != null ) {
                    Control child = VisualTreeHelper.FindChildByNameRecoursively( window, window.ChildToFocus );
                    ConsoleApplication.Instance.FocusManager.SetFocus( child );
                } else {
                    ConsoleApplication.Instance.FocusManager.SetFocusScope( window );
                }
            }
        }

        public void AddWindow(Window window) {
            if ( Children.Count != 0 ) {
                Control topWindow = Children[ Children.Count - 1 ];
                topWindow.RaiseEvent( Window.DeactivatedEvent, new RoutedEventArgs( topWindow, Window.DeactivatedEvent ) );
            }
            AddChild(window);
            window.RaiseEvent( Window.ActivatedEvent, new RoutedEventArgs( window, Window.ActivatedEvent ) );
            initializeFocusOnActivatedWindow(window);
        }

        public void RemoveWindow(Window window) {
            window.RaiseEvent( Window.DeactivatedEvent, new RoutedEventArgs( window, Window.DeactivatedEvent ) );
            RemoveChild(window);
            // после удаления окна активизировать то, которое было активным до него
            List<Control> childrenOrderedByZIndex = GetChildrenOrderedByZIndex();
            if ( childrenOrderedByZIndex.Count != 0 ) {
                Window topWindow = ( Window ) childrenOrderedByZIndex[ childrenOrderedByZIndex.Count - 1 ];
                topWindow.RaiseEvent( Window.ActivatedEvent, new RoutedEventArgs( topWindow, Window.ActivatedEvent ) );
                initializeFocusOnActivatedWindow(topWindow);
                Invalidate();
            }
        }
    }
}
