using System;
using System.Collections.Generic;
using ConsoleFramework.Core;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right,
        Stretch
    }


    public enum VerticalAlignment
    {
        Top,
        Center,
        Bottom,
        Stretch
    }



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

        public Vector ActualOffset {
            get;
            private set;
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

            // If LayoutConstrained==true (parent wins in layout),
            // we might get finalRect.Size smaller then UnclippedDesiredSize. 
            // Stricltly speaking, this may be the case even if LayoutConstrained==false (child wins),
            // since who knows what a particualr parent panel will try to do in error.
            // In this case we will not actually arrange a child at a smaller size,
            // since the logic of the child does not expect to receive smaller size 
            // (if it coudl deal with smaller size, it probably would accept it in MeasureOverride)
            // so lets replace the smaller arreange size with UnclippedDesiredSize 
            // and then clip the guy later. 
            // We will use at least UnclippedDesiredSize to compute arrangeSize of the child, and
            // we will use layoutSlotSize to compute alignments - so the bigger child can be aligned within 
            // smaller slot.

            // This is computed on every ArrangeCore. Depending on LayoutConstrained, actual clip may apply or not
            // todo : introduce a property ?
            bool NeedsClipBounds = false;

            // Start to compute arrange size for the child. 
            // It starts from layout slot or deisred size if layout slot is smaller then desired, 
            // and then we reduce it by margins, apply Width/Height etc, to arrive at the size
            // that child will get in its ArrangeOverride. 
            Size arrangeSize = finalRect.Size;

            Thickness margin = Margin;
            int marginWidth = margin.Left + margin.Right;
            int marginHeight = margin.Top + margin.Bottom;

            arrangeSize.Width = Math.Max(0, arrangeSize.Width - marginWidth);
            arrangeSize.Height = Math.Max(0, arrangeSize.Height - marginHeight);
            
            // Next, compare against unclipped, transformed size.
            Size unclippedDesiredSize = m_unclippedDesiredSize;

            if (arrangeSize.Width < unclippedDesiredSize.Width) {
                NeedsClipBounds = true;
                arrangeSize.Width = unclippedDesiredSize.Width;
            }

            if (arrangeSize.Height < unclippedDesiredSize.Height) {
                NeedsClipBounds = true;
                arrangeSize.Height = unclippedDesiredSize.Height;
            }

            // Alignment==Stretch --> arrange at the slot size minus margins
            // Alignment!=Stretch --> arrange at the unclippedDesiredSize 
            if (HorizontalAlignment != HorizontalAlignment.Stretch) {
                arrangeSize.Width = unclippedDesiredSize.Width;
            }

            if (VerticalAlignment != VerticalAlignment.Stretch) {
                arrangeSize.Height = unclippedDesiredSize.Height;
            }

            MinMax mm = new MinMax(MinHeight, MaxHeight, MinWidth, MaxWidth, Height, Width);

            //we have to choose max between UnclippedDesiredSize and Max here, because
            //otherwise setting of max property could cause arrange at less then unclippedDS.
            //Clipping by Max is needed to limit stretch here 
            int effectiveMaxWidth = Math.Max(unclippedDesiredSize.Width, mm.maxWidth);
            if (effectiveMaxWidth < arrangeSize.Width) {
                NeedsClipBounds = true;
                arrangeSize.Width = effectiveMaxWidth;
            }

            int effectiveMaxHeight = Math.Max(unclippedDesiredSize.Height, mm.maxHeight);
            if (effectiveMaxHeight < arrangeSize.Height) {
                NeedsClipBounds = true;
                arrangeSize.Height = effectiveMaxHeight;
            }
            
            //Size oldRenderSize = RenderSize;
            Size innerInkSize = ArrangeOverride(arrangeSize);

            //Here we use un-clipped InkSize because element does not know that it is
            //clipped by layout system and it shoudl have as much space to render as
            //it returned from its own ArrangeOverride 
            RenderSize = innerInkSize;

            //clippedInkSize differs from InkSize only what MaxWidth/Height explicitly clip the
            //otherwise good arrangement. For ex, DS<clientSize but DS>MaxWidth - in this
            //case we should initiate clip at MaxWidth and only show Top-Left portion 
            //of the element limited by Max properties. It is Top-left because in case when we
            //are clipped by container we also degrade to Top-Left, so we are consistent. 
            Size clippedInkSize = new Size(Math.Min(innerInkSize.Width, mm.maxWidth),
                                           Math.Min(innerInkSize.Height, mm.maxHeight));

            //remember we have to clip if Max properties limit the inkSize 
            NeedsClipBounds |=
                    (clippedInkSize.Width < innerInkSize.Width)
                || (clippedInkSize.Height < innerInkSize.Height);

            //Note that inkSize now can be bigger then layoutSlotSize-margin (because of layout 
            //squeeze by the parent or LayoutConstrained=true, which clips desired size in Measure). 

            // The client size is the size of layout slot decreased by margins. 
            // This is the "window" through which we see the content of the child.
            // Alignments position ink of the child in this "window".
            // Max with 0 is neccessary because layout slot may be smaller then unclipped desired size.
            Size clientSize = new Size(Math.Max(0, finalRect.Width - marginWidth),
                                    Math.Max(0, finalRect.Height - marginHeight));

            //remember we have to clip if clientSize limits the inkSize
            NeedsClipBounds |=
                    (clientSize.Width < clippedInkSize.Width)
                || (clientSize.Height < clippedInkSize.Height);

            Vector offset = ComputeAlignmentOffset(clientSize, clippedInkSize);

            offset.X += finalRect.X + margin.Left;
            offset.Y += finalRect.Y + margin.Top;

            //SetLayoutOffset(offset, oldRenderSize);
            if (!this.ActualOffset.Equals(offset)) {
                this.ActualOffset = offset;
            }

            LayoutIsValid = true;
        }

        public HorizontalAlignment HorizontalAlignment {
            get;
            set;
        }

        public VerticalAlignment VerticalAlignment {
            get;
            set;
        }

        public Size RenderSize {
            get;
            private set;
        }

        private Vector ComputeAlignmentOffset(Size clientSize, Size inkSize) {
            Vector vector = new Vector();
            HorizontalAlignment horizontalAlignment = this.HorizontalAlignment;
            VerticalAlignment verticalAlignment = this.VerticalAlignment;
            if ((horizontalAlignment == HorizontalAlignment.Stretch) && (inkSize.Width > clientSize.Width)) {
                horizontalAlignment = HorizontalAlignment.Left;
            }
            if ((verticalAlignment == VerticalAlignment.Stretch) && (inkSize.Height > clientSize.Height)) {
                verticalAlignment = VerticalAlignment.Top;
            }
            switch (horizontalAlignment) {
                case HorizontalAlignment.Center:
                case HorizontalAlignment.Stretch:
                    vector.X = (clientSize.Width - inkSize.Width) / 2;
                    break;

                default:
                    if (horizontalAlignment == HorizontalAlignment.Right) {
                        vector.X = clientSize.Width - inkSize.Width;
                    } else {
                        vector.X = 0;
                    }
                    break;
            }
            switch (verticalAlignment) {
                case VerticalAlignment.Center:
                case VerticalAlignment.Stretch:
                    vector.Y = (clientSize.Height - inkSize.Height) / 2;
                    return vector;

                case VerticalAlignment.Bottom:
                    vector.Y = clientSize.Height - inkSize.Height;
                    return vector;
            }
            vector.Y = 0;
            return vector;
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
                        Vector offset = currentControl.ActualOffset;
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
                        Vector offset = currentControl.ActualOffset;
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
                    Vector offset = currentControl.ActualOffset;
                    point.Offset(offset.X, offset.Y);
                    currentControl = currentControl.Parent;
                }
                // traverse back from dest to common ancestor
                currentControl = dest;
                while (currentControl != ancestor) {
                    Vector offset = currentControl.ActualOffset;
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
