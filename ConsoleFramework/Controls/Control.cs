using System;
using ConsoleFramework.Core;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Base class for all controls.
    /// </summary>
    public class Control {
        internal VirtualCanvas canvas;

        public Control Parent {
            get;
            set;
        }

        public Control() {
        }

        public Control(PhysicalCanvas canvas) {
            //
            this.canvas = new VirtualCanvas(this, canvas, 0, 0);
        }

        public Control(Control parent) {
            Parent = parent;
            canvas = new VirtualCanvas(this);
        }

        public Point ActualOffset {
            get;
            private set;
        }

        public int ActualWidth {
            get;
            protected set;
        }

        public int ActualHeight {
            get;
            protected set;
        }

        public int Width {
            get;
            set;
        }

        public int Height {
            get;
            set;
        }

        public Thickness Margin {
            get;
            set;
        }

        public bool Visible {
            get;
            set;
        }

        internal Size DesiredSize {
            get;
            private set;
        }

        internal void Measure(Size availableSize) {
            DesiredSize = availableSize;
        }

        protected virtual Size MeasureOverride(Size availableSize) {
            return Size.Empty;
        }

        internal void Arrange(Size finalSize) {
            ActualWidth = finalSize.width;
            ActualHeight = finalSize.height;
            //
            ArrangeOverride(finalSize);
        }

        protected virtual void ArrangeOverride(Size finalSize) {
            //
        }

        public virtual void Draw(int actualLeft, int actualTop, int actualWidth, int actualHeight) {
            //
            if (null == canvas) {
                throw new InvalidOperationException("Control doesn't linked to any canvas. Set the parent control" +
                                                    " or set this control as main application control.");
            }
            //
        }

        public virtual void HandleEvent(INPUT_RECORD inputRecord) {
            //
        }

        public virtual Point GetChildPoint(Control control) {
            throw new NotImplementedException();
        }
    }
}
