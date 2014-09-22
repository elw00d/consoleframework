using System;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Event args containing info for ScrollViewer - how to display inner content.
    /// </summary>
    public class ContentShouldBeScrolledEventArgs : RoutedEventArgs
    {
        private readonly int? mostLeftVisibleX;
        private readonly int? mostRightVisibleX;
        private readonly int? mostTopVisibleY;
        private readonly int? mostBottomVisibleY;

        public ContentShouldBeScrolledEventArgs( object source, RoutedEvent routedEvent,
            int? mostLeftVisibleX, int? mostRightVisibleX,
            int? mostTopVisibleY, int? mostBottomVisibleY)
            : base(source, routedEvent) {
            if (mostLeftVisibleX.HasValue && mostRightVisibleX.HasValue)
                throw new ArgumentException("Only one of X values can be specified");
            if (mostTopVisibleY.HasValue && mostBottomVisibleY.HasValue)
                throw new ArgumentException("Only one of Y values can be specified");
            this.mostLeftVisibleX = mostLeftVisibleX;
            this.mostRightVisibleX = mostRightVisibleX;
            this.mostTopVisibleY = mostTopVisibleY;
            this.mostBottomVisibleY = mostBottomVisibleY;
        }

        public int? MostLeftVisibleX {
            get { return mostLeftVisibleX; }
        }

        public int? MostRightVisibleX {
            get { return mostRightVisibleX; }
        }

        public int? MostTopVisibleY {
            get { return mostTopVisibleY; }
        }

        public int? MostBottomVisibleY {
            get { return mostBottomVisibleY; }
        }
    }

    public delegate void ContentShouldBeScrolledEventHandler(object sender,
                                                                  ContentShouldBeScrolledEventArgs args);


    /// <summary>
    /// Контрол, виртуализирующий содержимое так, что можно прокручивать его, если
    /// оно не вмещается в отведённое пространство.
    /// todo : добавить обработку нажатий мыши по ползункам и между ползунками и стрелками
    /// todo : добавить повторение нажатий мыши по таймеру
    /// </summary>
    public class ScrollViewer : Control
    {
        /// <summary>
        /// Event can be fired by children when needs to explicitly set current
        /// visible region (for example, ListBox after mouse wheel scrolling).
        /// </summary>
        public static RoutedEvent ContentShouldBeScrolledEvent =
            EventManager.RegisterRoutedEvent("ContentShouldBeScrolled", RoutingStrategy.Bubble, 
            typeof(ContentShouldBeScrolledEventHandler), typeof(ScrollViewer));
        
        public ScrollViewer( ) {
            AddHandler( MouseDownEvent, new MouseButtonEventHandler(OnMouseDown) );
            HorizontalScrollEnabled = true;
            VerticalScrollEnabled = true;
            AddHandler( ContentShouldBeScrolledEvent, new ContentShouldBeScrolledEventHandler(onContentShouldBeScrolled) );
        }

        private void onContentShouldBeScrolled( object sender, ContentShouldBeScrolledEventArgs args ) {
            if ( args.MostLeftVisibleX.HasValue ) {
                if ( this.deltaX <= args.MostLeftVisibleX.Value &&
                     this.deltaX + this.RenderSize.Width > args.MostLeftVisibleX.Value ) {
                    // This X coord is already visible - do nothing
                } else {
                    this.deltaX = Math.Min( args.MostLeftVisibleX.Value,
                                            Content.RenderSize.Width - this.RenderSize.Width );
                }
            } else if ( args.MostRightVisibleX.HasValue ) {
                if (this.deltaX <= args.MostRightVisibleX.Value &&
                     this.deltaX + this.RenderSize.Width > args.MostRightVisibleX.Value ) {
                    // This X coord is already visible - do nothing
                } else {
                    this.deltaX = Math.Max(args.MostRightVisibleX.Value - this.ActualWidth + 1,
                                            0 );
                }
            }

            if ( args.MostTopVisibleY.HasValue ) {
                if (this.deltaY <= args.MostTopVisibleY.Value &&
                     this.deltaY + this.RenderSize.Height > args.MostTopVisibleY.Value ) {
                    // This Y coord is already visible - do nothing
                } else {
                    this.deltaY = Math.Min(args.MostTopVisibleY.Value,
                                            Content.RenderSize.Height - this.RenderSize.Height );
                }
            } else if ( args.MostBottomVisibleY.HasValue ) {
                if (this.deltaY <= args.MostBottomVisibleY.Value &&
                     this.deltaY + this.RenderSize.Height > args.MostBottomVisibleY.Value ) {
                    // This Y coord is already visible - do nothing
                } else {
                    this.deltaY = Math.Max(args.MostBottomVisibleY.Value - this.ActualHeight + 1,
                                            0 );
                }
            }

            this.Invalidate(  );
            
        }

        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        public bool ContentFullyVisible {
            get { return !verticalScrollVisible && !horizontalScrollVisible; }
        }

        public void ScrollContent( Direction direction, int delta ) {
            for (int i = 0; i < delta; i++)
            switch ( direction ) {
                    case Direction.Left:
                    {
                        // сколько места сейчас оставлено дочернему контролу
                        int remainingWidth = ActualWidth - (verticalScrollVisible ? 1 : 0);
                        if (deltaX < Content.RenderSize.Width - remainingWidth)
                        {
                            deltaX++;
                        }
                        Invalidate();
                        break;
                    }
                    case Direction.Right:
                    {
                        if (deltaX > 0)
                        {
                            deltaX--;
                        }
                        Invalidate();
                        break;
                    }
                    case Direction.Up:
                    {
                        // сколько места сейчас оставлено дочернему контролу
                        int remainingHeight = ActualHeight - (horizontalScrollVisible ? 1 : 0);
                        if (deltaY < Content.RenderSize.Height - remainingHeight)
                        {
                            deltaY++;
                        }
                        Invalidate();
                        break;
                    }
                    case Direction.Down:
                    {
                        if (deltaY > 0)
                        {
                            deltaY--;
                        }
                        Invalidate();
                        break;
                    }
            }
        }

        public int DeltaX {
            get { return deltaX; }
        }

        public int DeltaY {
            get { return deltaY; }
        }

        public bool HorizontalScrollVisible {
            get { return horizontalScrollVisible; }
        }

        public bool VerticalScrollVisible {
            get { return verticalScrollVisible; }
        }

        public bool VerticalScrollEnabled {
            get;
            set;
        }

        public bool HorizontalScrollEnabled {
            get;
            set;
        }

        private void OnMouseDown( object sender, MouseButtonEventArgs args ) {
            Point pos = args.GetPosition( this );
            if ( horizontalScrollVisible ) {
                if ( pos == new Point( 0, ActualHeight - 1 ) ) {
                    // left arrow clicked
                    if (deltaX > 0)
                    {
                        deltaX--;
                        Invalidate();
                    }
                    
                } else {
                    if ( pos ==
                         new Point( ActualWidth - ( 1 + ( verticalScrollVisible ? 1 : 0 ) ), ActualHeight - 1 ) ) {
                        // right arrow clicked
                        
                        // сколько места сейчас оставлено дочернему контролу
                        int remainingWidth = ActualWidth - (verticalScrollVisible ? 1 : 0);
                        if (deltaX < Content.RenderSize.Width - remainingWidth)
                        {
                            deltaX++;
                            Invalidate();
                        }
                    }
                }
            }
            if ( verticalScrollVisible ) {
                if ( pos == new Point( ActualWidth - 1, 0 ) ) {
                    // up arrow clicked
                    if (deltaY > 0)
                    {
                        deltaY--;
                        Invalidate();
                    }
                } else {
                    if ( pos ==
                         new Point( ActualWidth - 1, ActualHeight - ( 1 + ( horizontalScrollVisible ? 1 : 0 ) ) ) ) {
                        // down arrow clicked
                        
                        // сколько места сейчас оставлено дочернему контролу
                        int remainingHeight = ActualHeight - (horizontalScrollVisible ? 1 : 0);
                        if (deltaY < Content.RenderSize.Height - remainingHeight)
                        {
                            deltaY++;
                            Invalidate();
                        }
                    }
                }
            }
            args.Handled = true;
        }

        private Control content;
        public Control Content {
            get { return content; }
            set {
                if ( content != null )
                    RemoveChild( content );
                AddChild( value );
                content = value;
            }
        }

        private bool horizontalScrollVisible = false;
        private bool verticalScrollVisible = false;

        private int deltaX;
        private int deltaY;
        
        protected override Size MeasureOverride(Size availableSize) {
            if (Content == null) return new Size(0, 0);

            // Размещаем контрол так, как будто бы у него имеется сколько угодно пространства
            Content.Measure( new Size(int.MaxValue, int.MaxValue) );
            
            Size desiredSize = Content.DesiredSize;

            horizontalScrollVisible = HorizontalScrollEnabled && (desiredSize.Width > availableSize.Width);
            verticalScrollVisible = VerticalScrollEnabled && (desiredSize.Height + (horizontalScrollVisible ? 1 : 0) > availableSize.Height);
            if ( verticalScrollVisible ) {
                // Нужно проверить ещё раз, нужен ли горизонтальный сколлбар
                horizontalScrollVisible = HorizontalScrollEnabled && (desiredSize.Width + 1 > availableSize.Width);
            }

            int width = Math.Min( verticalScrollVisible ? desiredSize.Width + 1 : desiredSize.Width, availableSize.Width );
            int height = Math.Min( horizontalScrollVisible ? desiredSize.Height + 1 : desiredSize.Height, availableSize.Height );

            // Если горизонтальная прокрутка отключена - то мы должны сообщить контролу, что по горизонтали он будет иметь не int.MaxValue
            // пространства, а ровно width. Таким образом мы даём возможность контролу приспособиться к тому, что прокрутки по горизонтали не будет.
            // Аналогично и с вертикальной прокруткой. Так как последний вызов Measure должен быть именно с такими размерами, которые реально
            // будут использоваться при размещении, то мы и должны выполнить Measure ещё раз.
            if (!HorizontalScrollEnabled || !VerticalScrollEnabled) {
                Content.Measure(new Size(HorizontalScrollEnabled ? int.MaxValue : width, VerticalScrollEnabled ? int.MaxValue : height));
            }

            Size result = new Size( width, height );
            return result;
        }

        protected override Size ArrangeOverride(Size finalSize) {
            if ( Content == null ) return finalSize;
            int width = finalSize.Width;
            int height = finalSize.Height;
            Rect finalRect = new Rect( new Point( -deltaX, -deltaY ), 
                new Size(
                    deltaX + Math.Max( 0, verticalScrollVisible ? width - 1 : width ), 
                    deltaY + Math.Max( 0, horizontalScrollVisible ? height - 1 : height ) )
                );

            // если мы сдвинули окно просмотра, а потом размеры, доступные контролу, увеличились,
            // мы должны вернуть дочерний контрол в точку (0, 0)
            if (deltaX > Content.DesiredSize.Width - Math.Max(0, verticalScrollVisible ? width - 1 : width))
            {
                deltaX = 0;
                finalRect = new Rect(new Point(-deltaX, -deltaY),
                new Size(
                    deltaX + Math.Max(0, verticalScrollVisible ? width - 1 : width),
                    deltaY + Math.Max(0, horizontalScrollVisible ? height - 1 : height))
                );
            }
            if (deltaY > Content.DesiredSize.Height - Math.Max(0, horizontalScrollVisible ? height - 1 : height))
            {
                deltaY = 0;
                finalRect = new Rect(new Point(-deltaX, -deltaY),
                new Size(
                    deltaX + Math.Max(0, verticalScrollVisible ? width - 1 : width),
                    deltaY + Math.Max(0, horizontalScrollVisible ? height - 1 : height))
                );
            }

            Content.Arrange( finalRect );
            int resultWidth =
                Math.Min(verticalScrollVisible ? 1 + finalRect.Width : finalRect.Width, width);
            int resultHeight =
                Math.Min(horizontalScrollVisible ? 1 + finalRect.Height : finalRect.Height, height);

            Size result = new Size(resultWidth, resultHeight);
            return result;
        }

        public override void Render(RenderingBuffer buffer) {
            Attr attr = Colors.Blend(Color.DarkCyan, Color.DarkBlue);

            buffer.SetOpacityRect( 0,0, ActualWidth, ActualHeight, 2 );

            if ( horizontalScrollVisible ) {
                buffer.SetOpacityRect( 0, ActualHeight-1, ActualWidth, 1, 0 );
                buffer.SetPixel(0, ActualHeight - 1, UnicodeTable.ArrowLeft, attr); // ◄
                // оставляем дополнительный пиксель справа, если одновременно видны оба скроллбара
                int rightOffset = verticalScrollVisible ? 1 : 0;
                if ( ActualWidth > 2 + rightOffset ) {
                    buffer.FillRectangle(1, ActualHeight - 1, ActualWidth - (2 + rightOffset), 1, 
                        UnicodeTable.MediumShade, attr); // ▒
                }
                if ( ActualWidth > 1 + rightOffset ) {
                    buffer.SetPixel(ActualWidth - (1 + rightOffset), ActualHeight - 1,
                        UnicodeTable.ArrowRight, attr); // ►
                }

                // определим, в каком месте находится ползунок
                if ( ActualWidth > 3 + ( verticalScrollVisible ? 1 : 0 ) ) {
                    int remainingWidth = ActualWidth - ( verticalScrollVisible ? 1 : 0 );
                    int extraWidth = Content.RenderSize.Width - remainingWidth;
                    int pages = extraWidth/( remainingWidth - 2 - 1 );

                    //Debugger.Log( 1, "", "pages: " + pages + "\n" );

                    int scrollerPos;
                    if ( pages == 0 ) {
                        double posInDelta = ( remainingWidth*1.0 - 2 - 1 )/extraWidth;
                        //Debugger.Log( 1, "", "posInDelta: " + posInDelta + "\n" );
                        scrollerPos = ( int ) Math.Round( posInDelta*deltaX );
                    } else {
                        double deltaInPos = ( extraWidth*1.0 )/( remainingWidth - 2 - 1 );
                        //Debugger.Log( 1, "", "deltaX/( deltaInPos ): " + deltaX/( deltaInPos ) + "\n" );
                        scrollerPos = ( int ) Math.Round( deltaX/( deltaInPos ) );
                    }

                    buffer.SetPixel(1 + scrollerPos, ActualHeight - 1, UnicodeTable.BlackSquare, attr); // ■
                } else if ( ActualWidth == 3 + ( verticalScrollVisible ? 1 : 0 ) ) {
                    buffer.SetPixel(1, ActualHeight - 1, UnicodeTable.BlackSquare, attr); // ■
                }
            }
            if ( verticalScrollVisible ) {
                buffer.SetOpacityRect(ActualWidth-1, 0, 1, ActualHeight, 0);

                buffer.SetPixel(ActualWidth - 1, 0, UnicodeTable.ArrowUp, attr); // ▲
                // оставляем дополнительный пиксель снизу, если одновременно видны оба скроллбара
                int downOffset = horizontalScrollVisible ? 1 : 0;
                if ( ActualHeight > 2 + downOffset ) {
                    buffer.FillRectangle(ActualWidth - 1, 1, 1, ActualHeight - (2 + downOffset), UnicodeTable.MediumShade, attr); // ▒
                }
                if ( ActualHeight > 1 + downOffset ) {
                    buffer.SetPixel(ActualWidth - 1, ActualHeight - (1 + downOffset), UnicodeTable.ArrowDown, attr); // ▼
                }

                // определим, в каком месте находится ползунок
                if ( ActualHeight > 3 + ( horizontalScrollVisible ? 1 : 0 ) ) {
                    int remainingHeight = ActualHeight - (horizontalScrollVisible ? 1 : 0);
                    int extraHeight = Content.RenderSize.Height - remainingHeight;
                    int pages = extraHeight/( remainingHeight - 2 - 1 );

                    //Debugger.Log( 1, "", "pages: " + pages + "\n" );

                    int scrollerPos;
                    if ( pages == 0 ) {
                        double posInDelta = ( remainingHeight*1.0 - 2 - 1 )/extraHeight;
                        //Debugger.Log( 1, "", "posInDelta: " + posInDelta + "\n" );
                        scrollerPos = ( int ) Math.Round( posInDelta*deltaY );
                    } else {
                        double deltaInPos = ( extraHeight*1.0 )/( remainingHeight - 2 - 1 );
                        //Debugger.Log( 1, "", "deltaY/( deltaInPos ): " + deltaY/( deltaInPos ) + "\n" );
                        scrollerPos = ( int ) Math.Round( deltaY/( deltaInPos ) );
                    }

                    buffer.SetPixel(ActualWidth - 1, 1 + scrollerPos, UnicodeTable.BlackSquare, attr); // ■
                } else if ( ActualHeight == 3 + ( horizontalScrollVisible ? 1 : 0 ) ) {
                    buffer.SetPixel(ActualWidth - 1, 1, UnicodeTable.BlackSquare, attr); // ■
                }
            }
            if ( horizontalScrollVisible && verticalScrollVisible ) {
                buffer.SetPixel(ActualWidth - 1, ActualHeight - 1, UnicodeTable.SingleFrameBottomRightCorner, attr); // ┘
            }
        }
    }
}
