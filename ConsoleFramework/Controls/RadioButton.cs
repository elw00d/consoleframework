using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;

namespace ConsoleFramework.Controls
{
    public class RadioGroup : Panel
    {
        private int? selectedItemIndex;
        public int? SelectedItemIndex
        {
            get { return selectedItemIndex; }
            set {
                if (selectedItemIndex != value) {
                    selectedItemIndex = value;
                    RaisePropertyChanged("SelectedItemIndex");
                    RaisePropertyChanged("SelectedItem");
                }
            }
        }

        public RadioButton SelectedItem
        {
            get { return selectedItemIndex.HasValue ? (RadioButton) Children[selectedItemIndex.Value] : null; }
        }

        public RadioGroup() {
            XChildren.ControlAdded += onControlAdded;
            XChildren.ControlRemoved -= onControlRemoved;
        }

        private void onControlRemoved(Control control) {
            if (!(control is RadioButton)) return;
            var radioButton = (RadioButton)control;
            radioButton.OnClick -= radioButton_OnClick;
        }

        private void onControlAdded(Control control) {
            if (!(control is RadioButton)) return;
            var radioButton = (RadioButton) control;
            radioButton.OnClick += radioButton_OnClick;
            int index = Children.IndexOf(radioButton);
            radioButton.Checked = selectedItemIndex != null && (selectedItemIndex == index);
        }

        private void radioButton_OnClick(object sender, RoutedEventArgs args) {
            foreach (var child in XChildren) {
                if (child is RadioButton && child != sender) {
                    ((RadioButton) child).Checked = false;
                }
            }
            ((RadioButton) sender).Checked = true;
            int index = Children.IndexOf((Control) sender);
            SelectedItemIndex = index;
        }
    }

    public class RadioButton : CheckBox
    {
        public override void Render(RenderingBuffer buffer)
        {
            Attr captionAttrs;
            if (HasFocus)
                captionAttrs = Colors.Blend(Color.White, Color.DarkGreen);
            else
                captionAttrs = Colors.Blend(Color.Black, Color.DarkGreen);

            Attr buttonAttrs = captionAttrs;
            //            if ( pressed )
            //                buttonAttrs = Colors.Blend(Color.Black, Color.DarkGreen);

            buffer.SetOpacityRect(0, 0, ActualWidth, ActualHeight, 3);

            buffer.SetPixel(0, 0, pressed ? '<' : '(', buttonAttrs);
            buffer.SetPixel(1, 0, Checked ? checkedChar : ' ', buttonAttrs);
            buffer.SetPixel(2, 0, pressed ? '>' : ')', buttonAttrs);
            buffer.SetPixel(3, 0, ' ', buttonAttrs);
            if (null != Caption)
                RenderString(Caption, buffer, 4, 0, ActualWidth - 4, captionAttrs);
        }
    }
}
