using ConsoleFramework.Controls;
using ConsoleFramework.Core;
using Xunit;

namespace Tests
{
    public class LayoutClipTests
    {
        [Theory]
        [InlineData(2, 3, HorizontalAlignment.Center, VerticalAlignment.Center, 4, 3)]
        [InlineData(2, 3, HorizontalAlignment.Left, VerticalAlignment.Center, 0, 3)]
        [InlineData(2, 3, HorizontalAlignment.Right, VerticalAlignment.Center, 8, 3)]
        [InlineData(2, 3, HorizontalAlignment.Stretch, VerticalAlignment.Center, 4, 3)]
        [InlineData(2, 3, HorizontalAlignment.Center, VerticalAlignment.Top, 4, 0)]
        [InlineData(2, 3, HorizontalAlignment.Center, VerticalAlignment.Bottom, 4, 7)]
        [InlineData(2, 3, HorizontalAlignment.Center, VerticalAlignment.Stretch, 4, 3)]
        [InlineData(2, 3, HorizontalAlignment.Left, VerticalAlignment.Top, 0, 0)]
        [InlineData(2, 3, HorizontalAlignment.Left, VerticalAlignment.Bottom, 0, 7)]
        [InlineData(2, 3, HorizontalAlignment.Left, VerticalAlignment.Stretch, 0, 3)]
        [InlineData(2, 3, HorizontalAlignment.Right, VerticalAlignment.Top, 8, 0)]
        [InlineData(2, 3, HorizontalAlignment.Right, VerticalAlignment.Bottom, 8, 7)]
        [InlineData(2, 3, HorizontalAlignment.Right, VerticalAlignment.Stretch, 8, 3)]
        [InlineData(2, 3, HorizontalAlignment.Stretch, VerticalAlignment.Stretch, 4, 3)]
        [InlineData(3, 2, HorizontalAlignment.Center, VerticalAlignment.Center, 3, 4)]
        // inkSize>clientSize -> getting negative offset
        [InlineData(25, 15, HorizontalAlignment.Center, VerticalAlignment.Center, -7, -2)]
        // inkSize>clientSize and mode is Stretch -> degenerating to TopLeft
        [InlineData(25, 15, HorizontalAlignment.Stretch, VerticalAlignment.Stretch, 0, 0)]
        // inkSize>clientSize and mode is Right -> getting negative offset
        [InlineData(25, 15, HorizontalAlignment.Right, VerticalAlignment.Bottom, -15, -5)]
        public void TestAlignmentOffsetCore(int inkWidth, int inkHeight,
            HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment,
            int expectedX, int expectedY)
        {
            Control control = new Control {
                HorizontalAlignment = horizontalAlignment,
                VerticalAlignment = verticalAlignment
            };
            Size inkSize = new Size(inkWidth, inkHeight);
            Vector offset = control.computeAlignmentOffsetCore(new Size(10, 10), inkSize);
            Assert.Equal(new Vector(expectedX, expectedY), offset);
        }

        [Fact]
        public void TestApplyMaxConstraints() {
            Control control = new Control {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MaxWidth = 3,
                layoutInfo = new LayoutInfo {
                    renderSize = new Size(10, 1)
                }
            };
            Rect layoutClip = control.applyMaxConstraints(new Rect(-10, -10, 20, 20));
            // If Max constraint is present, the layoutClip will be clipped to
            // visualLayoutClip: a rect starting from (0, 0) and with size of (MaxWidth, MaxHeight)
            Assert.Equal(new Rect(0, 0, 3, 1), layoutClip);
        }
    }
}
