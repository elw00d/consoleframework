using System;
using System.Collections.Generic;
using ConsoleFramework.Core;
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
        protected override Size MeasureOverride(Size availableSize)
        {
            // дочерние окна могут занимать сколько угодно пространства
            foreach (Control control in children)
            {
                Window window = (Window) control;
                window.Measure(availableSize);
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
            Control oldTopWindow = children[children.Count - 1];
            children[children.Count - 1] = window;
            children[index] = oldTopWindow;
            if (oldTopWindow != window)
                Invalidate();
        }

        public override void HandleEvent(INPUT_RECORD inputRecord)
        {
            // todo : add another event types support
            if (inputRecord.EventType == EventType.MOUSE_EVENT) {
                MOUSE_EVENT_RECORD mouseEvent = inputRecord.MouseEvent;
                COORD position = mouseEvent.dwMousePosition;
                List<Control> childrenOrderedByZIndex = GetChildrenOrderedByZIndex();
                Point translatedPoint = Control.TranslatePoint(null, new Point(position.X, position.Y), this);
                for (int i = childrenOrderedByZIndex.Count - 1; i >= 0; i--) {
                    Control topChild = childrenOrderedByZIndex[i];
                    if (topChild.RenderSlotRect.Contains(translatedPoint)) {
                        if (mouseEvent.dwButtonState == MouseButtonState.FROM_LEFT_1ST_BUTTON_PRESSED &&
                            i != childrenOrderedByZIndex.Count - 1) {
                            //
                            ActivateWindow((Window) topChild);
                        }
                        topChild.HandleEvent(inputRecord);
                        break;
                    }
                }
            } else 
                base.HandleEvent(inputRecord);
        }

        public void AddWindow(Window window) {
            AddChild(window);
        }

        public void RemoveWindow(Window window) {
            RemoveChild(window);
        }
    }
}
