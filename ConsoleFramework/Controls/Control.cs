using System;
using System.Collections.Generic;
using ConsoleFramework.Core;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Base class for all controls.
    /// </summary>
    public class Control {
        internal VirtualCanvas canvas;

        public string Name {
            get;
            set;
        }

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
            get {
                return Parent != null ? Parent.GetChildOffset(this) : new Point(0, 0);
            }
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

        public void Arrange(Size finalSize) {
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

        public virtual Point GetChildOffset(Control control) {
            throw new NotImplementedException();
        }

        public static Point TranslatePoint(Control source, Point point, Control dest) {
            if (source == null || dest == null) {
                if (source == null && dest != null) {
                    // translating raw point (absolute coords) into relative to dest control point
                    Control currentControl = dest;
                    for (;;) {
                        Point offset = currentControl.ActualOffset;
                        point.Offset(-offset.X, -offset.Y);
                        if (currentControl.Parent == null) {
                            break;
                        }
                        currentControl = currentControl.Parent;
                    }
                    if (currentControl.canvas == null) {
                        throw new InvalidOperationException("Root of dest control doesn't linked to canvas.");
                    }
                    return point;
                } else if (source != null && dest == null) {
                    // translating point relative to source into absolute coords
                    Control currentControl = source;
                    for (;;) {
                        Point offset = currentControl.ActualOffset;
                        point.Offset(offset.X, offset.Y);
                        if (currentControl.Parent == null)
                            break;
                        currentControl = currentControl.Parent;
                    }
                    if (currentControl.canvas == null)
                        throw new InvalidOperationException("Root of source control doesn't linked to canvas.");
                    return point;
                } else {
                    // both source and dest are null - we shouldn't to do anything
                    return point;
                }
            } else {
                // find common ancestor
                Control ancestor = FindCommonAncestor(source, dest);
                // traverse back from source to common ancestor
                Control currentControl = source;
                while (currentControl != ancestor) {
                    Point offset = currentControl.ActualOffset;
                    point.Offset(offset.X, offset.Y);
                    currentControl = currentControl.Parent;
                }
                // traverse back from dest to common ancestor
                currentControl = dest;
                while (currentControl != ancestor) {
                    Point offset = currentControl.ActualOffset;
                    point.Offset(-offset.X, -offset.Y);
                    currentControl = currentControl.Parent;
                }
                return point;
            }
        }

        /// <summary>
        /// Returns common ancestor for specified controls pair.
        /// If there are no common ancestor found, null will be returned.
        /// But this situation is impossible because there are only one main control in application.
        /// </summary>
        public static Control FindCommonAncestor(Control a, Control b) {
            if (null == a)
                throw new ArgumentNullException("a");
            if (null == b)
                throw new ArgumentNullException("b");
            //
            List<Control> visited = new List<Control>();
            Control refA = a;
            Control refB = b;
            bool f = true;
            for (;;) {
                if (null == refA.canvas || null == refB.canvas)
                    throw new InvalidOperationException("Found control that doesn't linked to canvas.");
                if (refA == refB)
                    return refA;
                if (visited.Contains(refB))
                    return refB;
                if (visited.Contains(refA))
                    return refA;
                if (refA.Parent == null && refB.Parent == null)
                    return null;
                if (f) {
                    if (refA.Parent != null) {
                        visited.Add(refA);
                        refA = refA.Parent;
                    }
                } else {
                    if (refB.Parent != null) {
                        visited.Add(refB);
                        refB = refB.Parent;
                    }
                }
                f = !f;
            }
        }

        public override string ToString() {
            return string.Format("Control: {0}", Name);
        }
    }
}
