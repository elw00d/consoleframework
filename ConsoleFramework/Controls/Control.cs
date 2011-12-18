using System;
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

        public int ActualLeft {
            get;
            private set;
        }

        public int ActualTop {
            get;
            private set;
        }

        public int ActualWidth {
            get;
            private set;
        }

        public int ActualHeight {
            get;
            private set;
        }

        public virtual void Draw(int actualLeft, int actualTop, int actualWidth, int actualHeight) {
            //
            if (null == canvas) {
                throw new InvalidOperationException("Control doesn't linked to any canvas. Set the parent control" +
                                                    " or set this control as main application control.");
            }
            //
            ActualLeft = actualLeft;
            ActualTop = actualTop;
            ActualWidth = actualWidth;
            ActualHeight = actualHeight;
        }

        public virtual void HandleEvent(INPUT_RECORD inputRecord) {
            //
        }
    }
}
