using System.Diagnostics;
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
            // reserve 2 pixels for frame and 2/1 pixels for shadow
            Content.Measure(new Size(availableSize.width - 4, availableSize.height - 3));
            return new Size(Content.DesiredSize.width + 4, Content.DesiredSize.height + 3);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Content != null) {
                Content.Arrange(new Rect(1, 1, finalSize.width - 4, finalSize.height - 3));
            }
            return finalSize;
        }

        public override void Render(RenderingBuffer buffer)
        {
            ushort borderAttrs = Color.Attr(Color.White, Color.Gray);
            // background
            buffer.FillRectangle(0, 0, this.ActualWidth, this.ActualHeight, ' ', borderAttrs);
            // corners
            buffer.SetPixel(0, 0, UnicodeTable.DoubleFrameTopLeftCorner, (CHAR_ATTRIBUTES) borderAttrs);
            buffer.SetPixel(ActualWidth - 3, ActualHeight - 2, UnicodeTable.DoubleFrameBottomRightCorner, (CHAR_ATTRIBUTES) borderAttrs);
            buffer.SetPixel(0, ActualHeight - 2, UnicodeTable.DoubleFrameBottomLeftCorner, (CHAR_ATTRIBUTES)borderAttrs);
            buffer.SetPixel(ActualWidth - 3, 0, UnicodeTable.DoubleFrameTopRightCorner, (CHAR_ATTRIBUTES)borderAttrs);
            // horizontal & vertical frames
            buffer.FillRectangle(1, 0, ActualWidth - 4, 1, UnicodeTable.DoubleFrameHorizontal, borderAttrs);
            buffer.FillRectangle(1, ActualHeight - 2, ActualWidth - 4, 1, UnicodeTable.DoubleFrameHorizontal, borderAttrs);
            buffer.FillRectangle(0, 1, 1, ActualHeight - 3, UnicodeTable.DoubleFrameVertical, borderAttrs);
            buffer.FillRectangle(ActualWidth - 3, 1, 1, ActualHeight -3 , UnicodeTable.DoubleFrameVertical, borderAttrs);
            //
            buffer.SetOpacity(0, ActualHeight - 1, 2);
            buffer.SetOpacity(1, ActualHeight - 1, 2);
            buffer.SetOpacity(ActualWidth - 1, 0, 2);
            buffer.SetOpacity(ActualWidth - 2, 0, 2);
            buffer.SetOpacityRect(2, ActualHeight - 1, ActualWidth - 2, 1, 1);
            buffer.SetOpacityRect(ActualWidth - 2, 1, 2, ActualHeight - 1, 1);
        }

        private bool moving = false;
        private int movingStartX;
        private int movingStartY;
        private Point movingStartPoint;

        private bool resizing = false;
        private int resizingStartWidth;
        private int resizingStartHeight;
        private Point resizingStartPoint;

        public override void HandleEvent(INPUT_RECORD inputRecord) {
            bool eventHandled = false;
            if (inputRecord.EventType == EventType.MOUSE_EVENT && inputRecord.MouseEvent.dwButtonState == MouseButtonState.FROM_LEFT_1ST_BUTTON_PRESSED)
            {
                COORD pos = inputRecord.MouseEvent.dwMousePosition;
                Point translatedPoint = TranslatePoint(null, new Point(pos.X, pos.Y), this);
                Debug.WriteLine("MOUSE_EVENT at {0}:{1} -> {2}:{3} window : {4}", pos.X, pos.Y, translatedPoint.x, translatedPoint.y, Name);
            }
            // moving & resizing
            if (inputRecord.EventType == EventType.MOUSE_EVENT) {
                MOUSE_EVENT_RECORD mouseEvent = inputRecord.MouseEvent;
                if (moving) {
                    if ((mouseEvent.dwEventFlags & MouseEventFlags.MOUSE_MOVED) == MouseEventFlags.MOUSE_MOVED) {
                        COORD mousePosition = mouseEvent.dwMousePosition;
                        Vector vector = new Vector(mousePosition.X - movingStartPoint.x, mousePosition.Y - movingStartPoint.y);
                        X = movingStartX + vector.X;
                        Y = movingStartY + vector.Y;
                        Debug.WriteLine("X:Y {0}:{1} -> {2}:{3}", movingStartX, movingStartY, X, Y);
                        getWindowsHost().Invalidate();
                        eventHandled = true;
                    } else if ((mouseEvent.dwButtonState & MouseButtonState.FROM_LEFT_1ST_BUTTON_PRESSED) != MouseButtonState.FROM_LEFT_1ST_BUTTON_PRESSED) {
                        moving = false;
                        ConsoleApplication.Instance.EndCaptureInput(this);
                        eventHandled = true;
                    }
                }
                if (resizing) {
                    if ((mouseEvent.dwEventFlags & MouseEventFlags.MOUSE_MOVED) == MouseEventFlags.MOUSE_MOVED) {
                        COORD mousePosition = mouseEvent.dwMousePosition;
                        int deltaWidth = mousePosition.X - resizingStartPoint.x;
                        int deltaHeight = mousePosition.Y - resizingStartPoint.y;
                        int width = resizingStartWidth + deltaWidth;
                        int height = resizingStartHeight + deltaHeight;
                        bool anyChanged = false;
                        if (width >= 4) {
                            this.Width = width;
                            anyChanged = true;
                        }
                        if (height >= 3) {
                            this.Height = height;
                            anyChanged = true;
                        }
                        if (anyChanged)
                            Invalidate();
                        //
                        eventHandled = true;
                    } else if ((mouseEvent.dwButtonState & MouseButtonState.FROM_LEFT_1ST_BUTTON_PRESSED) != MouseButtonState.FROM_LEFT_1ST_BUTTON_PRESSED) {
                        resizing = false;
                        ConsoleApplication.Instance.EndCaptureInput(this);
                        eventHandled = true;
                    }
                }
                // перемещение можно начинать только когда окно не ресайзится и наоборот
                if (!moving && !resizing) {
                    if (mouseEvent.dwButtonState == MouseButtonState.FROM_LEFT_1ST_BUTTON_PRESSED) {
                        COORD mousePosition = mouseEvent.dwMousePosition;
                        Point translatedPoint = TranslatePoint(null, new Point(mousePosition.X, mousePosition.Y), this);
                        if (translatedPoint.y == 0) {
                            moving = true;
                            movingStartPoint = new Point(mousePosition.X, mousePosition.Y);
                            movingStartX = X;
                            movingStartY = Y;
                            ConsoleApplication.Instance.BeginCaptureInput(this);
                            eventHandled = true;
                        }
                        if (translatedPoint.x == ActualWidth - 3 && translatedPoint.y == ActualHeight - 2) {
                            resizing = true;
                            resizingStartPoint = new Point(mousePosition.X, mousePosition.Y);
                            resizingStartWidth = ActualWidth;
                            resizingStartHeight = ActualHeight;
                            ConsoleApplication.Instance.BeginCaptureInput(this);
                            eventHandled = true;
                        }
                    }
                }
            }
            //
            if (!eventHandled) {
                base.HandleEvent(inputRecord);
            }
        }
    }
}
