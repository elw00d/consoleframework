using ConsoleFramework.Core;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Панель, которая размещает дочерние контролы в слотах, которые ей самой недоступны.
    /// </summary>
    public class StrangePanel : Control
    {
        public Control Content {
            get {
                return children.Count != 0 ? children[0] : null;
            }
            set {
                if (children.Count != 0) {
                    RemoveChild(children[0]);
                }
                AddChild(value);
            }
        }

        protected override Core.Size MeasureOverride(Core.Size availableSize) {
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
            buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, '-', CHAR_ATTRIBUTES.BACKGROUND_GREEN);
        }

    }
}
