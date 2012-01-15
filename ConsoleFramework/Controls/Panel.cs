using System;
using System.Collections.Generic;
using ConsoleFramework.Core;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Контрол, который может состоять из других контролов.
    /// Позиционирует входящие в него контролы в соответствии с внутренним поведением панели и
    /// заданными свойствами дочерних контролов.
    /// Как и все контролы, связан с виртуальным канвасом.
    /// Может быть самым первым контролом программы (окно не может, к примеру, оно может существовать
    /// только в рамках хоста окон).
    /// </summary>
    public class Panel : Control {
        private readonly List<Control> children = new List<Control>();
        private readonly Dictionary<Control, Point> childrenPositions = new Dictionary<Control, Point>();

        public Panel() {
        }

        public Panel(PhysicalCanvas canvas) : base(canvas) {
        }

        public Panel(Control parent) : base(parent) {
        }

        public CHAR_ATTRIBUTES Background {
            get;
            set;
        }

        public void AddChild(Control control) {
            children.Add(control);
            control.canvas = new VirtualCanvas(control);
            control.Parent = this;
            //
            recalculateChildrenPositions();
        }

        protected override Size ArrangeOverride(Size finalSize) {
            base.ArrangeOverride(finalSize);
            //
            recalculateChildrenPositions();
            return finalSize;
        }

        private void recalculateChildrenPositions() {
            foreach (Control child in children) {
                child.Measure(new Size(this.ActualWidth / children.Count, this.ActualHeight / children.Count));
                //
            }
            //
            int heightUsed = 0;
            for (int i = 0; i < children.Count; i++) {
                Control child = children[i];
                int height = this.ActualHeight/children.Count;
                if (height + heightUsed > this.ActualHeight || i + 1 == children.Count) {
                    height = this.ActualHeight - heightUsed;
                }
                Size finalSize = new Size(this.ActualWidth, height);
                child.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
                //
                if (!childrenPositions.ContainsKey(child)) {
                    childrenPositions.Add(child, new Point(0, heightUsed));
                } else {
                    childrenPositions[child] = new Point(0, heightUsed);
                }
                heightUsed += height;
            }
            //
            if (heightUsed != this.ActualHeight) {
                throw new InvalidOperationException("Not all available height is used in panel.");
            }
        }

        public override void Draw() {
            for (int x = 0; x < ActualWidth; ++x) {
                for (int y = 0; y < ActualHeight; ++y) {
                    canvas.SetPixel(x + ActualOffset.X, y + ActualOffset.Y, 'x', CHAR_ATTRIBUTES.BACKGROUND_BLUE |
                        CHAR_ATTRIBUTES.BACKGROUND_GREEN | CHAR_ATTRIBUTES.BACKGROUND_RED | CHAR_ATTRIBUTES.FOREGROUND_BLUE |
                        CHAR_ATTRIBUTES.FOREGROUND_GREEN | CHAR_ATTRIBUTES.FOREGROUND_RED | CHAR_ATTRIBUTES.FOREGROUND_INTENSITY);
                }
            }
            //
            foreach (Control child in children) {
                child.Draw();
            }
        }

        public override Point GetChildOffset(Control control) {
            return childrenPositions[control];
        }
    }
}
