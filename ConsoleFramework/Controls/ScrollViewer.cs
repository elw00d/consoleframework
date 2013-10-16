using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    public class ScrollViewer : Control
    {
        public ScrollViewer( ) {
//            HorizontalAlignment = HorizontalAlignment.Stretch;
//            VerticalAlignment = VerticalAlignment.Stretch;
            AddHandler( MouseDownEvent, new MouseButtonEventHandler(OnMouseDown) );
        }

        private void OnMouseDown( object sender, MouseButtonEventArgs args ) {
            Point pos = args.GetPosition( this );
            if ( horizontalScrollVisible ) {
                if ( pos == new Point( 0, ActualHeight - 1 ) ) {
                    // left arrow clicked todo : учитывать наличие verticalScroll ! тогда можно еще на 1 сдвигать
                    if ( deltaX <= Content.RenderSize.Width - Content.RenderSlotRect.Size.Width ) {
                        deltaX++;
                        Invalidate( );
                    }
                }
                if ( pos ==
                     new Point( ActualWidth - ( 1 + ( verticalScrollVisible ? 1 : 0 ) ), ActualHeight - 1 ) ) {
                    // right arrow clicked
                    if ( deltaX > 0 ) {
                        deltaX--;
                        Invalidate( );
                    }
                }
            }
            if ( verticalScrollVisible ) {
                if ( pos == new Point( ActualWidth - 1, 0 ) ) {
                    // up arrow clicked
                }
                if ( pos ==
                     new Point( ActualWidth - 1, ActualHeight - ( 1 + ( horizontalScrollVisible ? 1 : 0 ) ) ) ) {
                    // down arrow clicked
                }
            }
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
            
            return new Size(
                Math.Min( verticalScrollVisible ? desiredSize.Width + 1 :desiredSize.Width, availableSize.Width),
                Math.Min( horizontalScrollVisible ? desiredSize.Height + 1 : desiredSize.Height, availableSize.Height)
            );
        }

        protected override Size ArrangeOverride(Size finalSize) {
            if ( Content == null ) return finalSize;
            Rect finalRect = new Rect( new Point( -deltaX, -deltaY ), 
                new Size( 
                    deltaX + Math.Max( 0, verticalScrollVisible ? finalSize.Width - 1 : finalSize.Width ), 
                    deltaY + Math.Max( 0, horizontalScrollVisible ? finalSize.Height - 1 : finalSize.Height ) )
                );
            // todo : для deltaY
            if (deltaX > Content.DesiredSize.Width - Math.Max(0, verticalScrollVisible ? finalSize.Width - 1 : finalSize.Width))
            {
                deltaX = 0;
                finalRect = new Rect(new Point(-deltaX, -deltaY),
                new Size(
                    deltaX + Math.Max(0, verticalScrollVisible ? finalSize.Width - 1 : finalSize.Width),
                    deltaY + Math.Max(0, horizontalScrollVisible ? finalSize.Height - 1 : finalSize.Height))
                );
            }
            Content.Arrange( finalRect );
            return new Size(Math.Min(verticalScrollVisible ? 1 + Content.DesiredSize.Width : Content.DesiredSize.Width, finalSize.Width),
                Math.Min( horizontalScrollVisible ? 1 + Content.DesiredSize.Height : Content.DesiredSize.Height, finalSize.Height));
        }

        public override void Render(RenderingBuffer buffer) {
            CHAR_ATTRIBUTES attr = (CHAR_ATTRIBUTES)Color.Attr(Color.DarkCyan, Color.DarkBlue);

            if ( horizontalScrollVisible ) {
                buffer.SetPixel(0, ActualHeight - 1, '\u25C4', attr); // ◄
                // оставляем дополнительный пиксель справа, если одновременно видны оба скроллбара
                int rightOffset = verticalScrollVisible ? 1 : 0;
                if ( ActualWidth > 2 + rightOffset ) {
                    buffer.FillRectangle(1, ActualHeight - 1, ActualWidth - (2 + rightOffset), 1, '\u2592', attr); // ▒
                }
                if ( ActualWidth > 1 + rightOffset ) {
                    buffer.SetPixel(ActualWidth - (1 + rightOffset), ActualHeight - 1, '\u25BA', attr); // ►
                }
            }
            if ( verticalScrollVisible ) {
                buffer.SetPixel(ActualWidth - 1, 0, '\u25B2', attr); // ▲
                // оставляем дополнительный пиксель снизу, если одновременно видны оба скроллбара
                int downOffset = horizontalScrollVisible ? 1 : 0;
                if ( ActualHeight > 2 + downOffset ) {
                    buffer.FillRectangle(ActualWidth - 1, 1, 1, ActualHeight - (2 + downOffset), '\u2592', attr); // ▒
                }
                if ( ActualHeight > 1 + downOffset ) {
                    buffer.SetPixel(ActualWidth - 1, ActualHeight - (1 + downOffset), '\u25BC', attr); // ▼
                }
            }
            if ( horizontalScrollVisible && verticalScrollVisible ) {
                buffer.SetPixel(ActualWidth - 1, ActualHeight - 1, '\u2518', attr); // ┘
            }
        }
    }
}
