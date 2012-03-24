using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ConsoleFramework.Core;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Представляет собой элемент управления, который может содержать только 1 дочерний элемент.
    /// Как правило, в роли дочернего элемента будут использоваться менеджеры размещения.
    /// Window должно знать о хосте, в контексте которого оно располагается и уметь с ним взаимодействовать.
    /// </summary>
    public class Window : Control
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public char C { get; set; }

        public Control Content {
            get {
                return children.Count != 0 ? children[0] : null;
            }
            set {
                if (children.Count != 0) {
                    RemoveChild(children[0]);
                }
                AddChild(value);
            }
        }

        private WindowsHost getWindowsHost()
        {
            return (WindowsHost) Parent;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (Content == null)
                return base.MeasureOverride(availableSize);
            // reserve 2 pixels for frame
            Content.Measure(new Size(availableSize.width - 2, availableSize.height - 2));
            return new Size(Content.DesiredSize.width + 2, Content.DesiredSize.height + 2);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Content != null) {
                Content.Arrange(new Rect(1, 1, finalSize.width - 2, finalSize.height - 2));
            }
            return finalSize;
        }

        public override void Render(RenderingBuffer buffer)
        {
            buffer.FillRectangle(0, 0, this.ActualWidth, this.ActualHeight, C, CHAR_ATTRIBUTES.FOREGROUND_BLUE | CHAR_ATTRIBUTES.BACKGROUND_GREEN);
        }

        public override void HandleEvent(INPUT_RECORD inputRecord)
        {
            base.HandleEvent(inputRecord);
            if (inputRecord.EventType == EventType.MOUSE_EVENT && inputRecord.MouseEvent.dwButtonState == MouseButtonState.FROM_LEFT_1ST_BUTTON_PRESSED)
            {
                COORD pos = inputRecord.MouseEvent.dwMousePosition;
                Point translatedPoint = TranslatePoint(null, new Point(pos.X, pos.Y), this);
                Debug.WriteLine("MOUSE_EVENT at {0}:{1} -> {2}:{3} window : {4}", pos.X, pos.Y, translatedPoint.x, translatedPoint.y, Name);
            }
        }
    }
}
