using System.Collections.Generic;
using System.Diagnostics;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;

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
    public class Panel : Control {
        private void subscribe() {
//            AddHandler(PreviewKeyDownEvent, new KeyEventHandler(Panel_PreviewKeyDown), true);
        }

        public Panel() {
            subscribe();
        }

        public Panel(Control parent) : base(parent) {
            subscribe();
        }

        public CHAR_ATTRIBUTES Background {
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

        public new void AddChild(Control control) {
            base.AddChild(control);
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
                foreach (Control child in Children) {
                    child.Measure(Size.MaxSize);
                    // todo : fix if child returns big size > availableSize
                    totalHeight += child.DesiredSize.Height;
                    if (child.DesiredSize.Width > maxWidth) {
                        maxWidth = child.DesiredSize.Width;
                    }
                }
                foreach (Control child in Children) {
                    child.Measure(new Size(maxWidth, child.DesiredSize.Height));
                }
                return new Size(maxWidth, totalHeight);
            } else {
                int totalWidth = 0;
                int maxHeight = 0;
                foreach (Control child in Children) {
                    child.Measure(Size.MaxSize);
                    totalWidth += child.DesiredSize.Width;
                    if (child.DesiredSize.Height > maxHeight)
                        maxHeight = child.DesiredSize.Height;
                }
                foreach (Control child in Children)
                    child.Measure(new Size(child.DesiredSize.Width, maxHeight));
                return new Size(totalWidth, maxHeight);
            }
        }
        
        protected override Size ArrangeOverride(Size finalSize) {
            if (orientation == Orientation.Vertical) {
                int totalHeight = 0;
                int maxWidth = 0;
                foreach (Control child in Children) {
                    if (child.DesiredSize.Width > maxWidth)
                        maxWidth = child.DesiredSize.Width;
                }
                foreach (Control child in Children) {
                    int y = totalHeight;
                    int height = child.DesiredSize.Height;
                    child.Arrange(new Rect(0, y, maxWidth, height));
                    totalHeight += height;
                }
                return finalSize;
            } else {
                int totalWidth = 0;
                int maxHeight = 0;
                foreach (Control child in Children) {
                    if (child.DesiredSize.Height > maxHeight)
                        maxHeight = child.DesiredSize.Height;
                }
                foreach (Control child in Children) {
                    int x = totalWidth;
                    int width = child.DesiredSize.Width;
                    child.Arrange(new Rect(x, 0, width, maxHeight));
                    totalWidth += width;
                }
                return finalSize;
            }
        }

//        public void Panel_PreviewKeyDown(object sender, KeyEventArgs args) {
//            if (args.wVirtualKeyCode == 09) {
//                Debug.WriteLine("Tab");
//                List<Control> childs = GetChildrenOrderedByZIndex();
//                if (childs.Count > 0) {
//                    int findIndex = childs.FindIndex(c => c.HasLogicalFocus);
//                    if (findIndex == -1)
//                        childs[0].SetFocus();
//                    else {
//                        Control child = childs[(findIndex + 1)%childs.Count];
//                        child.SetFocus();
//                    }
//                }
//                args.Handled = true;
//            }
//        }

        /// <summary>
        /// Рисует исключительно себя - просто фон.
        /// </summary>
        /// <param name="buffer"></param>
        public override void Render(RenderingBuffer buffer) {
            for (int x = 0; x < ActualWidth; ++x) {
                for (int y = 0; y < ActualHeight; ++y) {
                    buffer.SetPixel(x, y, ' ', CHAR_ATTRIBUTES.BACKGROUND_BLUE |
                        CHAR_ATTRIBUTES.BACKGROUND_GREEN | CHAR_ATTRIBUTES.BACKGROUND_RED | CHAR_ATTRIBUTES.FOREGROUND_BLUE |
                        CHAR_ATTRIBUTES.FOREGROUND_GREEN | CHAR_ATTRIBUTES.FOREGROUND_RED | CHAR_ATTRIBUTES.FOREGROUND_INTENSITY);
                }
            }
        }

        public override string ToString() {
            return "Panel";
        }
    }
}
