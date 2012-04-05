using System.Diagnostics;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
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
        public Window() {
            EventManager.AddHandler(this, Control.MouseDownEvent, new MouseButtonEventHandler(Window_OnMouseDown));
            EventManager.AddHandler(this, Control.MouseUpEvent, new MouseButtonEventHandler(Window_OnMouseUp));
            EventManager.AddHandler(this, Control.MouseMoveEvent, new MouseEventHandler(Window_OnMouseMove));
        }

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

        public string Title {
            get;
            set;
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
            ushort borderAttrs = moving ? Color.Attr(Color.Green, Color.Gray) : Color.Attr(Color.White, Color.Gray);
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
            // close button
            if (ActualWidth > 4) {
                buffer.SetPixel(2, 0, '[');
                buffer.SetPixel(3, 0, showClosingGlyph ? UnicodeTable.WindowClosePressedSymbol : UnicodeTable.WindowCloseSymbol,
                    (CHAR_ATTRIBUTES) Color.Attr(Color.Green, Color.Gray));
                buffer.SetPixel(4, 0, ']');
            }
            // shadows
            buffer.SetOpacity(0, ActualHeight - 1, 2);
            buffer.SetOpacity(1, ActualHeight - 1, 2);
            buffer.SetOpacity(ActualWidth - 1, 0, 2);
            buffer.SetOpacity(ActualWidth - 2, 0, 2);
            buffer.SetOpacityRect(2, ActualHeight - 1, ActualWidth - 2, 1, 1);
            buffer.SetOpacityRect(ActualWidth - 2, 1, 2, ActualHeight - 1, 1);
            // title
            if (!string.IsNullOrEmpty(Title)) {
                int titleStartX = 7;
                bool renderTitle = false;
                string renderTitleString = null;
                int availablePixelsCount = ActualWidth - titleStartX*2;
                if (availablePixelsCount > 0) {
                    renderTitle = true;
                    if (Title.Length <= availablePixelsCount) {
                        // dont truncate title
                        titleStartX += (availablePixelsCount - Title.Length)/2;
                        renderTitleString = Title;
                    } else {
                        renderTitleString = Title.Substring(0, availablePixelsCount);
                        if (renderTitleString.Length > 2) {
                            renderTitleString = renderTitleString.Substring(0, renderTitleString.Length - 2) + "..";
                        } else {
                            renderTitle = false;
                        }
                    }
                }
                if (renderTitle) {
                    // assert !string.IsNullOrEmpty(renderingTitleString);
                    buffer.SetPixel(titleStartX - 1, 0, ' ', (CHAR_ATTRIBUTES) borderAttrs);
                    for (int i = 0; i < renderTitleString.Length; i++) {
                        buffer.SetPixel(titleStartX + i, 0, renderTitleString[i], (CHAR_ATTRIBUTES) borderAttrs);
                    }
                    buffer.SetPixel(titleStartX + renderTitleString.Length, 0, ' ', (CHAR_ATTRIBUTES)borderAttrs);
                }
            }
        }

        private bool closing = false;
        private bool showClosingGlyph = false;

        private bool moving = false;
        private int movingStartX;
        private int movingStartY;
        private Point movingStartPoint;

        private bool resizing = false;
        private int resizingStartWidth;
        private int resizingStartHeight;
        private Point resizingStartPoint;

        public void Window_OnMouseDown(object sender, MouseButtonEventArgs args) {
            // перемещение можно начинать только когда окно не ресайзится и наоборот
            if (!moving && !resizing && !closing) {
                Point point = args.GetPosition(this);
                Point parentPoint = args.GetPosition(getWindowsHost());
                if (point.y == 0 && point.x == 3) {
                    closing = true;
                    showClosingGlyph = true;
                    ConsoleApplication.Instance.BeginCaptureInput(this);
                    // closing is started, we should redraw the border
                    Invalidate();
                    args.Handled = true;
                } else if (point.y == 0) {
                    moving = true;
                    movingStartPoint = parentPoint;
                    movingStartX = X;
                    movingStartY = Y;
                    ConsoleApplication.Instance.BeginCaptureInput(this);
                    // moving is started, we should redraw the border
                    Invalidate();
                    args.Handled = true;
                } else if (point.x == ActualWidth - 3 && point.y == ActualHeight - 2) {
                    resizing = true;
                    resizingStartPoint = parentPoint;
                    resizingStartWidth = ActualWidth;
                    resizingStartHeight = ActualHeight;
                    ConsoleApplication.Instance.BeginCaptureInput(this);
                    // resizing is started, we should redraw the border
                    Invalidate();
                    args.Handled = true;
                }
            }
        }

        public void Window_OnMouseUp(object sender, MouseButtonEventArgs args) {
            if (closing) {
                Point point = args.GetPosition(this);
                if (point.x == 3 && point.y == 0) {
                    getWindowsHost().RemoveWindow(this);
                }
                closing = false;
                showClosingGlyph = false;
                ConsoleApplication.Instance.EndCaptureInput(this);
                Invalidate();
                args.Handled = true;
            }
            if (moving) {
                moving = false;
                ConsoleApplication.Instance.EndCaptureInput(this);
                Invalidate();
                args.Handled = true;
            }
            if (resizing) {
                resizing = false;
                ConsoleApplication.Instance.EndCaptureInput(this);
                Invalidate();
                args.Handled = true;
            }
        }

        public void Window_OnMouseMove(object sender, MouseEventArgs args) {
            if (closing) {
                Point point = args.GetPosition(this);
                bool anyChanged = false;
                if (point.x == 3 && point.y == 0) {
                    if (!showClosingGlyph) {
                        showClosingGlyph = true;
                        anyChanged = true;
                    }
                } else {
                    if (showClosingGlyph) {
                        showClosingGlyph = false;
                        anyChanged = true;
                    }
                }
                if (anyChanged)
                    Invalidate();
                args.Handled = true;
            }
            if (moving) {
                Point parentPoint = args.GetPosition(getWindowsHost());
                Vector vector = new Vector(parentPoint.X - movingStartPoint.x, parentPoint.Y - movingStartPoint.y);
                X = movingStartX + vector.X;
                Y = movingStartY + vector.Y;
                Debug.WriteLine("X:Y {0}:{1} -> {2}:{3}", movingStartX, movingStartY, X, Y);
                getWindowsHost().Invalidate();
                args.Handled = true;
            }
            if (resizing) {
                Point parentPoint = args.GetPosition(getWindowsHost());
                int deltaWidth = parentPoint.X - resizingStartPoint.x;
                int deltaHeight = parentPoint.Y - resizingStartPoint.y;
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
                args.Handled = true;
            }
        }
    }
}
