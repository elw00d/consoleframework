using System;
using System.Diagnostics;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Контрол, виртуализирующий содержимое так, что можно прокручивать его, если
    /// оно не вмещается в отведённое пространство.
    /// todo : добавить обработку нажатий мыши по ползункам и между ползунками и стрелками
    /// todo : добавить повторение нажатий мыши по таймеру
    /// </summary>
    public class ScrollViewer : Control
    {
        public ScrollViewer( ) {
//            HorizontalAlignment = HorizontalAlignment.Stretch;
//            VerticalAlignment = VerticalAlignment.Stretch;
            AddHandler( MouseDownEvent, new MouseButtonEventHandler(OnMouseDown) );
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
            switch ( direction ) {
                    case Direction.Left:
                    {
                        // сколько места сейчас оставлено дочернему контролу
                        int remainingWidth = ActualWidth - (verticalScrollVisible ? 1 : 0);
                        while (deltaX < Content.RenderSize.Width - remainingWidth)
                        {
                            deltaX++;
                        }
                        Invalidate();
                        break;
                    }
                    case Direction.Right:
                    {
                        while (deltaX > 0)
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
                        while (deltaY < Content.RenderSize.Height - remainingHeight)
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

            // последний вызов Measure должен быть именно с такими размерами, которые реально
            // будут использоваться при размещении, вот мы и размещаем его так, как будто бы у него
            // имеется сколько угодно пространства
            Content.Measure( new Size(int.MaxValue, int.MaxValue) );
            
            Size desiredSize = Content.DesiredSize;

            horizontalScrollVisible = ( desiredSize.Width > availableSize.Width );
            verticalScrollVisible = desiredSize.Height + ( horizontalScrollVisible ? 1 : 0 ) > availableSize.Height;
            if ( verticalScrollVisible ) {
                // нужно проверить ещё раз, нужен ли горизонтальный сколлбар
                horizontalScrollVisible = (desiredSize.Width + 1 > availableSize.Width);
            }

            int width = Math.Min( verticalScrollVisible ? desiredSize.Width + 1 : desiredSize.Width, availableSize.Width );
            int height = Math.Min( horizontalScrollVisible ? desiredSize.Height + 1 : desiredSize.Height, availableSize.Height );
            Size result = new Size( width, height );
            Debugger.Log( 1, "", "ScrollViewer.MeasureOverride: " + result+ "\n" );
            return result;
        }

        protected override Size ArrangeOverride(Size finalSize) {
            if ( Content == null ) return finalSize;
            int width = finalSize.Width;
            int height = finalSize.Height;
//            int width = Content.DesiredSize.Width;
//            int height = Content.DesiredSize.Height;
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

            Debugger.Log( 1,"", "Content.Arrange(" + finalRect + ")\n" );
            Content.Arrange( finalRect );
            int resultWidth =
                Math.Min(verticalScrollVisible ? 1 + finalRect.Width : finalRect.Width, width);
            int resultHeight =
                Math.Min(horizontalScrollVisible ? 1 + finalRect.Height : finalRect.Height, height);

            // !!! Нельзя так делать:
            // Нельзя вызывать для дочерних элементов Arrange со значением, превышающим
            // то, которое будет возвращено из ArrangeOverride. Это будет означать, что родительский
            // контрол сообщит, что ему надо места мало, а дочернему выделит даже больше чем имеется,
            // и дочерний контрол при рендеринге будет затирать родительский элемент управления.
            // За рамки слота родительского контрола он, конечно, не залезет (обрежется системой
            // отрисовки), но и родительскому контролу не даст ничего нарисовать.

//            int resultWidth2 =
//                Math.Min( verticalScrollVisible ? 1 + Content.DesiredSize.Width : Content.DesiredSize.Width, width );
//            int resultHeight2 =
//                Math.Min( horizontalScrollVisible ? 1 + Content.DesiredSize.Height : Content.DesiredSize.Height, height );

//            if (HorizontalAlignment == HorizontalAlignment.Stretch)
//                resultWidth = finalSize.Width;
//            if (VerticalAlignment == VerticalAlignment.Stretch)
//                resultHeight = finalSize.Height;
            Size result = new Size(resultWidth, resultHeight);
            Debugger.Log(1, "", "ScrollViewer.ArrangeOverride: " + result + "\n");
            //Debugger.Log(1, "", "ScrollViewer.ActualOffset: " + ActualOffset + "\n");
            return result;
        }

        public override void Render(RenderingBuffer buffer) {
            Debugger.Log(1, "", "ScrollViewer.RenderSize: " + RenderSize + "\n");
            Debugger.Log(1, "", "ScrollViewer.RenderSlotRect: " + RenderSlotRect + "\n");
            Debugger.Log(1, "", "ScrollViewer.ActualOffset: " + ActualOffset + "\n");

            CHAR_ATTRIBUTES attr = (CHAR_ATTRIBUTES)Color.Attr(Color.DarkCyan, Color.DarkBlue);

            buffer.SetOpacityRect( 0,0, ActualWidth, ActualHeight, 2 );

            if ( horizontalScrollVisible ) {
                buffer.SetOpacityRect( 0, ActualHeight-1, ActualWidth, 1, 0 );
                buffer.SetPixel(0, ActualHeight - 1, '\u25C4', attr); // ◄
                // оставляем дополнительный пиксель справа, если одновременно видны оба скроллбара
                int rightOffset = verticalScrollVisible ? 1 : 0;
                if ( ActualWidth > 2 + rightOffset ) {
                    buffer.FillRectangle(1, ActualHeight - 1, ActualWidth - (2 + rightOffset), 1, '\u2592', attr); // ▒
                }
                if ( ActualWidth > 1 + rightOffset ) {
                    buffer.SetPixel(ActualWidth - (1 + rightOffset), ActualHeight - 1, '\u25BA', attr); // ►
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

                    buffer.SetPixel( 1 + scrollerPos, ActualHeight - 1, '\u25A0', attr ); // ■
                } else if ( ActualWidth == 3 + ( verticalScrollVisible ? 1 : 0 ) ) {
                    buffer.SetPixel(1, ActualHeight - 1, '\u25A0', attr); // ■
                }
            }
            if ( verticalScrollVisible ) {
                buffer.SetOpacityRect(ActualWidth-1, 0, 1, ActualHeight, 0);

                buffer.SetPixel(ActualWidth - 1, 0, '\u25B2', attr); // ▲
                // оставляем дополнительный пиксель снизу, если одновременно видны оба скроллбара
                int downOffset = horizontalScrollVisible ? 1 : 0;
                if ( ActualHeight > 2 + downOffset ) {
                    buffer.FillRectangle(ActualWidth - 1, 1, 1, ActualHeight - (2 + downOffset), '\u2592', attr); // ▒
                }
                if ( ActualHeight > 1 + downOffset ) {
                    buffer.SetPixel(ActualWidth - 1, ActualHeight - (1 + downOffset), '\u25BC', attr); // ▼
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

                    buffer.SetPixel( ActualWidth - 1, 1 + scrollerPos, '\u25A0', attr ); // ■
                } else if ( ActualHeight == 3 + ( horizontalScrollVisible ? 1 : 0 ) ) {
                    buffer.SetPixel(ActualWidth - 1, 1, '\u25A0', attr); // ■
                }
            }
            if ( horizontalScrollVisible && verticalScrollVisible ) {
                buffer.SetPixel(ActualWidth - 1, ActualHeight - 1, '\u2518', attr); // ┘
            }
        }
    }
}
