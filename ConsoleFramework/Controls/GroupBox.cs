using System;
using ConsoleFramework.Core;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Оборачивает указанный контрол рамкой с заголовком.
    /// </summary>
    public class GroupBox : Control
    {
        private string title;
        public string Title {
            get { return title; }
            set {
                if ( title != value ) {
                    title = value;
                    Invalidate(  );
                }
            }
        }

        private Control content;
        public Control Content {
            get { return content; }
            set {
                if ( content != value ) {
                    if (content != null) RemoveChild( content );
                    content = value;
                    AddChild(content);
                    Invalidate(  );
                }
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size contentSize = Size.Empty;
            if ( content != null ) {
                content.Measure( new Size(int.MaxValue, int.MaxValue ));
                contentSize = content.DesiredSize;
            }
            Size needSize = new Size(
                Math.Max( contentSize.Width + 2, (title??string.Empty).Length + 4 ),
                contentSize.Height + 2
                );
            Size constrainedSize = new Size(
                Math.Min( needSize.Width, availableSize.Width ),
                Math.Min( needSize.Height, availableSize.Height )
                );
            if ( needSize != constrainedSize && content != null ) {
                // если контрол вместе с содержимым не помещается в availableSize,
                // то мы оставляем содержимому меньше места, чем ему хотелось бы,
                // и поэтому повторным вызовом Measure должны установить его реальные размеры,
                // которые будут использованы при размещении
                content.Measure( new Size(
                    Math.Max( 0, constrainedSize.Width - 2),
                    Math.Max(0, constrainedSize.Height - 2)
                    ));
            }
            return constrainedSize;
        }

        protected override Size ArrangeOverride(Size finalSize) {
            if ( null == content)
                return finalSize;
            Rect contentRect = new Rect(1, 1, 
                Math.Max( 0, finalSize.Width - 2), 
                Math.Max(0, finalSize.Height - 2));
            content.Arrange( contentRect );
            return finalSize;
        }

        public override void Render(RenderingBuffer buffer) {
            Attr attr = Colors.Blend( Color.Black, Color.DarkGreen );

            // прозрачный фон для рамки
            buffer.SetOpacityRect( 0, 0, ActualWidth, ActualHeight, 3 );
            // полностью прозрачный внутри
            if (ActualWidth > 2 && ActualHeight > 2)
                buffer.SetOpacityRect( 1, 1, ActualWidth-2, ActualHeight-2, 2 );
            // title
            int titleRenderedWidth = 0;
            if ( !string.IsNullOrEmpty( title ) )
                titleRenderedWidth = RenderString( title, buffer, 2, 0, ActualWidth - 4, attr );
            // upper border
            for ( int x = 0; x < ActualWidth; x++ ) {
                char? c = null;
                if ( x == 0 )
                    c = '\u250C';
                else if (x == ActualWidth - 1)
                    c = '\u2510';
                else if (x == 1 || x == 2 + titleRenderedWidth)
                    c = ' ';
                else if ( x > 2 + titleRenderedWidth && x < ActualWidth - 1 )
                    c = '\u2500';
                if (c != null)
                    buffer.SetPixel( x, 0, c.Value, attr );
            }
            // left border
            if (ActualHeight > 2)
                buffer.FillRectangle( 0, 1, 1, ActualHeight - 2, '\u2502', attr );
            if (ActualHeight > 1)
                buffer.SetPixel( 0, ActualHeight-1, '\u2514', attr );
            // right border
            if ( ActualWidth > 1 ) {
                if (ActualHeight > 2)
                    buffer.FillRectangle( ActualWidth - 1, 1, 1, ActualHeight - 2, '\u2502', attr );
                if (ActualHeight > 1)
                    buffer.SetPixel( ActualWidth-1, ActualHeight-1, '\u2518', attr );
            }
            // bottom border
            if ( ActualHeight > 1 && ActualWidth > 2 ) {
                buffer.FillRectangle(1, ActualHeight - 1, ActualWidth - 2, 1, '\u2500', attr);
            }
        }
    }
}
