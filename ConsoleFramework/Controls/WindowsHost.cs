using System;
using System.Collections.Generic;
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
            AddHandler(MouseDownEvent, new MouseButtonEventHandler(WindowsHost_MouseDown), true);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // дочерние окна могут занимать сколько угодно пространства
            foreach (Control control in children)
            {
                Window window = (Window) control;
                window.Measure(new Size(int.MaxValue, int.MaxValue));
            }
            if (availableSize.width == int.MaxValue && availableSize.height == int.MaxValue)
                return new Size(availableSize.width - 1, availableSize.height - 1);
            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // сколько дочерние окна хотели - столько и получают
            foreach (Control control in children)
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

        public void ActivateWindow(Window window) {
            int index = children.FindIndex(0, control => control == window);
            if (-1 == index)
                throw new InvalidOperationException("Assertion failed.");
            //
            Control oldTopWindow = children[children.Count - 1];
            for (int i = index; i < children.Count - 1; i++) {
                children[i] = children[i + 1];
            }
            children[children.Count - 1] = window;
            window.SetFocus();
            if (oldTopWindow != window)
                Invalidate();
        }
        
        public void WindowsHost_MouseDown(object sender, MouseButtonEventArgs args) {
            Point position = args.GetPosition(this);
            List<Control> childrenOrderedByZIndex = GetChildrenOrderedByZIndex();
            for (int i = childrenOrderedByZIndex.Count - 1; i >= 0; i--) {
                Control topChild = childrenOrderedByZIndex[i];
                if (topChild.RenderSlotRect.Contains(position)) {
                    ActivateWindow((Window)topChild);
                    break;
                }
            }
        }

        public void AddWindow(Window window) {
            AddChild(window);
        }

        public void RemoveWindow(Window window) {
            RemoveChild(window);
        }
    }
}
