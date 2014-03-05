using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ConsoleFramework;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;
using ConsoleFramework.Xaml;
using Xaml;

namespace Examples
{
    internal class Program
    {
    class MyDataContext : INotifyPropertyChanged
        {
            private string str;
            public String Str {
                get { return str; }
                set {
                    if ( str != value ) {
                        str = value;
                        raisePropertyChanged( "Str" );
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void raisePropertyChanged( string propertyName ) {
                PropertyChangedEventHandler handler = PropertyChanged;
                if ( handler != null ) handler( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        private static void Main(string[] args) {
//            Control window = ConsoleApplication.LoadFromXaml( "ConsoleFramework.Layout.xml", null );
////            window.FindChildByName< TextBlock >( "text" ).MouseDown += ( sender, eventArgs ) => {
////                window.FindChildByName< TextBlock >( "text" ).Text = "F";
////                eventArgs.Handled = true;
////            };
////            window.MouseDown += ( sender, eventArgs ) => {
////                window.Width = window.ActualWidth + 3;
////                window.Invalidate(  );
////            };
//            ConsoleApplication.Instance.Run( window );
//            return;

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Examples.GridTest.xml";
            Window createdFromXaml;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                // todo : try to specify dataContext
                MyDataContext dataContext = new MyDataContext( );
                dataContext.Str = "Введите заголовок";
                createdFromXaml = XamlParser.CreateFromXaml<Window>(result, dataContext, new List<string>()
                    {
                        "clr-namespace:Xaml;assembly=Xaml",
                        "clr-namespace:ConsoleFramework.Xaml;assembly=ConsoleFramework",
                        "clr-namespace:ConsoleFramework.Controls;assembly=ConsoleFramework",
                    });
            }
//            ConsoleApplication.Instance.Run(createdFromXaml);
//            return;

            using (ConsoleApplication application = ConsoleApplication.Instance) {
                Panel panel = new Panel();
                panel.Name = "panel1";
                panel.HorizontalAlignment =  HorizontalAlignment.Center;
                panel.VerticalAlignment = VerticalAlignment.Stretch;
                panel.AddChild(new TextBlock() {
                    Name = "label1",
                    Text = "Label1",
                    Margin = new Thickness(1,2,1,0)
                    //,Visibility = Visibility.Collapsed
                });
                panel.AddChild(new TextBlock() {
                    Name = "label2",
                    Text = "Label2_____",
                    HorizontalAlignment = HorizontalAlignment.Right
                });
                TextBox textBox = new TextBox() {
                    Width = 15,
                    Size = 10
                };
                Button button = new Button() {
                    Name = "button1",
                    Caption = "Button!",
                    Margin = new Thickness(1),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                button.OnClick += (sender, eventArgs) => {
                    Debug.WriteLine("Click");
                    MessageBox.Show( "Окно сообщения", "Внимание ! Тестовое сообщение", delegate( MessageBoxResult result ) {  } );
                    Control label = panel.FindDirectChildByName("label1");
                    if (label.Visibility == Visibility.Visible) {
                        label.Visibility = Visibility.Collapsed;
                    } else if (label.Visibility == Visibility.Collapsed) {
                        label.Visibility = Visibility.Hidden;
                    } else {
                        label.Visibility = Visibility.Visible;
                    }
                    label.Invalidate();
                };
                ComboBox comboBox = new ComboBox(  )
                    {
//                        Width = 14
//HorizontalAlignment = HorizontalAlignment.Stretch
                    };
                comboBox.Items.Add( "Сделать одно" );
                comboBox.Items.Add("Сделать второе");
                comboBox.Items.Add("Ничего не делать");
                ListBox listbox = new ListBox(  );
                listbox.Items.Add( "First item" );
                listbox.Items.Add( "second item1!!!!!!1fff" );
                listbox.HorizontalAlignment = HorizontalAlignment.Stretch;
                //listbox.Width = 10;

                panel.AddChild(comboBox);
                panel.AddChild(button);
                panel.AddChild(textBox);
                panel.AddChild( listbox );
                
                //application.Run(panel);
                WindowsHost windowsHost = new WindowsHost()
                                              {
                                                  Name = "WindowsHost"
                                              };
                Window window1 = new Window {
                    X = 5,
                    Y = 4,
                    //MinHeight = 100,
                    //MaxWidth = 30,
                    //Width = 10,
                    Height = 20,
                    Name = "Window1",
                    Title = "Window1",
                    Content = panel
                };
                GroupBox groupBox = new GroupBox(  );
                groupBox.Title = "Группа";
                ScrollViewer scrollViewer = new ScrollViewer(  );
                ListBox listBox = new ListBox(  );
                listBox.Items.Add( "Длинный элемент" );
                listBox.Items.Add("Длинный элемент 2");
                listBox.Items.Add("Длинный элемент 3");
                listBox.Items.Add("Длинный элемент 4");
                listBox.Items.Add("Длинный элемент 5");
                listBox.Items.Add("Длинный элемент 6");
                listBox.Items.Add("Длинный элемент 700");
                listBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                listBox.VerticalAlignment = VerticalAlignment.Stretch;
                scrollViewer.Content = listBox;
//                scrollViewer.HorizontalAlignment = HorizontalAlignment.Stretch;
                scrollViewer.VerticalAlignment = VerticalAlignment.Stretch;
                scrollViewer.HorizontalScrollEnabled = false;

                groupBox.Content = scrollViewer;
                groupBox.HorizontalAlignment = HorizontalAlignment.Stretch;

                windowsHost.Show(new Window() {
                    X = 30,
                    Y = 6,
                    //MinHeight = 10,
                    //MinWidth = 10,
                    Name = "Window2",
                    Title = "Очень длинное название окна",
                    Content = groupBox
                });
                windowsHost.Show(new Window() {
                    X = 30,
                    Y = 15,
                    Name = "window 3",
                    Content = new StrangePanel() {
                        Content = new StrangeControl()
                    }
                });
                windowsHost.Show(window1);
                windowsHost.Show(createdFromXaml);
                //textBox.SetFocus(); todo : научиться задавать фокусный элемент до добавления в визуальное дерево
                //application.TerminalSizeChanged += ( sender, eventArgs ) => {
                //    application.CanvasSize = new Size(eventArgs.Width, eventArgs.Height);
                //   application.RootElementRect = new Rect(new Size(eventArgs.Width, eventArgs.Height));
               // };
				//windowsHost.Width = 80;
				//windowsHost.Height = 20;
				application.Run(windowsHost);//, new Size(80, 30), Rect.Empty);
            }
        }
    }
}
