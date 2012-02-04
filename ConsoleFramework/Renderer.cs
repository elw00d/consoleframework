using System;
using System.Collections.Generic;
using ConsoleFramework.Controls;

namespace ConsoleFramework
{
    public sealed class Renderer
    {
        private readonly Dictionary<Control, ControlRenderingBuffer> buffers = new Dictionary<Control, ControlRenderingBuffer>();

        public void Render(Control rootElement) {
            if (null == rootElement)
                throw new ArgumentNullException("rootElement");
            //
            if (rootElement.LayoutIsValid)
                return;
            rootElement.Draw();
            //
            int count = rootElement.children.Count;
            for (int i = 0; i < count; ++i) {
                Control child = rootElement.children[i];
                Render(child);
            }
        }

        public ControlRenderingBuffer GetControlRenderingBuffer(Control control) {
            if (null == control) {
                throw new ArgumentNullException("control");
            }
            ControlRenderingBuffer res;
            if (!buffers.TryGetValue(control, out res)) {
                res = new ControlRenderingBuffer(control, this);
                buffers.Add(control, res);
            }
            return res;
        }
    }
}
