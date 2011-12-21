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

        public void SetPixel(int x, int y, char character, CHAR_ATTRIBUTES attributes) {
            if (physicalCanvas != null) {
                physicalCanvas[x + initialX][y + initialY].AsciiChar = character;
                physicalCanvas[x + initialX][y + initialY].Attributes = attributes;
            } else {
                Point point = control.Parent.GetChildPoint(control);
                control.Parent.canvas.SetPixel(x + point.X, y + point.Y, character, attributes);
            }
        }

        public void SetPixel(int x, int y, char character) {
            if (physicalCanvas != null) {
                physicalCanvas[x + initialX][y + initialY].AsciiChar = character;
            } else {
                Point point = control.Parent.GetChildPoint(control);
                control.Parent.canvas.SetPixel(x + point.X, y + point.Y, character);
            }
        }
    }
}
