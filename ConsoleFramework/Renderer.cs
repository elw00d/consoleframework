using System;
using System.Collections.Generic;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;

namespace ConsoleFramework
{
    public sealed class Renderer {
        public Rect Rect {
            get;
            set;
        }

        public Control RootElement {
            get;
            set;
        }

        public PhysicalCanvas Canvas {
            get;
            set;
        }
        
        private readonly List<Control> renderingUpdatedControls = new List<Control>();

        public void UpdateRender() {
            renderingUpdatedControls.Clear();
            // invalidate layout and fill renderingUpdatedControls list
            InvalidateLayout();
            // propagate renderingUpdatedControls to parent elements
            Rect affectedRect = Rect.Empty;
            foreach (Control control in renderingUpdatedControls) {
                Rect currentAffectedRect = applyChangesToCanvas(control, new Rect(new Point(0, 0), control.RenderSize));
                affectedRect.Union(currentAffectedRect);
            }
            Canvas.Flush(affectedRect);
        }

        /// <summary>
        /// Получает для указанного контрола full render buffer и применяет его последовательно
        /// ко всем родительским элементам управления, вплоть до изображения на экране.
        /// Возвращает прямоугольник, необходимый для ревалидации на экране
        /// (affected rect).
        /// Первый вызов производится с affectedRect = control.RenderSize.
        /// </summary>
        /// <returns>Affected rectangle in canvas should be copyied to console screen.</returns>
        private Rect applyChangesToCanvas(Control control, Rect affectedRect) {
            RenderingBuffer fullBuffer = getOrCreateFullBufferForControl(control);
            if (control.Parent != null) {
                RenderingBuffer fullParentBuffer = getOrCreateFullBufferForControl(control.Parent);
                if (affectedRect == new Rect(new Point(0, 0), control.RenderSize)) {
                    fullParentBuffer.ApplyChild(fullBuffer, control.ActualOffset, control.RenderSlotRect,
                                                control.LayoutClip);
                } else {
                    fullParentBuffer.ApplyChild(fullBuffer, control.ActualOffset, control.RenderSlotRect,
                                                control.LayoutClip, affectedRect);
                }
                Rect parentAffectedRect = control.RenderSlotRect;
                parentAffectedRect.Intersect(new Rect(affectedRect.x + control.ActualOffset.x,
                                                      affectedRect.y + control.ActualOffset.y,
                                                      affectedRect.width,
                                                      affectedRect.height));
                // нет смысла продолжать подъем вверх по дереву, если контрола точно уже не видно
                if (parentAffectedRect.IsEmpty) {
                    return Rect.Empty;
                }
                return applyChangesToCanvas(control.Parent, parentAffectedRect);
            } else {
                // мы добрались до экрана консоли
                fullBuffer.CopyToPhysicalCanvas(Canvas, Rect, affectedRect);
                return affectedRect;
            }
        }

        internal void InvalidateLayout() {
            while (invalidatedControls.Count != 0) {
                Control control = invalidatedControls.Dequeue();
                // set previous results of layout passes dirty
                control.ResetValidity();
                //
                updateLayout(control);
            }
        }

        private void updateLayout(Control control) {
            LayoutInfo lastLayoutInfo = control.lastLayoutInfo;
            // работаем с родительским элементом управления
            if (control.Parent != null) {
                bool needUpdateParentLayout = true;
                // если размер текущего контрола не изменился, то состояние ревалидации не распространяется
                // вверх по дереву элементов, и мы переходим к работе с дочерними элементами
                // в противном случае мы добавляем родительский элемент в конец очереди ревалидации, и
                // возвращаем управление
                if (lastLayoutInfo.validity != LayoutValidity.Nothing) {
                    control.Measure(lastLayoutInfo.measureArgument);
                    if (lastLayoutInfo.unclippedDesiredSize == control.layoutInfo.unclippedDesiredSize) {
                        needUpdateParentLayout = false;
                    }
                }
                if (needUpdateParentLayout) {
                    // mark the parent control for invalidation too and enqueue them
                    control.Parent.Invalidate();
                    // мы можем закончить с этим элементом, поскольку мы уже добавили
                    // в конец очереди его родителя, и мы все равно вернемся к нему в след. раз
                    return;
                }
            }
            // работаем с дочерними элементами управления
            // вызываем для текущего контрола Measure&Arrange с последними значениями аргументов
            if (lastLayoutInfo.validity == LayoutValidity.Nothing && control.Parent != null) {
                throw new InvalidOperationException("Assertion failed.");
            }
            // rootElement - особый случай
            if (control.Parent == null) {
                if (control != RootElement) {
                    throw new InvalidOperationException("Control has no parent but is not known rootElement.");
                }
                control.Measure(Rect.Size);
                control.Arrange(Rect);
            } else {
                control.Measure(lastLayoutInfo.measureArgument);
                control.Arrange(lastLayoutInfo.renderSlotRect);
            }
            // update render buffers of current control and its children
            RenderingBuffer buffer = getOrCreateBufferForControl(control);
            RenderingBuffer fullBuffer = getOrCreateFullBufferForControl(control);
            // replace buffers if control has grown
            LayoutInfo layoutInfo = control.layoutInfo;
            if (layoutInfo.renderSize.width > buffer.Width || layoutInfo.renderSize.height > buffer.Height) {
                buffer = new RenderingBuffer(layoutInfo.renderSize.width, layoutInfo.renderSize.height);
                fullBuffer = new RenderingBuffer(layoutInfo.renderSize.width, layoutInfo.renderSize.height);
                buffers[control] = buffer;
                fullBuffers[control] = fullBuffer;
            }
            control.Render(buffer);
            // проверяем дочерние контролы - если их layoutInfo не изменился по сравнению с последним,
            // то мы можем взять их последний renderBuffer без обновления и применить к текущему контролу
            fullBuffer.CopyFrom(buffer);
            List<Control> children = control.children;
            foreach (Control child in children) {
                RenderingBuffer fullChildBuffer = processControl(child);
                fullBuffer.ApplyChild(fullChildBuffer, child.ActualOffset, child.RenderSlotRect, child.LayoutClip);
            }
            //
            control.LayoutValidity = LayoutValidity.Render;
            addControlToRenderingUpdatedList(control);
        }

        /// <summary>
        /// Добавляет указанный контрол в список контролов, для которых обновлен full rendering buffer.
        /// Причем делает это таким образом, что если в списке уже лежит контрол, являющийся родительским
        /// для него, то родительский контрол заменяется текущим. А если в списке уже лежит контрол,
        /// являющийся дочерним для текущего контрола, то список не изменяется. Таким образом, в списке
        /// всегда находится лишь минимально необходимый набор контролов, буферы которых необходимо
        /// применить ко всем родительским контролам впроть до rootElement, чтобы обновить ситуацию на
        /// экране.
        /// </summary>
        private void addControlToRenderingUpdatedList(Control control) {
            // todo : add optimization logic
            renderingUpdatedControls.Add(control);
        }

        private RenderingBuffer processControl(Control control) {
            RenderingBuffer buffer = getOrCreateBufferForControl(control);
            RenderingBuffer fullBuffer = getOrCreateFullBufferForControl(control);
            //
            LayoutInfo lastLayoutInfo = control.lastLayoutInfo;
            LayoutInfo layoutInfo = control.layoutInfo;
            //
            control.Measure(lastLayoutInfo.measureArgument);
            control.Arrange(lastLayoutInfo.renderSlotRect);
            // if lastLayoutInfo eq layoutInfo we can use last rendered buffer
            if (layoutInfo.Equals(lastLayoutInfo) && lastLayoutInfo.validity == LayoutValidity.Render) {
                layoutInfo.validity = LayoutValidity.Render;
                return fullBuffer;
            }
            // replace buffers if control has grown
            if (layoutInfo.renderSize.width > buffer.Width || layoutInfo.renderSize.height > buffer.Height) {
                buffer = new RenderingBuffer(layoutInfo.renderSize.width, layoutInfo.renderSize.height);
                fullBuffer = new RenderingBuffer(layoutInfo.renderSize.width, layoutInfo.renderSize.height);
                buffers[control] = buffer;
                fullBuffers[control] = fullBuffer;
            }
            // otherwise we should assemble full rendered buffer using childs
            control.Render(buffer);
            //
            fullBuffer.CopyFrom(buffer);
            foreach (Control child in control.children) {
                RenderingBuffer fullChildBuffer = processControl(child);
                fullBuffer.ApplyChild(fullChildBuffer, child.ActualOffset, child.RenderSlotRect, child.LayoutClip);
            }
            //
            control.LayoutValidity = LayoutValidity.Render;
            //
            addControlToRenderingUpdatedList(control);
            //
            return fullBuffer;
        }

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

        //private void Render(Control rootElement, PhysicalCanvas canvas, Rect rect) {
        //    if ((uint) rootElement.LayoutValidity < (uint) LayoutValidity.MeasureAndArrange) {
        //        // measuring all visual elements tree
        //        rootElement.Measure(rect.Size);
        //        rootElement.Arrange(rect);
        //    }
        //    //
        //    RenderingBuffer buffer = UpdateRender(rootElement);
        //    buffer.CopyToPhysicalCanvas(canvas, rect);
        //}

        ///// <summary>
        ///// Updates the rendering buffers for specified control if need, and returns
        ///// buffer with full rendered control content (including its children).
        ///// </summary>
        //private RenderingBuffer UpdateRender(Control control) {
        //    RenderingBuffer buffer = getOrCreateBufferForControl(control);
        //    RenderingBuffer fullBuffer = getOrCreateFullBufferForControl(control);
        //    //
        //    if ((uint) control.LayoutValidity < (uint) LayoutValidity.Render) {
        //        if ((uint)control.LayoutValidity < (uint)LayoutValidity.MeasureAndArrange) {
        //            throw new NotSupportedException("You should invalidate a layout state of control before call render.");
        //        }
        //        control.Render(buffer);
        //        //
        //        fullBuffer.CopyFrom(buffer);
        //        foreach (Control child in control.children) {
        //            RenderingBuffer fullChildBuffer = UpdateRender(child);
        //            fullBuffer.ApplyChild(fullChildBuffer, child.ActualOffset, child.RenderSlotRect, child.LayoutClip);
        //        }
        //        //
        //        control.LayoutValidity = LayoutValidity.Render;
        //    }
        //    return fullBuffer;
        //}
    }
}
