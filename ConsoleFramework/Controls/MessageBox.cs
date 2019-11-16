using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleFramework.Core;
using ConsoleFramework.Events;

namespace ConsoleFramework.Controls
{
    public delegate void MessageBoxClosedEventHandler(MessageBoxResult result);

    public class MessageBox : Window
    {
        private readonly TextBlock textBlock;

        public MessageBox( ) {
            Panel panel = new Panel();
            textBlock = new TextBlock();
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Margin = new Thickness(1);
            Button button = new Button(  );
            button.Margin = new Thickness(4, 0, 4, 0);
            button.HorizontalAlignment = HorizontalAlignment.Center;
            button.Caption = "OK";
            button.OnClick+=CloseButtonOnClicked;
            panel.Children.Add( textBlock );
            panel.Children.Add( button );
            panel.HorizontalAlignment = HorizontalAlignment.Center;
            panel.VerticalAlignment = VerticalAlignment.Bottom;
            this.Content = panel;
        }

        protected virtual void CloseButtonOnClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public string Text {
            get { return textBlock.Text; }
            set { textBlock.Text = value; }
        }

        public static void Show( string title, string text, MessageBoxClosedEventHandler onClosed) {
            Control rootControl = ConsoleApplication.Instance.RootControl;
            if (!(rootControl is WindowsHost)) 
                throw new InvalidOperationException("Default windows host not found, create MessageBox manually");
            WindowsHost windowsHost = ( WindowsHost ) rootControl;
            MessageBox messageBox = new MessageBox(  );
            messageBox.Title = title;
            messageBox.Text = text;
            messageBox.AddHandler( ClosedEvent, new EventHandler(( sender, args ) => {
                if ( null != onClosed ) {
                    onClosed( MessageBoxResult.Button1 );
                }
            }) );
            //messageBox.X =
            windowsHost.ShowModal( messageBox );
        }
    }

    public enum MessageBoxResult
    {
        Button1,
        Button2,
        Button3
    }
}
