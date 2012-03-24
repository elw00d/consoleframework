using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleFramework.Core;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Класс, служащий хост-панелью для набора перекрывающихся окон.
    /// Хранит в себе список окон в порядке их Z-Order и отрисовывает рамки,
    /// управляет их перемещением.
    /// Также может содержать контролы прямо на панели, как и другие панели (чтобы заполнять пустое пространство).
    /// Прорисовываются они первыми, потом прорисовываются окна.
    /// События к ним доставляются только в том случае, если они не перекрыты окнами.
    /// </summary>
    public class WindowsHost : Control
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            // дочерние окна могут занимать сколько угодно пространства
            foreach (Control control in children)
            {
                Window window = (Window) control;
                window.Measure(availableSize);
            }
            if (availableSize.width == int.MaxValue && availableSize.height == int.MaxValue)
                return new Size(availableSize.width - 1, availableSize.height - 1);
            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // сколько дочерние окна хотели - столько и получают
            foreach (Control control in children)
            {
                Window window = (Window) control;
                window.Arrange(new Rect(window.X, window.Y, window.DesiredSize.Width, window.DesiredSize.Height));
            }
            return finalSize;
        }

        public override void Render(RenderingBuffer buffer)
        {
            buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, ' ', CHAR_ATTRIBUTES.BACKGROUND_BLUE);
        }

        public override void HandleEvent(INPUT_RECORD inputRecord)
        {
            base.HandleEvent(inputRecord);
        }

        public void AddWindow(Window window)
        {
            // todo : определить, нужно ли вызывать Invalidate при добавлении дочерних элементов
            // может быть, следует это делать автоматически при вызове base.AddChild() ?
            AddChild(window);
            //Invalidate();
        }
    }
}
