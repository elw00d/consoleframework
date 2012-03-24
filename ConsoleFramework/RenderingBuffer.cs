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
            for (int x = 0; x < renderSlotRect.width; x++) {
                int parentX = x + renderSlotRect.x;
                int childX = parentX - actualOffset.x;
                // поскольку renderSlotRect может выходить за рамки буфера родительского контрола
                // (если родительский контрол вызвал Arrange и указал в качестве аргумента большой Rect),
                // то мы должны обработать этот случай и предотвратить переполнение буфера
                if (parentX >= 0 && parentX < this.width && childX >= 0 && childX < childBuffer.width) {
                    for (int y = 0; y < renderSlotRect.height; y++) {
                        int parentY = y + renderSlotRect.y;
                        int childY = parentY - actualOffset.y;
                        if (parentY >= 0 && parentY < this.height && childY >= 0 && childY < childBuffer.height) {
                            if (layoutClip.Contains(childX, childY)) {
                                CHAR_INFO charInfo = childBuffer.buffer[childX, childY];
                                // skip empty pixels (considering it as transparent pixels)
                                if (charInfo.AsciiChar != '\0' || charInfo.Attributes != CHAR_ATTRIBUTES.NO_ATTRIBUTES) {
                                    this.buffer[parentX, parentY] = charInfo;
                                }
                            }
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
                int childX = x + affectedRect.x;
                int parentX = childX + actualOffset.x;
                if (parentX >= 0 && parentX < this.width && childX >= 0 && childX < childBuffer.width) {
                    for (int y = 0; y < affectedRect.height; y++) {
                        int childY = y + affectedRect.y;
                        int parentY = childY + actualOffset.y;
                        if (parentY >= 0 && parentY < this.height && childY >= 0 && childY < childBuffer.height) {
                            if (renderSlotRect.Contains(parentX, parentY) && layoutClip.Contains(x, y)) {
                                CHAR_INFO charInfo = childBuffer.buffer[x + affectedRect.x, y + affectedRect.y];
                                // skip empty pixels (considering it as transparent pixels)
                                if (charInfo.AsciiChar != '\0' || charInfo.Attributes != CHAR_ATTRIBUTES.NO_ATTRIBUTES) {
                                    this.buffer[parentX, parentY] = charInfo;
                                }
                            }
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
        /// Копирует часть изображения из буфера на экран консоли так, что точка (rect.x, rect.y) на canvas'e
        /// совмещается с точкой (affectedRect.x, affectedRect.y) буфера.
        /// AffectedRect определяет, какая часть буфера будет скопирована на экран.
        /// Rect определяет часть Canvas'a, в которую будет произведено копирование (обычно - канвас целиком то есть
        /// (0, 0, canvas.Width, canvas.Height) ).
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="rect">Canvas rectangle.</param>
        /// <param name="affectedRect"></param>
        public void CopyToPhysicalCanvas(PhysicalCanvas canvas, Rect rect, Rect affectedRect) {
            // проверяем, что rect влезает в canvas целиком
            if (rect.x + rect.width > canvas.Width || rect.y + rect.height > canvas.Height) {
                throw new ArgumentException("Assertion failed: rect should not be out of canvas bounds.", "rect");
            }
            // определяем Rect относительно буфера, по размеру не превышающий переданного rect
            // (если этого не сделать, то возможна ситуация, когда мы пытаемся большой прямоугольник, заданный affectedRect,
            // впихнуть в маленький rect на canvas'e
            Rect rectToCopy = affectedRect;
            rectToCopy.Intersect(new Rect(new Point(affectedRect.x, affectedRect.y), rect.Size));
            //
            for (int x = 0; x < rectToCopy.width; x++) {
                for (int y = 0; y < rectToCopy.height; y++) {
                    CHAR_INFO charInfo = buffer[x + rectToCopy.x, y + rectToCopy.y];
                    if (charInfo.AsciiChar != '\0' || charInfo.Attributes != CHAR_ATTRIBUTES.NO_ATTRIBUTES) {
                        canvas[x + rect.X][y + rect.Y].Assign(charInfo);
                    }
                }
            }
        }
    }
}
