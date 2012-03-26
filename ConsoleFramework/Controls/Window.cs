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
            ushort borderAttrs = Color.Attr(Color.White, Color.Gray);
            // background
            buffer.FillRectangle(0, 0, this.ActualWidth, this.ActualHeight, ' ', borderAttrs);
            // corners
            buffer.SetPixel(0, 0, UnicodeTable.DoubleFrameTopLeftCorner, (CHAR_ATTRIBUTES) borderAttrs);
            buffer.SetPixel(ActualWidth - 1, ActualHeight - 1, UnicodeTable.DoubleFrameBottomRightCorner, (CHAR_ATTRIBUTES) borderAttrs);
            buffer.SetPixel(0, ActualHeight - 1, UnicodeTable.DoubleFrameBottomLeftCorner, (CHAR_ATTRIBUTES)borderAttrs);
            buffer.SetPixel(ActualWidth - 1, 0, UnicodeTable.DoubleFrameTopRightCorner, (CHAR_ATTRIBUTES)borderAttrs);
            // horizontal & vertical frames
            buffer.FillRectangle(1, 0, ActualWidth - 2, 1, UnicodeTable.DoubleFrameHorizontal, borderAttrs);
            buffer.FillRectangle(1, ActualHeight - 1, ActualWidth - 2, 1, UnicodeTable.DoubleFrameHorizontal, borderAttrs);
            buffer.FillRectangle(0, 1, 1, ActualHeight - 2, UnicodeTable.DoubleFrameVertical, borderAttrs);
            buffer.FillRectangle(ActualWidth - 1, 1, 1, ActualHeight - 2, UnicodeTable.DoubleFrameVertical, borderAttrs);
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
