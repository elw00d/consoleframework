using System;
using System.Collections.Generic;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;

namespace ConsoleFramework
{
    /// <summary>
    /// Central point of the console framework layout system.
    /// </summary>
    public sealed class Renderer {
        /// <summary>
        /// Прямоугольная область относительно экрана консоли, в которой будет размещён Root Element.
        /// </summary>
        public Rect RootElementRect {
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

        // buffers containing only control rendering representation itself
        private readonly Dictionary<Control, RenderingBuffer> buffers = new Dictionary<Control, RenderingBuffer>();
        // buffers containing full control render (with children render applied)
        private readonly Dictionary<Control, RenderingBuffer> fullBuffers = new Dictionary<Control, RenderingBuffer>();
        // queue of controls marked for layout invalidation
        private readonly Queue<Control> invalidatedControls = new Queue<Control>();

        // список контролов, у которых обновилось содержимое full render buffer
        // актуален только при вызове UpdateRender, после вызова очищается
        private readonly List<Control> renderingUpdatedControls = new List<Control>();

        /// <summary>
        /// Пересчитывает лайаут для всех контролов, добавленных в очередь ревалидации.
        /// Определяет, какие контролы необходимо перерисовать, вызывает Render у них.
        /// Определяет, какие области экрана необходимо обновить и выполняет перерисовку
        /// экрана консоли.
        /// </summary>
        public void UpdateRender() {
            renderingUpdatedControls.Clear();
            // invalidate layout and fill renderingUpdatedControls list
            InvalidateLayout();
            // propagate updated rendered buffers to parent elements and eventually to Canvas
            Rect affectedRect = Rect.Empty;
            //if (renderingUpdatedControls.Count > 0) {
            //    Debug.WriteLine("Rendering updated controls : {0}.", renderingUpdatedControls.Count);
            //}
            foreach (Control control in renderingUpdatedControls) {
                Rect currentAffectedRect = applyChangesToCanvas(control, new Rect(new Point(0, 0), control.RenderSize));
                affectedRect.Union(currentAffectedRect);
            }
            if (!affectedRect.IsEmpty) {
                // flush stored image (with this.RootElementRect offset)

                // affected rect relative to canvas
                Rect affectedRectAbsolute = new Rect(affectedRect.x + RootElementRect.x, affectedRect.y + RootElementRect.y, affectedRect.width, affectedRect.height);
                // clip according to real canvas size
                affectedRectAbsolute.Intersect(new Rect(new Point(0, 0), new Size(Canvas.Width, Canvas.Height)));
                Canvas.Flush(affectedRectAbsolute);
            }
            // if anything changed in layout - update displaying cursor state
            if (renderingUpdatedControls.Count > 0) {
                ConsoleApplication.Instance.FocusManager.RefreshMouseCursor();
            }
        }

        /// <summary>
        /// Получает для указанного контрола full render buffer и применяет его последовательно
        /// ко всем родительским элементам управления, вплоть до изображения на экране.
        /// Возвращает прямоугольник, необходимый для ревалидации на экране (affected rect).
        /// Учитывает Z-Order контролов-соседей (если родительский контрол имеет несколько дочерних, они могут перекрывать
        /// друг друга).
        /// Первый вызов производится с affectedRect = control.RenderSize.
        /// </summary>
        /// <returns>Affected rectangle in canvas should be copyied to console screen.</returns>
        private Rect applyChangesToCanvas(Control control, Rect affectedRect) {
            // если системой лайаута были определены размеры дочернего контрола, превышающие размеры слота
            // (такое может произойти, если дочерний контрол игнорирует переданные аргументы в MeasureOverride
            // и ArrangeOverride), то в этом месте может прийти affectedRect, выходящий за рамки
            // текущего RenderSize контрола, и мы должны выполнить intersection для корректного наложения
            affectedRect.Intersect(new Rect(new Point(0, 0), control.RenderSize));
            RenderingBuffer fullBuffer = getOrCreateFullBufferForControl(control);
            if (control.Parent != null) {
                RenderingBuffer fullParentBuffer = getOrCreateFullBufferForControl(control.Parent);
                // если буфер контрола содержит opacity пиксели в affectedRect, то мы вынуждены переинициализировать
                // буфер парента целиком (не вызывая Render, конечно, но переналожением буферов дочерних элементов)
                if (fullBuffer.ContainsOpacity(affectedRect)) {
                    fullParentBuffer.Clear();
                    fullParentBuffer.CopyFrom(getOrCreateBufferForControl(control.Parent));
                    foreach (Control child in control.Parent.Children) {
                        if (child.Visibility == Visibility.Visible) {
                            RenderingBuffer childBuffer = getOrCreateFullBufferForControl(child);
                            fullParentBuffer.ApplyChild(childBuffer, child.ActualOffset, child.RenderSlotRect,
                                                        child.LayoutClip);
                        }
                    }
                }
                // определим соседей контрола, которые могут перекрывать его
                List<Control> neighbors = control.Parent.GetChildrenOrderedByZIndex();
                int controlIndex = neighbors.FindIndex(0, control1 => control1 == control);
                if (control.Visibility == Visibility.Visible) {
                    // начиная с controlIndex + 1 в списке лежат контролы с z-index больше чем z-index текущего контрола
                    if (affectedRect == new Rect(new Point(0, 0), control.RenderSize)) {
                        fullParentBuffer.ApplyChild(fullBuffer, control.ActualOffset, control.RenderSlotRect,
                                                    control.LayoutClip);
                    } else {
                        fullParentBuffer.ApplyChild(fullBuffer, control.ActualOffset, control.RenderSlotRect,
                                                    control.LayoutClip, affectedRect);
                    }
                }
                // восстанавливаем изображение поверх обновленного контрола, если
                // имеются контролы, лежащие выше по z-order
                for (int i = controlIndex + 1; i < neighbors.Count; i++) {
                    Control neighbor = neighbors[i];
                    fullParentBuffer.ApplyChild(getOrCreateFullBufferForControl(neighbor), neighbor.ActualOffset,
                        neighbor.RenderSlotRect, neighbor.LayoutClip);
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
                fullBuffer.CopyToPhysicalCanvas(Canvas, affectedRect, RootElementRect.TopLeft);
                return affectedRect;
            }
        }

        /// <summary>
        /// Пересчитывает лайаут для всех контролов, добавленных в очередь ревалидации.
        /// После того, как лайаут контрола рассчитан, выполняется рендеринг.
        /// Рендеринг производится только тогда, когда размеры контрола изменились или
        /// контрол явно помечен как изменивший свое изображение. В остальных случаях
        /// используются кешированные буферы, содержащие уже отрендеренные изображения.
        /// </summary>
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
                control.Measure(RootElementRect.Size);
                control.Arrange(RootElementRect);
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
            buffer.Clear();
            if (control.RenderSize.Width != 0 && control.RenderSize.Height != 0)
                control.Render(buffer);
            // проверяем дочерние контролы - если их layoutInfo не изменился по сравнению с последним,
            // то мы можем взять их последний renderBuffer без обновления и применить к текущему контролу
            fullBuffer.CopyFrom(buffer);
            List<Control> children = control.Children;
            foreach (Control child in children) {
                if (child.Visibility == Visibility.Visible) {
                    RenderingBuffer fullChildBuffer = processControl(child);
                    fullBuffer.ApplyChild(fullChildBuffer, child.ActualOffset, child.RenderSlotRect, child.LayoutClip);
                } else {
                    // чтобы следующий Invalidate перезаписал lastLayoutInfo
                    child.LayoutValidity = LayoutValidity.Render;
                }
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
        /// экране. Но такая оптимизация может применяться только для тех контролов, которые за прошедший
        /// layout pass не побывали в invalidatedControlsQueue. В противном случае мы вынуждены обновить их
        /// целиком.
        /// </summary>
        private void addControlToRenderingUpdatedList(Control control) {
            //if (renderingUpdatedControls.Contains(control)) {
            //    return;
            //}
            //Control controlToReplace = null;
            //bool dontAddControl = false;
            //foreach (Control c in renderingUpdatedControls) {
            //    // check if c is parent of control
            //    Control current = control;
            //    do {
            //        if (current == c) {
            //            // c is parent of control, we should replace c with control
            //            controlToReplace = c;
            //            break;
            //        }
            //        current = current.Parent;
            //    } while (current != null);
            //    // check if control is parent of c
            //    current = c;
            //    do {
            //        if (current == control) {
            //            // control is parent of c, we should do nothing with list
            //            dontAddControl = true;
            //            break;
            //        }
            //        current = current.Parent;
            //    } while (current != null);
            //}
            //if (null != controlToReplace) {
            //    if (!currentLayoutPassInvalidatedControls.Contains(controlToReplace)) {
            //        renderingUpdatedControls.Remove(controlToReplace);
            //    }
            //}
            //if (!dontAddControl || currentLayoutPassInvalidatedControls.Contains(control)) {
            //    renderingUpdatedControls.Add(control);
            //}
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
            buffer.Clear();
            if (control.RenderSize.Width != 0 && control.RenderSize.Height != 0)
                control.Render(buffer);
            //
            fullBuffer.CopyFrom(buffer);
            foreach (Control child in control.Children) {
                if (child.Visibility == Visibility.Visible) {
                    RenderingBuffer fullChildBuffer = processControl(child);
                    fullBuffer.ApplyChild(fullChildBuffer, child.ActualOffset, child.RenderSlotRect, child.LayoutClip);
                } else {
                    // чтобы следующий Invalidate для этого контрола
                    // перезаписал lastLayoutInfo
                    child.LayoutValidity = LayoutValidity.Render;
                }
            }
            //
            control.LayoutValidity = LayoutValidity.Render;
            //
            addControlToRenderingUpdatedList(control);
            //
            return fullBuffer;
        }

        public void AddControlToInvalidationQueue(Control control) {
            if (null == control) {
                throw new ArgumentNullException("control");
            }
            if (!invalidatedControls.Contains(control)) {
                // add to queue only if it has parent or it is root element
                if (control.Parent != null || control == RootElement) {
                    invalidatedControls.Enqueue(control);
                }
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

        /// <summary>
        /// Возващает код прозрачности контрола в указанной точке.
        /// Это необходимо для определения контрола, который станет источником события мыши.
        /// </summary>
        internal int getControlOpacityAt( Control control, int x, int y ) {
            return buffers[ control ].GetOpacityAt( x, y );
        }
    }
}
