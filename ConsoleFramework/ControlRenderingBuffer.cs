using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleFramework.Controls;
using ConsoleFramework.Native;

namespace ConsoleFramework
{
    public sealed class ControlRenderingBuffer {
        private readonly Control control;
        public Control Control {
            get {
                return control;
            }
        }

        private bool contentRendered;
        public bool ContentRendered {
            get {
                return contentRendered;
            }
        }

        // только свой контент (у панели например это будет пустой баффер)
        private CHAR_INFO[,] selfBuffer;
        // свой контент с наложенным на него контентом дочерних элементов (полностью готовый для отображения баффер)
        private CHAR_INFO[,] buffer;
        private Renderer renderer;

        public ControlRenderingBuffer(Control control, Renderer renderer) {
            if (null == control) {
                throw new ArgumentNullException("control");
            }
            if (null == renderer) {
                throw new ArgumentNullException("renderer");
            }
            //
            this.control = control;
            this.renderer = renderer;
            selfBuffer = new CHAR_INFO[control.RenderSize.Width, control.RenderSize.Height];
        }

        public void RenderSelfOnly() {
            if (ContentRendered && control.LayoutIsValid) {
                return;
            }
            //
            control.Render(this);
            //
            contentRendered = true;
        }

        // рекурсивно вызывает Render() у чилдов контрола, а потом их бафферы применяет к копии своего баффера
        public void Render() {
            List<Control> children = control.children;
            foreach (Control child in children) {
                renderer.Render(child);
                renderer.GetControlRenderingBuffer(child);
            }
        }

        public void ApplyToPhysicalCanvas(int x, int y) {
            //
        }
    }
}
