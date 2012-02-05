using ConsoleFramework.Core;
using ConsoleFramework.Native;

namespace ConsoleFramework
{
    public sealed class RenderingBuffer {
        private CHAR_INFO[,] buffer;
        private int width;
        private int height;

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
        /// </summary>
        /// <param name="childBuffer"></param>
        /// <param name="actualOffset">Смещение буфера дочернего элемента относительно текущего.</param>
        /// <param name="renderSlotRect">Размер и положение слота, выделенного дочернему элементу.</param>
        public void ApplyChild(RenderingBuffer childBuffer, Vector actualOffset, Rect renderSlotRect) {
            //
            for (int x = 0; x < childBuffer.width; x++) {
                int parentX = x + actualOffset.x;
                for (int y = 0; y < childBuffer.height; y++) {
                    int parentY = y + actualOffset.y;
                    if (renderSlotRect.Contains(parentX, parentY)) {
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

        public void CopyToPhysicalCanvas(PhysicalCanvas canvas, Rect rect) {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    CHAR_INFO charInfo = buffer[x, y];
                    if (charInfo.AsciiChar != '\0' || charInfo.Attributes != CHAR_ATTRIBUTES.NO_ATTRIBUTES) {
                        canvas[x + rect.X][y + rect.Y].Assign(charInfo);
                    }
                }
            }
        }
    }
}
