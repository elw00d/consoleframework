using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Represents a control that a user can select and clear.
    /// </summary>
    public class CheckBox : ButtonBase
    {
        public CheckBox( ) {
            OnClick += CheckBox_OnClick;
        }

        private void CheckBox_OnClick( object sender, RoutedEventArgs routedEventArgs ) {
            Checked = !Checked;
        }

        protected char checkedChar = 'X';

        private string caption;
        public string Caption {
            get {
                return caption;
            }
            set {
                if ( caption != value ) {
                    caption = value;
                    Invalidate( );
                }
            }
        }

        private bool isChecked;
        public bool Checked {
            get {
                return isChecked;
            }
            set {
                if ( isChecked != value ) {
                    isChecked = value;
                    RaisePropertyChanged("Checked");
                    Invalidate(  );
                }
            }
        }

        public string CheckedChar { get => checkedChar.ToString(); set => checkedChar = char.Parse(value); }

        protected override Size MeasureOverride(Size availableSize) {
            if (!string.IsNullOrEmpty(caption)) {
                Size minButtonSize = new Size(caption.Length + 4, 1);
                return minButtonSize;
            } else return new Size(8, 1);
        }

        public override void Render(RenderingBuffer buffer) {
            Attr captionAttrs;
            if (HasFocus)
                captionAttrs = Colors.Blend(Color.White, Color.DarkGreen);
            else
                captionAttrs = Colors.Blend(Color.Black, Color.DarkGreen);

            Attr buttonAttrs = captionAttrs;
//            if ( pressed )
//                buttonAttrs = Colors.Blend(Color.Black, Color.DarkGreen);

            buffer.SetOpacityRect( 0, 0, ActualWidth, ActualHeight, 3 );

            buffer.SetPixel(0, 0, pressed ? '<' : '[', buttonAttrs);
            buffer.SetPixel(1, 0, Checked ? checkedChar : ' ', buttonAttrs);
            buffer.SetPixel(2, 0, pressed ? '>' : ']', buttonAttrs);
            buffer.SetPixel(3, 0, ' ', buttonAttrs);
            if (null != caption)
                RenderString( caption, buffer, 4, 0, ActualWidth - 4, captionAttrs );
        }
    }
}
