using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleFramework.Core;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// MeasureOverride этого контрола возвращает 10х10, а
    /// ArrangeOverride - 5x5
    /// </summary>
    public class StrangeControl : Control
    {
        protected override Size MeasureOverride(Size availableSize) {
            return new Size(20, 20);
        }

        protected override Size ArrangeOverride(Size finalSize) {
            //return new Size(5, 5);
            return base.ArrangeOverride(finalSize);
        }

        public override void Render(RenderingBuffer buffer) {
            buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, '0', CHAR_ATTRIBUTES.FOREGROUND_RED);
        }
    }
}
