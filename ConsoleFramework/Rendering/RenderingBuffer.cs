using System;
using System.Diagnostics;
using System.Text;
using ConsoleFramework.Core;
using ConsoleFramework.Native;

namespace ConsoleFramework.Rendering
{
    /// <summary>
    /// Stores rendered control content.
    /// Supports impositioning and opacity mask matrix.
    /// </summary>
    public sealed class RenderingBuffer {
        private CHAR_INFO[,] buffer;
        /// <summary> todo : convert to enum
        /// 0 - непрозрачный пиксель
        /// 1 - полупрозрачный (отображается как тень)
        /// 2 - полностью прозрачный (при наложении на другой буфер будет проигнорирован)
        /// 3 - прозначный фон (при наложении на другой буфер символы возьмут его background) - для рамок на кнопках, к примеру
        /// 
        /// 4 - то же, что и 0, но пропускает через себя события мыши
        /// 5 - то же, что и 1, но пропускает через себя события мыши
        /// 6 - то же, что и 2, но пропускает через себя события мыши
        /// 7 - то же, что и 3, но пропускает через себя события мыши
        /// </summary>
        private int[,] opacityMatrix;
        // todo : add bool hasOpacityAttributes and optimize this
        private int width;
        private int height;

        public int Width {
            get {
                return width;
            }
        }

        public int Height {
            get {
                return height;
            }
        }

        public RenderingBuffer() {
        }

        public RenderingBuffer(int width, int height) {
            buffer = new CHAR_INFO[width, height];
            opacityMatrix = new int[width, height];
            this.width = width;
            this.height = height;
        }

        public void CopyFrom(RenderingBuffer renderingBuffer) {
            this.buffer = new CHAR_INFO[renderingBuffer.width, renderingBuffer.height];
            this.opacityMatrix = new int[renderingBuffer.width, renderingBuffer.height];
            this.width = renderingBuffer.width;
            this.height = renderingBuffer.height;
            //
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    buffer[x, y] = renderingBuffer.buffer[x, y];
                    opacityMatrix[x, y] = renderingBuffer.opacityMatrix[x, y];
                }
            }
        }

        /// <summary>
        /// Накладывает буфер дочернего элемента на текущий. Дочерний буфер виртуально накладывается на текущий
        /// в соответствии с переданным actualOffset, а потом та часть дочернего буфера, которая попадает в 
        /// renderSlotRect, прорисовывается. renderSlotRect определен отн-но текущего буфера (а не дочернего).
        /// layoutClip определяет, какая часть дочернего буфера будет прорисована в текущий буфер (клиппинг,
        /// возникающий при применении Margin и Alignment'ов).
        /// </summary>
        /// <param name="childBuffer"></param>
        /// <param name="actualOffset">Смещение буфера дочернего элемента относительно текущего.</param>
        /// <param name="childRenderSize">Размер отрендеренного дочерним элементом контента - может
        /// быть меньше размера childBuffer, поэтому нужно передавать явно.</param>
        /// <param name="renderSlotRect">Размер и положение слота, выделенного дочернему элементу.</param>
        /// <param name="layoutClip">Часть дочернего буфера, которая будет отрисована - по размеру может быть
        /// меньше или равна RenderSlotRect.Size. По координатам соотносится с childBuffer.</param>
        public void ApplyChild(RenderingBuffer childBuffer, Vector actualOffset,
            Size childRenderSize, Rect renderSlotRect, Rect layoutClip) {

            ApplyChild( childBuffer, actualOffset, childRenderSize, renderSlotRect, layoutClip, null );
        }

        /// <summary>
        /// Оверлоад для оптимизированного наложения в случае, когда известно, что в дочернем
        /// контроле поменялась лишь часть, идентифицируемая параметром affectedRect.
        /// Будет обработана только эта часть дочернего контрола, и количество операций уменьшится.
        /// </summary>
        /// <param name="childBuffer"></param>
        /// <param name="actualOffset"></param>
        /// <param name="childRenderSize"></param>
        /// <param name="renderSlotRect"></param>
        /// <param name="layoutClip"></param>
        /// <param name="affectedRect">Прямоугольник в дочернем контроле, который был изменен.</param>
        public void ApplyChild(RenderingBuffer childBuffer, Vector actualOffset,
                               Size childRenderSize, Rect renderSlotRect,
                               Rect layoutClip, Rect? affectedRect) {

            // Считаем finalRect - прямоугольник относительно parent, который нужно закрасить
            Rect finalRect = layoutClip;

            if (affectedRect != null)
                finalRect.Intersect(affectedRect.Value);

            // Если child.RenderSlotRect больше child.RenderSize, а rendering buffer
            // дочернего контрола больше его RenderSize (такое бывает после уменьшения
            // размеров контрола - т.к. буфер может только увеличиваться, но не уменьшаться) -
            // то нам нужно либо передать в метод ApplyChild и child.RenderSize, либо
            // выполнить пересечение заранее
            finalRect.Intersect(new Rect(new Point(0, 0), childRenderSize));

            // Because cannot call Offset() method of empty rect
            if ( finalRect.IsEmpty ) return;

            finalRect.Offset(actualOffset);
            finalRect.Intersect( renderSlotRect );

            // Нужно также учесть размеры буфера текущего контрола
            finalRect.Intersect( new Rect(new Point(0, 0), new Size(this.width, this.height)) );

            for ( int x = finalRect.Left; x < finalRect.Right; x++ ) {
                int parentX = x;
                int childX = parentX - actualOffset.x;
                for ( int y = finalRect.Top; y < finalRect.Bottom; y++ ) {
                    int parentY = y;
                    int childY = parentY - actualOffset.y;

                    CHAR_INFO charInfo = childBuffer.buffer[childX, childY];
                    int opacity = childBuffer.opacityMatrix[childX, childY];

                    // Для полностью прозрачных пикселей родительского буфера - присваиваем и значение
                    // пикселя, и значение opacity, дальше дело за следующим родителем
                    if ( this.opacityMatrix[ parentX, parentY ] == 2 || this.opacityMatrix[ parentX, parentY ] == 6 ) {
                        this.buffer[ parentX, parentY ] = charInfo;
                        this.opacityMatrix[ parentX, parentY ] = opacity;
                    } else {
                        // В остальных случаях opacity родительского буфера остаётся, а
                        // сам пиксель зависит от opacity дочернего элемента
                        if ( opacity == 0 || opacity == 4 ) {
                            this.buffer[ parentX, parentY ] = charInfo;
                        } else if ( opacity == 1 || opacity == 5 ) {
                            charInfo.Attributes = Colors.Blend( Color.DarkGray, Color.Black );
                            charInfo.UnicodeChar = buffer[ parentX, parentY ].UnicodeChar;
                            buffer[ parentX, parentY ] = charInfo;
                        } else if ( opacity == 3 || opacity == 7 ) {
                            // берем фоновые атрибуты символа из родительского буфера
                            Attr parentAttr = buffer[ parentX, parentY ].Attributes;
                            if ( ( parentAttr & Attr.BACKGROUND_BLUE ) == Attr.BACKGROUND_BLUE ) {
                                charInfo.Attributes |= Attr.BACKGROUND_BLUE;
                            } else {
                                charInfo.Attributes &= ~Attr.BACKGROUND_BLUE;
                            }
                            if ( ( parentAttr & Attr.BACKGROUND_GREEN ) == Attr.BACKGROUND_GREEN ) {
                                charInfo.Attributes |= Attr.BACKGROUND_GREEN;
                            } else {
                                charInfo.Attributes &= ~Attr.BACKGROUND_GREEN;
                            }
                            if ( ( parentAttr & Attr.BACKGROUND_RED ) == Attr.BACKGROUND_RED ) {
                                charInfo.Attributes |= Attr.BACKGROUND_RED;
                            } else {
                                charInfo.Attributes &= ~Attr.BACKGROUND_RED;
                            }
                            if ( ( parentAttr & Attr.BACKGROUND_INTENSITY ) == Attr.BACKGROUND_INTENSITY ) {
                                charInfo.Attributes |= Attr.BACKGROUND_INTENSITY;
                            } else {
                                charInfo.Attributes &= ~Attr.BACKGROUND_INTENSITY;
                            }
                            buffer[ parentX, parentY ] = charInfo;
                        }
                    }
                }
            }
        }

        public void SetPixel(int x, int y, char c) {
            buffer[x, y].UnicodeChar = c;
        }

        public void SetPixel(int x, int y, Attr attr) {
            buffer[x, y].Attributes = attr;
        }

        public void SetPixel(int x, int y, char c, Attr attr) {
            buffer[x, y].UnicodeChar = c;
            buffer[x, y].Attributes = attr;
        }

        public void SetOpacity(int x, int y, int opacity) {
            if (opacity < 0 || opacity > 7)
                throw new ArgumentException("opacity");
            //
            opacityMatrix[x, y] = opacity;
        }

        public void SetOpacityRect(int x, int y, int w, int h, int opacity) {
            if (opacity < 0 || opacity > 7)
                throw new ArgumentException("opacity");
            for (int i = 0; i < w; i++) {
                int _x = x + i;
                for (int j = 0; j < h; j++) {
                    opacityMatrix[_x, y + j] = opacity;
                }
            }
        }

        public void FillRectangle(int x, int y, int w, int h, char c, Attr attributes) {
            for (int _x = 0; _x < w; _x++) {
                for (int _y = 0; _y < h; _y++) {
                    SetPixel(x + _x, y + _y, c, attributes);
                }
            }
        }

        /// <summary>
        /// Копирует affectedRect из буфера на экран консоли с учетом того, что буфер
        /// находится на экране консоли по смещению offset.
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="affectedRect">Измененная область относительно this.</param>
        /// <param name="offset">В какой точке экрана размещен контрол (см <see cref="Renderer.RootElementRect"/>).</param>
        public void CopyToPhysicalCanvas(PhysicalCanvas canvas, Rect affectedRect, Point offset) {
            Rect rectToCopy = affectedRect;
            Rect bufferRect = new Rect(new Point(0, 0), new Size(this.width, this.height));
            Rect canvasRect = new Rect(new Point(-offset.X, -offset.Y), new Size(canvas.Width, canvas.Height));
            rectToCopy.Intersect(canvasRect);
            rectToCopy.Intersect(bufferRect);
            //
            for (int x = 0; x < rectToCopy.width; x++) {
                int bufferX = x + rectToCopy.x;
                int canvasX = x + rectToCopy.x + offset.x;
                for (int y = 0; y < rectToCopy.height; y++) {
                    int bufferY = y + rectToCopy.y;
                    int canvasY = y + rectToCopy.y + offset.y;
                    CHAR_INFO charInfo = buffer[bufferX, bufferY];
                    canvas[canvasX][canvasY].Assign(charInfo);
                }
            }
        }

        /// <summary>
        /// Renderer should call this method before any control render.
        /// </summary>
        public void Clear() {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    buffer[x, y] = new CHAR_INFO();
                    opacityMatrix[x, y] = 0;
                }
            }
        }

        public void DumpOpacityMatrix() {
            for (int y = 0; y < height; y++) {
                StringBuilder sb = new StringBuilder();
                for (int x = 0; x < width; x++) {
                    sb.Append(opacityMatrix[x, y]);
                }
                Debug.WriteLine(sb);
            }
        }

        /// <summary>
        /// Проверяет, содержит ли affectedRect пиксели с выставленным значением opacity.
        /// Это необходимо для обеспечения корректного смешивания с родительскими буферами в случае
        /// частичного обновления экрана (если это не учитывать, то состояние экрана может смешивать
        /// новые пиксели со старыми, которые были получены при предыдущем вызове рендеринга).
        /// </summary>
        public bool ContainsOpacity(Rect affectedRect) {
            for (int x = 0; x < affectedRect.width; x++) {
                for (int y = 0; y < affectedRect.height; y++) {
                    if (opacityMatrix[x + affectedRect.x, y + affectedRect.y] != 0) {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Возвращает код прозрачности в указанной точке.
        /// </summary>
        public int GetOpacityAt( int x, int y ) {
            return opacityMatrix[ x, y ];
        }
    }
}
