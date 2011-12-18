//using System;
//using System.Collections.Generic;
//using ConsoleFramework.Native;

//namespace ConsoleFramework
//{
//    /// <summary>
//    /// Хранит в себе стек окон, по запросам, приходящим из ConsoleApplication, обрабатывает ввод пользователя,
//    /// вычисляя, какому окно был предназначен этот ввод и перенаправляя событие ввода этому окну.
//    /// </summary>
//    public sealed class ConsoleDispatcher : IElementsVisibilityAware {
//        public List<FrameworkElement> WindowsStack = new List<FrameworkElement>();
//        private readonly Dictionary<FrameworkElement, Visibility> visibilityCache = new Dictionary<FrameworkElement, Visibility>();
//        public bool visibilityAnalyzeCompleted = false;

//        public FrameworkElementVisibility GetElementVisibility(FrameworkElement element) {
//            return getElementVisibilityCore(element).Type;
//        }

//        private Visibility getElementVisibilityCore(FrameworkElement element) {
//            if (!visibilityAnalyzeCompleted) {
//                AnalyzeVisibility();
//            }
//            Visibility res;
//            if (!visibilityCache.TryGetValue(element, out res)) {
//                throw new InvalidOperationException("Element was not found at visibility cache.");
//            }
//            return res;
//        }

//        public bool IsPointOfElementVisible(int x, int y, FrameworkElement element) {
//            if (x < 0 || y < 0 || x > element.Width || y > element.Height) {
//                throw new ArgumentException("Coords to point are invalid.");
//            }
//            //
//            Visibility visibility = getElementVisibilityCore(element);
//            if (visibility.Type == FrameworkElementVisibility.FullVisible) {
//                return true;
//            }
//            if (visibility.Type == FrameworkElementVisibility.Hidden) {
//                return false;
//            }
//            //
//            List<SMALL_RECT> overlappedRegions = visibility.OverlappedRegions;
//            foreach (SMALL_RECT rect in overlappedRegions) {
//                // если точка лежит внутри одной из областей, перекрытых верхними окнами - false
//                if (x >= rect.Left && x <= rect.Right && y >= rect.Top && y <= rect.Bottom) {
//                    return false;
//                }
//            }
//            return true;
//        }

//        public void DispatchInputEvent(INPUT_RECORD inputRecord) {
//            //
//        }

//        public void RegisterWindow(FrameworkElement element) {
//            WindowsStack.Add(element);
//            if (visibilityAnalyzeCompleted) {
//                visibilityAnalyzeCompleted = false;
//            }
//        }

//        public void AnalyzeVisibility() {
//            if (visibilityAnalyzeCompleted) {
//                return;
//            }
//            //
//            if (WindowsStack.Count > 0) {
//                for (int i = WindowsStack.Count - 1; i >= 0; --i) {
//                    FrameworkElement element = WindowsStack[i];
//                    Visibility visibility;
//                    if (!visibilityCache.TryGetValue(element, out visibility)) {
//                        visibility = new Visibility(FrameworkElementVisibility.FullVisible);
//                        visibilityCache.Add(element, visibility);
//                    } else {
//                        visibility.Type = FrameworkElementVisibility.FullVisible;
//                        visibility.OverlappedRegions.Clear();
//                    }
//                    //
//                    for (int j = WindowsStack.Count - 1; j > i; --j) {
//                        FrameworkElement element2 = WindowsStack[j];
//                        if (visibilityCache[element2].Type == FrameworkElementVisibility.Hidden) {
//                            continue;
//                        }
//                        SMALL_RECT? overlappingRegion = getOverlappingRegion(element, element2);
//                        if (overlappingRegion != null) {
//                            SMALL_RECT rect = overlappingRegion.Value;
//                            if (rect.Left == 0 && rect.Top == 0 && rect.Right == element.Width && rect.Bottom == element.Height) {
//                                visibility.Type = FrameworkElementVisibility.Hidden;
//                                break;
//                            }
//                            visibility.OverlappedRegions.Add(rect);
//                            visibility.Type = FrameworkElementVisibility.PartiallyVisible;
//                        }
//                    }
//                }
//            }
//            visibilityAnalyzeCompleted = true;
//        }

//        /// <summary>
//        /// Проверяет, перекрывает ли b, находящийся сверху, элемент a, находящийся под ним.
//        /// Возвращает координаты прямоугольной области относительно элемента a, если b его перекрывает.
//        /// Или null в случае, если b не перекрывает a вообще.
//        /// </summary>
//        public static SMALL_RECT? getOverlappingRegion(FrameworkElement a, FrameworkElement b) {
//            SMALL_RECT? res = null;
//            //
//            bool firstIsLeft = a.X <= b.X;
//            FrameworkElement leftElement = firstIsLeft ? a : b;
//            FrameworkElement rightElement = firstIsLeft ? b : a;
//            // вертикальные составляющие прямоугольников - это области между двумя параллельными прямыми,
//            // одна из которых образована левой границей фигуры, а другая - правой
//            bool verticalIntersects = leftElement.X <= rightElement.X &&
//                                      rightElement.X <= leftElement.X + leftElement.Width;
//            bool firstIsTop = a.Y <= b.Y;
//            FrameworkElement topElement = firstIsTop ? a : b;
//            FrameworkElement bottomElement = firstIsTop ? b : a;
//            // аналогично определяются горизонтальные составляющие прямоугольников
//            bool horizontalIntersects = topElement.Y <= bottomElement.Y &&
//                                        bottomElement.Y <= topElement.Y + topElement.Height;
//            // если пересекаются и горизонтальные, и вертикальные составляющие - мы имеем
//            // пересечение прямоугольников, и можем получить координаты области пересечения
//            if (verticalIntersects && horizontalIntersects) {
//                // получаем глобальные координаты области пересечения
//                int intersectionLeft = rightElement.X;
//                int intersectionRight = Math.Min(leftElement.X + leftElement.Width, rightElement.X + rightElement.Width);
//                int intersectionTop = bottomElement.Y;
//                int intersectionBottom = Math.Min(topElement.Y + topElement.Height,
//                                                  bottomElement.Y + bottomElement.Height);
//                // возвращаем координаты области пересечения относительно a
//                res = new SMALL_RECT((short) (intersectionLeft - a.X), (short) (intersectionTop - a.Y),
//                                     (short) (intersectionRight - a.X), (short) (intersectionBottom - a.Y));
//            }
//            return res;
//        }
//    }

//    internal class Visibility {
//        private List<SMALL_RECT> overlappedRegions;
//        public List<SMALL_RECT> OverlappedRegions {
//            get {
//                return overlappedRegions ?? (overlappedRegions = new List<SMALL_RECT>());
//            }
//        }

//        public FrameworkElementVisibility Type {
//            get;
//            set;
//        }

//        public Visibility() {
//        }

//        public Visibility(FrameworkElementVisibility type) {
//            this.Type = type;
//        }
//    }
//}
