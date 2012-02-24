using System;
using System.Collections.Generic;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;

namespace ConsoleFramework
{
    public sealed class Renderer
    {
        // buffers containing only control rendering representation itself
        private readonly Dictionary<Control, RenderingBuffer> buffers = new Dictionary<Control, RenderingBuffer>();
        // buffers containing full control render (with children render applied)
        private readonly Dictionary<Control, RenderingBuffer> fullBuffers = new Dictionary<Control, RenderingBuffer>();

        private readonly Queue<Control> invalidatedControls = new Queue<Control>();

        public void AddControlToInvalidationQueue(Control control) {
            if (null == control) {
                throw new ArgumentNullException("control");
            }
            invalidatedControls.Enqueue(control);
        }

        public void InvalidateLayout() {
            Control control;
            while ((control = invalidatedControls.Dequeue()) != null) {
                updateLayout(control);
            }
        }

        private void updateLayout(Control control) {
            LayoutInfo lastLayoutInfo = control.lastLayoutInfo;
            bool needUpdateParentLayout = true;
            if (lastLayoutInfo.validity != LayoutValidity.Nothing) {
                control.Measure(lastLayoutInfo.measureArgument);
                if (lastLayoutInfo.desiredSize == control.DesiredSize) {
                    needUpdateParentLayout = false;
                }
            }
            if (needUpdateParentLayout && control.Parent != null) {
                // mark the parent control for invalidation too
                control.Parent.Invalidate();
            }
        }

        private RenderingBuffer getOrCreateBufferForControl(Control control) {
            RenderingBuffer value;
            if (buffers.TryGetValue(control, out value)) {
                return value;
            } else {
                RenderingBuffer buffer = new RenderingBuffer(control.ActualWidth, control.ActualHeight);
                buffers.Add(control, buffer);
                return buffer;
            }
        }

        private RenderingBuffer getOrCreateFullBufferForControl(Control control) {
            RenderingBuffer value;
            if (fullBuffers.TryGetValue(control, out value)) {
                return value;
            } else {
                RenderingBuffer buffer = new RenderingBuffer(control.ActualWidth, control.ActualHeight);
                fullBuffers.Add(control, buffer);
                return buffer;
            }
        }

        public void Render(Control rootElement, PhysicalCanvas canvas, Rect rect) {
            if ((uint) rootElement.LayoutValidity < (uint) LayoutValidity.MeasureAndArrange) {
                // measuring all visual elements tree
                rootElement.Measure(rect.Size);
                rootElement.Arrange(rect);
            }
            //
            RenderingBuffer buffer = UpdateRender(rootElement);
            buffer.CopyToPhysicalCanvas(canvas, rect);
        }

        /// <summary>
        /// Updates the rendering buffers for specified control if need, and returns
        /// buffer with full rendered control content (including its children).
        /// </summary>
        public RenderingBuffer UpdateRender(Control control) {
            RenderingBuffer buffer = getOrCreateBufferForControl(control);
            RenderingBuffer fullBuffer = getOrCreateFullBufferForControl(control);
            //
            if ((uint) control.LayoutValidity < (uint) LayoutValidity.Render) {
                if ((uint)control.LayoutValidity < (uint)LayoutValidity.MeasureAndArrange) {
                    throw new NotSupportedException("You should invalidate a layout state of control before call render.");
                }
                control.Render(buffer);
                //
                fullBuffer.CopyFrom(buffer);
                foreach (Control child in control.children) {
                    RenderingBuffer fullChildBuffer = UpdateRender(child);
                    fullBuffer.ApplyChild(fullChildBuffer, child.ActualOffset, child.RenderSlotRect, child.LayoutClip);
                }
                //
                control.LayoutValidity = LayoutValidity.FullRender;
            }
            return fullBuffer;
        }
    }
}
