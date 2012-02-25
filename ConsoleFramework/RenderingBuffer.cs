using System;
using ConsoleFramework.Core;
using ConsoleFramework.Native;

namespace ConsoleFramework
{
    public sealed class RenderingBuffer {
        private CHAR_INFO[,] buffer;
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
            this.width = width;
            this.height = height;
        }

        public void CopyFrom(RenderingBuffer renderingBuffer) {
            this.buffer = new CHAR_INFO[renderingBuffer.width, renderingBuffer.height];
            this.width = renderingBuffer.width;
            this.height = renderingBuffer.height;
            //
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    buffer[x, y] = renderingBuffer.buffer[x, y];
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
        /// <param name="renderSlotRect">Размер и положение слота, выделенного дочернему элементу.</param>
        /// <param name="layoutClip">Часть дочернего буфера, которая будет отрисована.</param>
        public void ApplyChild(RenderingBuffer childBuffer, Vector actualOffset, Rect renderSlotRect, Rect layoutClip) {
            // todo : optimize this
            // для уменьшения количества итераций необходимо двигаться не по всему childBuffer'у, а
            // по renderSlotRect'у, поскольку как правило renderSlotRect существенно меньше размера childBuffer
            for (int x = 0; x < childBuffer.width; x++) {
                int parentX = x + actualOffset.x;
                for (int y = 0; y < childBuffer.height; y++) {
                    int parentY = y + actualOffset.y;
                    if (renderSlotRect.Contains(parentX, parentY) && layoutClip.Contains(x, y)) {
                        CHAR_INFO charInfo = childBuffer.buffer[x, y];
                        // skip empty pixels (considering it as transparent pixels)
                        if (charInfo.AsciiChar != '\0' || charInfo.Attributes != CHAR_ATTRIBUTES.NO_ATTRIBUTES) {
                            this.buffer[parentX, parentY] = charInfo;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Оверлоад для оптимизированного наложения в случае, когда известно, что в дочернем
        /// контроле поменялась лишь часть, идентифицируемая параметром affectedRect.
        /// Будет обработана только эта часть дочернего контрола, и количество операций уменьшится.
        /// </summary>
        /// <param name="childBuffer"></param>
        /// <param name="actualOffset"></param>
        /// <param name="renderSlotRect"></param>
        /// <param name="layoutClip"></param>
        /// <param name="affectedRect">Прямоугольник в дочернем контроле, который был изменен.</param>
        public void ApplyChild(RenderingBuffer childBuffer, Vector actualOffset, Rect renderSlotRect,
                               Rect layoutClip, Rect affectedRect) {
            //
            for (int x = 0; x < affectedRect.width; x++) {
                int parentX = x + actualOffset.x + affectedRect.x;
                for (int y = 0; y < affectedRect.height; y++) {
                    int parentY = y + actualOffset.y + affectedRect.y;
                    if (renderSlotRect.Contains(parentX, parentY) && layoutClip.Contains(x, y)) {
                        CHAR_INFO charInfo = childBuffer.buffer[x, y];
                        // skip empty pixels (considering it as transparent pixels)
                        if (charInfo.AsciiChar != '\0' || charInfo.Attributes != CHAR_ATTRIBUTES.NO_ATTRIBUTES) {
                            this.buffer[parentX, parentY] = charInfo;
                        }
                    }
                }
            }
        }

        public void SetPixel(int x, int y, char c) {
            buffer[x, y].AsciiChar = c;
        }

        public void SetPixel(int x, int y, CHAR_ATTRIBUTES attr) {
            buffer[x, y].Attributes = attr;
        }

        public void SetPixel(int x, int y, char c, CHAR_ATTRIBUTES attr) {
            buffer[x, y].AsciiChar = c;
            buffer[x, y].Attributes = attr;
        }

        public void FillRectangle(int x, int y, int w, int h, char c, CHAR_ATTRIBUTES attributes) {
            for (int _x = 0; _x < w; _x++) {
                for (int _y = 0; _y < h; _y++) {
                    SetPixel(x + _x, y + _y, c, attributes);
                }
            }
        }

        /// <summary>
        /// Копирует содержимое буфера на экран консоли, при этом точка (0, 0) буфера
        /// будет скопирована на экран в место, определяемое (rect.x, rect.y).
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="rect"></param>
        public void CopyToPhysicalCanvas(PhysicalCanvas canvas, Rect rect) {
            int minWidth = Math.Min(width, rect.width);
            int minHeight = Math.Min(height, rect.height);
            //
            for (int x = 0; x < minWidth; x++) {
                for (int y = 0; y < minHeight; y++) {
                    CHAR_INFO charInfo = buffer[x, y];
                    if (charInfo.AsciiChar != '\0' || charInfo.Attributes != CHAR_ATTRIBUTES.NO_ATTRIBUTES) {
                        canvas[x + rect.X][y + rect.Y].Assign(charInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Overload для частичного копирования буфера.
        /// AffectedRect определяет, какая часть буфера будет скопирована на экран.
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="rect"></param>
        /// <param name="affectedRect"></param>
        public void CopyToPhysicalCanvas(PhysicalCanvas canvas, Rect rect, Rect affectedRect) {
            Rect rectToCopy = affectedRect;
            rectToCopy.Intersect(new Rect(new Point(0, 0), rect.Size));
            //
            for (int x = 0; x < rectToCopy.width; x++) {
                for (int y = 0; y < rectToCopy.height; y++) {
                    CHAR_INFO charInfo = buffer[x, y];
                    if (charInfo.AsciiChar != '\0' || charInfo.Attributes != CHAR_ATTRIBUTES.NO_ATTRIBUTES) {
                        canvas[x + rect.X][y + rect.Y].Assign(charInfo);
                    }
                }
            }
        }
    }
}
