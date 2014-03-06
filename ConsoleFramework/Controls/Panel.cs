using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Binding.Observables;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;

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
        }

        public Panel() {
            collection = new UIElementCollection(this);
            subscribe();
        }

        public Panel(Control parent) : base(parent) {
            subscribe();
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

        private readonly UIElementCollection collection;
        /// <summary>
        /// todo : rename to Children
        /// </summary>
        public UIElementCollection Content {
            get { return collection; }
        }

        public new void AddChild(Control control) {
            base.AddChild(control);
        }

        public void ClearChilds( ) {
            List< Control > children = new List< Control >(this.Children);
            foreach ( var child in children ) {
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
                foreach (Control child in Children) {
                    child.Measure(availableSize);
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
                    child.Measure(availableSize);
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
                    buffer.SetPixel(x, y, ' ', Attr.BACKGROUND_BLUE |
                        Attr.BACKGROUND_GREEN | Attr.BACKGROUND_RED | Attr.FOREGROUND_BLUE |
                        Attr.FOREGROUND_GREEN | Attr.FOREGROUND_RED | Attr.FOREGROUND_INTENSITY);
                }
            }
        }

        public override string ToString() {
            return "Panel";
        }
    }
}
