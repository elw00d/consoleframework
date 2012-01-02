using ConsoleFramework.Controls;
using ConsoleFramework.Core;
using ConsoleFramework.Native;

namespace ConsoleFramework
{
    /// <summary>
    /// Область, доступная для рисования контролу.
    /// Контрол является самым первым, то VirtualCanvas рисует напрямую в PhysicalCanvas.
    /// Если же под контролом есть другие контролы, то рисование делегируется underlying контролу.
    /// </summary>
    public class VirtualCanvas {

        private PhysicalCanvas physicalCanvas;
        private int initialX;
        private int initialY;
        // связанный с канвасом контрол либо null, если пишем напрямую в physical
        private Control control;

        public VirtualCanvas(Control control, PhysicalCanvas canvas, int initialX, int initialY) {
            this.control = control;
            physicalCanvas = canvas;
            this.initialX = initialX;
            this.initialY = initialY;
        }

        public VirtualCanvas(Control control) {
            //
            this.control = control;
        }

        public char GetPixel(int x, int y) {
            if (physicalCanvas != null) {
                return physicalCanvas[x + initialX][y + initialY].AsciiChar;
            } else {
                Point point = control.Parent.GetChildOffset(control);
                return control.Parent.canvas.GetPixel(x + point.X, y + point.Y);
            }
        }

        public CHAR_ATTRIBUTES GetPixelAttributes(int x, int y) {
            if (physicalCanvas != null) {
                return physicalCanvas[x + initialX][y + initialY].Attributes;
            } else {
                Point point = control.Parent.GetChildOffset(control);
                return control.Parent.canvas.GetPixelAttributes(x + point.X, y + point.Y);
            }
        }

        public void SetPixel(int x, int y, CHAR_ATTRIBUTES attributes) {
            if (physicalCanvas != null) {
                physicalCanvas[x + initialX][y + initialY].Attributes = attributes;
            } else {
                Point point = control.Parent.GetChildOffset(control);
                control.Parent.canvas.SetPixel(x + point.X, y + point.Y, attributes);
            }
        }

        public void SetPixel(int x, int y, char character, CHAR_ATTRIBUTES attributes) {
            if (physicalCanvas != null) {
                physicalCanvas[x + initialX][y + initialY].AsciiChar = character;
                physicalCanvas[x + initialX][y + initialY].Attributes = attributes;
            } else {
                Point point = control.Parent.GetChildOffset(control);
                control.Parent.canvas.SetPixel(x + point.X, y + point.Y, character, attributes);
            }
        }

        public void SetPixel(int x, int y, char character) {
            if (physicalCanvas != null) {
                physicalCanvas[x + initialX][y + initialY].AsciiChar = character;
            } else {
                Point point = control.Parent.GetChildOffset(control);
                control.Parent.canvas.SetPixel(x + point.X, y + point.Y, character);
            }
        }

        public void FillRectangle(int x, int y, int width, int height, char c, CHAR_ATTRIBUTES attributes) {
            for (int _x = 0; _x < width; _x++) {
                for (int _y = 0; _y < height; _y++) {
                    SetPixel(x + _x, y + _y, c, attributes);
                }
            }
        }
    }
}
