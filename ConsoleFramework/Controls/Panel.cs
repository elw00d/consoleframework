using System.Collections.Generic;
using ConsoleFramework.Core;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;
using Xaml;

namespace ConsoleFramework.Controls
{
    public enum Orientation {
        Horizontal,
        Vertical
    }

    /// <summary>
    /// Контрол, который может состоять из других контролов.
    /// Позиционирует входящие в него контролы в соответствии с внутренним поведением панели и
    /// заданными свойствами дочерних контролов.
    /// Как и все контролы, связан с виртуальным канвасом.
    /// Может быть самым первым контролом программы (окно не может, к примеру, оно может существовать
    /// только в рамках хоста окон).
    /// </summary>
    [ContentProperty("Children")]
    public class Panel : Control {
        public Panel() {
            children = new UIElementCollection(this);
        }

        public Attr Background {
            get;
            set;
        }

        private Orientation orientation = Orientation.Vertical;

        public Orientation Orientation {
            get {
                return orientation;
            }
            set {
                if (orientation != value) {
                    orientation = value;
                    this.Invalidate();
                }
            }
        }

        private readonly UIElementCollection children;
        public new UIElementCollection Children {
            get { return children; }
        }

        public new void AddChild(Control control) {
            base.AddChild(control);
        }

        public void ClearChilds( ) {
            foreach ( var child in new List< Control >(base.Children) ) {
                RemoveChild(child);
            }
        }

        /// <summary>
        /// Размещает элементы вертикально, самым простым методом.
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize) {
            if (orientation == Orientation.Vertical) {
                int totalHeight = 0;
                int maxWidth = 0;
                foreach (Control child in base.Children) {
                    child.Measure(availableSize);
                    totalHeight += child.DesiredSize.Height;
                    if (child.DesiredSize.Width > maxWidth) {
                        maxWidth = child.DesiredSize.Width;
                    }
                }
                foreach (Control child in base.Children) {
                    child.Measure(new Size(maxWidth, child.DesiredSize.Height));
                }
                return new Size(maxWidth, totalHeight);
            } else {
                int totalWidth = 0;
                int maxHeight = 0;
                foreach (Control child in base.Children) {
                    child.Measure(availableSize);
                    totalWidth += child.DesiredSize.Width;
                    if (child.DesiredSize.Height > maxHeight)
                        maxHeight = child.DesiredSize.Height;
                }
                foreach (Control child in base.Children)
                    child.Measure(new Size(child.DesiredSize.Width, maxHeight));
                return new Size(totalWidth, maxHeight);
            }
        }
        
        protected override Size ArrangeOverride(Size finalSize) {
            if (orientation == Orientation.Vertical) {
                int totalHeight = 0;
                int maxWidth = 0;
                foreach (Control child in base.Children) {
                    if (child.DesiredSize.Width > maxWidth)
                        maxWidth = child.DesiredSize.Width;
                }
                foreach (Control child in base.Children) {
                    int y = totalHeight;
                    int height = child.DesiredSize.Height;
                    child.Arrange(new Rect(0, y, maxWidth, height));
                    totalHeight += height;
                }
                return finalSize;
            } else {
                int totalWidth = 0;
                int maxHeight = 0;
                foreach (Control child in base.Children) {
                    if (child.DesiredSize.Height > maxHeight)
                        maxHeight = child.DesiredSize.Height;
                }
                foreach (Control child in base.Children) {
                    int x = totalWidth;
                    int width = child.DesiredSize.Width;
                    child.Arrange(new Rect(x, 0, width, maxHeight));
                    totalWidth += width;
                }
                return finalSize;
            }
        }

        /// <summary>
        /// Рисует исключительно себя - просто фон.
        /// </summary>
        /// <param name="buffer"></param>
        public override void Render(RenderingBuffer buffer) {
            for (int x = 0; x < ActualWidth; ++x) {
                for (int y = 0; y < ActualHeight; ++y) {
                    buffer.SetPixel(x, y, ' ', Attr.BACKGROUND_BLUE |
                        Attr.BACKGROUND_GREEN | Attr.BACKGROUND_RED | Attr.FOREGROUND_BLUE |
                        Attr.FOREGROUND_GREEN | Attr.FOREGROUND_RED | Attr.FOREGROUND_INTENSITY);
                }
            }
        }
    }
}
