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
            MinWidth = 0;
        }

        public Control(PhysicalCanvas canvas) {
            MinWidth = 0;
            //
            this.canvas = new VirtualCanvas(this, canvas, 0, 0);
        }

        public Control(Control parent) {
            MinWidth = 0;
            Parent = parent;
            canvas = new VirtualCanvas(this);
        }

        public Point ActualOffset {
            get {
                return Parent != null ? Parent.GetChildOffset(this) : new Point(0, 0);
            }
        }

        public bool LayoutIsValid {
            get;
            set;
        }

        public int ActualWidth {
            get;
            protected set;
        }

        public int ActualHeight {
            get;
            protected set;
        }

        public int MinWidth {
            get;
            set;
        }

        private int maxWidth = int.MaxValue;
        public int MaxWidth {
            get {
                return maxWidth;
            }
            set {
                maxWidth = value;
            }
        }

        public int MinHeight {
            get;
            set;
        }

        private int maxHeight = int.MaxValue;
        public int MaxHeight {
            get {
                return maxHeight;
            }
            set {
                maxHeight = value;
            }
        }

        public int? Width {
            get;
            set;
        }

        public int? Height {
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

        public Size DesiredSize {
            get;
            private set;
        }

        private struct MinMax
        {
            /// <summary>
            /// Определяет реальные констрейнты для текущих значений MinHeight/MaxHeight, MinWidth/MaxWidth
            /// и Width/Height. Min-значения не могут быть null, по дефолту равны нулю, также не могут быть int.MaxValue.
            /// Max-значения тоже не могут быть null, по дефолту равны int.MaxValue.
            /// Width и Height могут быть не заданы - в этом случае они контрол будет занимать как можно большее
            /// доступное пространство.
            /// В случае конфликта приоритет имеет Min-property, затем явно заданное значение (Width или Height),
            /// и в последнюю очередь играет роль Max-property.
            /// </summary>
            internal MinMax(int minHeight, int maxHeight, int minWidth, int maxWidth, int? width, int? height) {
                this.maxHeight = maxHeight;
                this.minHeight = minHeight;
                int? l = height;

                int tmp_height = l ?? int.MaxValue;
                this.maxHeight = Math.Max(Math.Min(tmp_height, this.maxHeight), this.minHeight);

                tmp_height = l ?? 0;
                this.minHeight = Math.Max(Math.Min(this.maxHeight, tmp_height), this.minHeight);

                this.maxWidth = maxWidth;
                this.minWidth = minWidth;
                l = width;

                int tmp_width = l ?? int.MaxValue;
                this.maxWidth = Math.Max(Math.Min(tmp_width, this.maxWidth), this.minWidth);

                tmp_width = l ?? 0;
                this.minWidth = Math.Max(Math.Min(this.maxWidth, tmp_width), this.minWidth);
            }

            internal readonly int minWidth;
            internal readonly int maxWidth;
            internal readonly int minHeight;
            internal readonly int maxHeight;
        }
        
        public void Measure(Size availableSize) {
            if (LayoutIsValid)
                // nothing to do
                return;

            // apply margin
            Thickness margin = Margin;
            int marginWidth = margin.Left + margin.Right;
            int marginHeight = margin.Top + margin.Bottom;

            //  parent size is what parent want us to be
            Size frameworkAvailableSize = new Size(
                Math.Max(availableSize.Width - marginWidth, 0),
                Math.Max(availableSize.Height - marginHeight, 0));

            // apply min/max/currentvalue constraints
            MinMax mm = new MinMax(MinHeight, MaxHeight, MinWidth, MaxWidth, Height, Width);

            frameworkAvailableSize.Width = Math.Max(mm.minWidth, Math.Min(frameworkAvailableSize.Width, mm.maxWidth));
            frameworkAvailableSize.Height = Math.Max(mm.minHeight, Math.Min(frameworkAvailableSize.Height, mm.maxHeight));

            Size desiredSize = MeasureOverride(frameworkAvailableSize);
            if (desiredSize.Width == int.MaxValue || desiredSize.Height == int.MaxValue) {
                throw new InvalidOperationException("MeasureOverride should not return int.MaxValue even for" +
                                                    "availableSize = {int.MaxValue, int.MaxValue} argument.");
            }

            //  maximize desiredSize with user provided min size
            desiredSize = new Size(
                Math.Max(desiredSize.Width, mm.minWidth),
                Math.Max(desiredSize.Height, mm.minHeight));

            //here is the "true minimum" desired size - the one that is
            //for sure enough for the control to render its content.
            Size unclippedDesiredSize = desiredSize;

            // User-specified max size starts to "clip" the control here. 
            //Starting from this point desiredSize could be smaller then actually
            //needed to render the whole control
            if (desiredSize.Width > mm.maxWidth) {
                desiredSize.Width = mm.maxWidth;
            }

            if (desiredSize.Height > mm.maxHeight) {
                desiredSize.Height = mm.maxHeight;
            }

            //  because of negative margins, clipped desired size may be negative.
            //  need to keep it as doubles for that reason and maximize with 0 at the 
            //  very last point - before returning desired size to the parent. 
            int clippedDesiredWidth = desiredSize.Width + marginWidth;
            int clippedDesiredHeight = desiredSize.Height + marginHeight;

            // In overconstrained scenario, parent wins and measured size of the child,
            // including any sizes set or computed, can not be larger then
            // available size. We will clip the guy later. 
            if (clippedDesiredWidth > availableSize.Width) {
                clippedDesiredWidth = availableSize.Width;
            }

            if (clippedDesiredHeight > availableSize.Height) {
                clippedDesiredHeight = availableSize.Height;
            }

            //  Note: unclippedDesiredSize is needed in ArrangeCore,
            //  because due to the layout protocol, arrange should be called 
            //  with constraints greater or equal to child's desired size
            //  returned from MeasureOverride.
            m_unclippedDesiredSize = unclippedDesiredSize;

            DesiredSize = new Size(Math.Max(0, clippedDesiredWidth), Math.Max(0, clippedDesiredHeight)); 
        }

        private Size m_unclippedDesiredSize;

        protected virtual Size MeasureOverride(Size availableSize) {
            return new Size(0, 0);
        }

        public void Arrange(Rect finalRect) {
            if (LayoutIsValid)
                return;
            //
            Size returnedSize = ArrangeOverride(m_unclippedDesiredSize);
            //
            ActualWidth = Math.Min(finalRect.Width, returnedSize.Width);
            ActualHeight = Math.Min(finalRect.Height, returnedSize.Height);
            //
            LayoutIsValid = true;
        }

        protected virtual Size ArrangeOverride(Size finalSize) {
            return finalSize;
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

        public void Invalidate() {
            //
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
