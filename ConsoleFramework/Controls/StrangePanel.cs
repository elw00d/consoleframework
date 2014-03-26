using ConsoleFramework.Core;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Панель, которая размещает дочерние контролы в слотах, которые ей самой недоступны.
    /// </summary>
    public class StrangePanel : Control
    {
        public Control Content {
            get {
                return Children.Count != 0 ? Children[0] : null;
            }
            set {
                if (Children.Count != 0) {
                    RemoveChild(Children[0]);
                }
                AddChild(value);
            }
        }

        protected override Size MeasureOverride(Size availableSize) {
            Content.Measure(availableSize);
            return new Size(10, 10);
        }

        protected override Size ArrangeOverride(Size finalSize) {
            if (Content != null) {
                Content.Arrange(new Rect(0, 0, 20, 20));
            }
            return base.ArrangeOverride(finalSize);
            //return new Size(20, 20);
        }

        public override void Render(RenderingBuffer buffer) {
            buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, '-', Attr.BACKGROUND_GREEN);
        }

    }
}
