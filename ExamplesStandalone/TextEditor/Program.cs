using ConsoleFramework;
using ConsoleFramework.Controls;

namespace Examples.TextEditor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WindowsHost windowsHost = new WindowsHost();
            Window mainWindow = (Window)ConsoleApplication.LoadFromXaml("TextEditor.main.xml", null);
            windowsHost.Show(mainWindow);
            ConsoleApplication.Instance.Run(windowsHost);
        }
    }
}

//using System;
//using ConsoleFramework;
//using ConsoleFramework.Controls;
//using ConsoleFramework.Core;
//using ConsoleFramework.Native;
//using ConsoleFramework.Rendering;

//namespace ConsoleFrameworkApp
//{
//    class Program
//    {
//        static void Main()
//        {
//            var window = new Panel
//            {
//                Width = 20,
//                Height = 2,
//                HorizontalAlignment = HorizontalAlignment.Left,
//                XChildren =
//                {
//                    new Fill
//                    {
//                        HorizontalAlignment = HorizontalAlignment.Stretch,
//                        Char = '_',
//                        Attr = Attr.FOREGROUND_BLUE | Attr.FOREGROUND_INTENSITY | Attr.BACKGROUND_BLUE,
//                        XChildren =
//                        {
//                            new FillAlphabet
//                            {
//                                AlphaWidth = 3,
//                                AlphaHeight = 2,
//                                MaxWidth = 7,
//                                Margin = new Thickness(-3, 0, 0, 0),
//                                HorizontalAlignment = HorizontalAlignment.Stretch,
//                                Attr = Attr.FOREGROUND_RED | Attr.FOREGROUND_INTENSITY | Attr.BACKGROUND_RED,
//                            }
//                        }
//                    }
//                }
//            };
//            ConsoleApplication.Instance.Run(window);
//        }
//    }

//    class Fill : Panel
//    {
//        public char Char { get; set; }
//        public Attr Attr { get; set; }

//        public override void Render(RenderingBuffer buffer)
//        {
//            buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, Char, Attr);
//        }
//    }

//    class FillAlphabet : Fill
//    {
//        public int AlphaWidth { get; set; }
//        public int AlphaHeight { get; set; }

//        public FillAlphabet()
//        {
//            Char = '-';
//        }

//        protected override Size MeasureOverride(Size availableSize) => new Size(AlphaWidth, AlphaHeight);

//        public override void Render(RenderingBuffer buffer)
//        {
//            base.Render(buffer);
//            char nextChar = 'a';
//            for (int y = 0; y < AlphaHeight; y++)
//                for (int x = 0; x < AlphaWidth; x++)
//                    buffer.SetPixel(x, y, nextChar++, Attr);
//        }

//        //protected override Size MeasureOverride(Size availableSize) {
//        //    return new Size(Math.Min(availableSize.Width, AlphaWidth),
//        //        Math.Min(availableSize.Height, AlphaHeight));
//        //}

//        //public override void Render(RenderingBuffer buffer)
//        //{
//        //    base.Render(buffer);
//        //    char nextChar = 'a';
//        //    for (int y = 0; y < Math.Min(AlphaHeight, buffer.Height); y++)
//        //    for (int x = 0; x < Math.Min(AlphaWidth, buffer.Width); x++)
//        //        buffer.SetPixel(x, y, nextChar++, Attr);
//        //}
//    }
//}