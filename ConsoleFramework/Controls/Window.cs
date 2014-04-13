using System;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Представляет собой элемент управления, который может содержать только 1 дочерний элемент.
    /// Как правило, в роли дочернего элемента будут использоваться менеджеры размещения.
    /// Window должно знать о хосте, в контексте которого оно располагается и уметь с ним взаимодействовать.
    /// </summary>
    public class Window : Control
    {
        public static RoutedEvent ActivatedEvent = EventManager.RegisterRoutedEvent("Activated", RoutingStrategy.Direct, typeof(EventHandler), typeof(Window));
        public static RoutedEvent DeactivatedEvent = EventManager.RegisterRoutedEvent("Deactivated", RoutingStrategy.Direct, typeof(EventHandler), typeof(Window));
        public static RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent("Closed", RoutingStrategy.Direct, typeof(EventHandler), typeof(Window));

        public string ChildToFocus
        {
            get; set;
        }

        public Window() {
            this.IsFocusScope = true;
            AddHandler(PreviewMouseDownEvent, new MouseButtonEventHandler(Window_OnPreviewMouseDown));
            initialize(  );

            // todo : remove after issue https://github.com/sq/JSIL/issues/388 will be fixed
            this.X = null;
            this.Y = null;
        }

        protected virtual void initialize( ) {
            AddHandler(MouseDownEvent, new MouseButtonEventHandler(Window_OnMouseDown));
            AddHandler(MouseUpEvent, new MouseButtonEventHandler(Window_OnMouseUp));
            AddHandler(MouseMoveEvent, new MouseEventHandler(Window_OnMouseMove));
            AddHandler(PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown));
        }

        /// <summary>
        /// По клику мышки ищет конечный Focusable контрол, который размещён 
        /// на месте нажатия и устанавливает на нём фокус.
        /// </summary>
        private void Window_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PassFocusToChildUnderPoint( e );
        }

        protected virtual void OnPreviewKeyDown(object sender, KeyEventArgs args) {
            if (args.wVirtualKeyCode == VirtualKeys.Tab) {
                ConsoleApplication.Instance.FocusManager.MoveFocusNext();
                args.Handled = true;
            }
        }

        /// <summary>
        /// Координаты в рамках WindowsHost. По сути - костыль вместо
        /// отсутствующих пока Attached properties.
        /// </summary>
        public int? X { get; set; }
        public int? Y { get; set; }

        public Control Content {
            get {
                return Children.Count != 0 ? Children[0] : null;
            }
            set {
                if (Children.Count != 0) {
                    RemoveChild(Children[0]);
                }
                AddChild(value);
            }
        }

        public string Title {
            get;
            set;
        }

        protected WindowsHost getWindowsHost()
        {
            return (WindowsHost) Parent;
        }

        public static Size EMPTY_WINDOW_SIZE = new Size(12, 3);

        protected override Size MeasureOverride(Size availableSize) {
            if ( Content == null )
                return new Size(
                    Math.Min(availableSize.Width ,EMPTY_WINDOW_SIZE.Width + 4),
                    Math.Min(availableSize.Height, EMPTY_WINDOW_SIZE.Height + 3));
            if ( availableSize.Width != int.MaxValue && availableSize.Height != int.MaxValue ) {
                // reserve 2 pixels for frame and 2/1 pixels for shadow
                Content.Measure( new Size( availableSize.width - 4, availableSize.height - 3 ) );
            } else {
                int width = availableSize.Width != int.MaxValue ? availableSize.Width - 4 : int.MaxValue;
                int height = availableSize.Height != int.MaxValue ? availableSize.Height - 3 : int.MaxValue;
                Content.Measure( new Size(width, height));
            }
            var result = new Size(Content.DesiredSize.width + 4, Content.DesiredSize.height + 3);
            return result;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Content != null) {
                Content.Arrange(new Rect(1, 1, 
                    finalSize.width - 4,
                    finalSize.height - 3));
            }
            return finalSize;
        }

        protected void RenderBorders( RenderingBuffer buffer, Point a, Point b, bool singleOrDouble, Attr attrs ) {
            if ( singleOrDouble ) {
                // Corners
                buffer.SetPixel(a.X, a.Y, UnicodeTable.SingleFrameTopLeftCorner, attrs);
                buffer.SetPixel(b.X, b.Y, UnicodeTable.SingleFrameBottomRightCorner, attrs);
                buffer.SetPixel(a.X, b.Y, UnicodeTable.SingleFrameBottomLeftCorner, attrs);
                buffer.SetPixel(b.X, a.Y, UnicodeTable.SingleFrameTopRightCorner, attrs);
                // Horizontal & vertical frames
                buffer.FillRectangle(a.X + 1, a.Y, b.X - a.X - 1, 1, UnicodeTable.SingleFrameHorizontal, attrs);
                buffer.FillRectangle(a.X + 1, b.Y, b.X - a.X - 1, 1, UnicodeTable.SingleFrameHorizontal, attrs);
                buffer.FillRectangle(a.X, a.Y + 1, 1, b.Y - a.Y - 1, UnicodeTable.SingleFrameVertical, attrs);
                buffer.FillRectangle(b.X, a.Y + 1, 1, b.Y - a.Y - 1, UnicodeTable.SingleFrameVertical, attrs);
            } else {
                // Corners
                buffer.SetPixel( a.X, a.Y, UnicodeTable.DoubleFrameTopLeftCorner, attrs );
                buffer.SetPixel( b.X, b.Y, UnicodeTable.DoubleFrameBottomRightCorner, attrs );
                buffer.SetPixel( a.X, b.Y, UnicodeTable.DoubleFrameBottomLeftCorner, attrs );
                buffer.SetPixel( b.X, a.Y, UnicodeTable.DoubleFrameTopRightCorner, attrs );
                // Horizontal & vertical frames
                buffer.FillRectangle( a.X + 1, a.Y, b.X - a.X - 1, 1, UnicodeTable.DoubleFrameHorizontal, attrs );
                buffer.FillRectangle( a.X + 1, b.Y, b.X - a.X - 1, 1, UnicodeTable.DoubleFrameHorizontal, attrs );
                buffer.FillRectangle( a.X, a.Y + 1, 1, b.Y - a.Y - 1, UnicodeTable.DoubleFrameVertical, attrs );
                buffer.FillRectangle( b.X, a.Y + 1, 1, b.Y - a.Y - 1, UnicodeTable.DoubleFrameVertical, attrs );
            }
        }

        public override void Render(RenderingBuffer buffer)
        {
            Attr borderAttrs = moving ? Colors.Blend(Color.Green, Color.Gray) : Colors.Blend(Color.White, Color.Gray);
            // background
            buffer.FillRectangle(0, 0, this.ActualWidth, this.ActualHeight, ' ', borderAttrs);
            // Borders
            RenderBorders(buffer, new Point(0, 0), new Point(ActualWidth - 3, ActualHeight - 2),
                this.moving || this.resizing, borderAttrs);
            // close button
            if (ActualWidth > 4) {
                buffer.SetPixel(2, 0, '[');
                buffer.SetPixel(3, 0, showClosingGlyph ? UnicodeTable.WindowClosePressedSymbol : UnicodeTable.WindowCloseSymbol,
                    Colors.Blend(Color.Green, Color.Gray));
                buffer.SetPixel(4, 0, ']');
            }
            // shadows
            buffer.SetOpacity(0, ActualHeight - 1, 2+4);
            buffer.SetOpacity(1, ActualHeight - 1, 2+4);
            buffer.SetOpacity(ActualWidth - 1, 0, 2+4);
            buffer.SetOpacity(ActualWidth - 2, 0, 2+4);
            buffer.SetOpacityRect(2, ActualHeight - 1, ActualWidth - 2, 1, 1+4);
            buffer.SetOpacityRect(ActualWidth - 2, 1, 2, ActualHeight - 1, 1+4);
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
                    buffer.SetPixel(titleStartX - 1, 0, ' ', borderAttrs);
                    for (int i = 0; i < renderTitleString.Length; i++) {
                        buffer.SetPixel(titleStartX + i, 0, renderTitleString[i], borderAttrs);
                    }
                    buffer.SetPixel(titleStartX + renderTitleString.Length, 0, ' ', borderAttrs);
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
                    movingStartX = RenderSlotRect.TopLeft.X;
                    movingStartY = RenderSlotRect.TopLeft.Y;
                    ConsoleApplication.Instance.BeginCaptureInput(this);
                    // moving is started, we should redraw the border
                    Invalidate();
                    args.Handled = true;
                } else if (point.x == ActualWidth - 3 && point.y == ActualHeight - 2)
                {
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

        public void Close( ) {
            getWindowsHost(  ).CloseWindow( this );
        }

        public void Window_OnMouseUp(object sender, MouseButtonEventArgs args) {
            if (closing) {
                Point point = args.GetPosition(this);
                if (point.x == 3 && point.y == 0) {
                    getWindowsHost().CloseWindow(this);
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
