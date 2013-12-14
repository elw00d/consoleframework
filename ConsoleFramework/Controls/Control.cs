using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;

namespace ConsoleFramework.Controls
{
    public enum Visibility
    {
        Collapsed = 3,
        Hidden = 2,
        Visible = 1
    }

    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right,
        Stretch
    }


    public enum VerticalAlignment
    {
        Top,
        Center,
        Bottom,
        Stretch
    }

    internal enum LayoutValidity {
        Nothing = 1,
        MeasureAndArrange = 2,
        Render = 3
    }

    /// <summary>
    /// Полностью описывает состояние лайаута контрола.
    /// </summary>
    internal class LayoutInfo : IEquatable<LayoutInfo> {
        public Size measureArgument;
        // если это поле не изменилось, то можно считать, что контрол не поменял своего размера
        public Size unclippedDesiredSize;
        public Size desiredSize;
        // по сути это arrangeArgument
        public Rect renderSlotRect;
        public Size renderSize;
        public Rect layoutClip;
        public Vector actualOffset;
        public LayoutValidity validity = LayoutValidity.Nothing;

        public void CopyValuesFrom(LayoutInfo layoutInfo) {
            this.measureArgument = layoutInfo.measureArgument;
            this.unclippedDesiredSize = layoutInfo.unclippedDesiredSize;
            this.desiredSize = layoutInfo.desiredSize;
            this.renderSlotRect = layoutInfo.renderSlotRect;
            this.renderSize = layoutInfo.renderSize;
            this.layoutClip = layoutInfo.layoutClip;
            this.actualOffset = layoutInfo.actualOffset;
            this.validity = layoutInfo.validity;
        }

        public void ClearValues() {
            this.measureArgument = new Size();
            this.unclippedDesiredSize = new Size();
            this.desiredSize = new Size();
            this.renderSlotRect = new Rect();
            this.renderSize = new Size();
            this.layoutClip = new Rect();
            this.actualOffset = new Vector();
            this.validity = LayoutValidity.Nothing;
        }

        public bool Equals(LayoutInfo other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.measureArgument.Equals(measureArgument) && other.unclippedDesiredSize.Equals(unclippedDesiredSize) && other.desiredSize.Equals(desiredSize) && other.renderSlotRect.Equals(renderSlotRect) && other.renderSize.Equals(renderSize) && other.layoutClip.Equals(layoutClip) && other.actualOffset.Equals(actualOffset);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (LayoutInfo)) return false;
            return Equals((LayoutInfo) obj);
        }
    }

    /// <summary>
    /// Base class for all controls.
    /// </summary>
    public class Control : INotifyPropertyChanged {

        public Object DataContext { get; set; }

        private Dictionary< String, Object > resources;
        public Dictionary< String, Object > Resources {
            get { return resources ?? ( resources = new Dictionary< string, object >( ) ); }
        }

        public static RoutedEvent PreviewMouseMoveEvent = EventManager.RegisterRoutedEvent("PreviewMouseMove", RoutingStrategy.Tunnel, typeof(MouseEventHandler), typeof(Control));
        public static RoutedEvent MouseMoveEvent = EventManager.RegisterRoutedEvent("MouseMove", RoutingStrategy.Bubble, typeof(MouseEventHandler), typeof(Control));
        public static RoutedEvent PreviewMouseDownEvent = EventManager.RegisterRoutedEvent("PreviewMouseDown", RoutingStrategy.Tunnel, typeof(MouseButtonEventHandler), typeof(Control));
        public static RoutedEvent MouseDownEvent = EventManager.RegisterRoutedEvent("MouseDown", RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(Control));
        public static RoutedEvent PreviewMouseUpEvent = EventManager.RegisterRoutedEvent("PreviewMouseUp", RoutingStrategy.Tunnel, typeof(MouseButtonEventHandler), typeof(Control));
        public static RoutedEvent MouseUpEvent = EventManager.RegisterRoutedEvent("MouseUp", RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(Control));
        public static RoutedEvent PreviewMouseWheelEvent = EventManager.RegisterRoutedEvent("PreviewMouseWheel", RoutingStrategy.Tunnel, typeof(MouseWheelEventHandler), typeof(Control));
        public static RoutedEvent MouseWheelEvent = EventManager.RegisterRoutedEvent("MouseWheel", RoutingStrategy.Bubble, typeof(MouseWheelEventHandler), typeof(Control));
        public static RoutedEvent MouseEnterEvent = EventManager.RegisterRoutedEvent("MouseEnter", RoutingStrategy.Direct, typeof(MouseEventHandler), typeof(Control));
        public static RoutedEvent MouseLeaveEvent = EventManager.RegisterRoutedEvent("MouseLeave", RoutingStrategy.Direct, typeof(MouseEventHandler), typeof(Control));

        public static RoutedEvent PreviewKeyDownEvent = EventManager.RegisterRoutedEvent("PreviewKeyDown", RoutingStrategy.Tunnel, typeof(KeyEventHandler), typeof(Control));
        public static RoutedEvent KeyDownEvent = EventManager.RegisterRoutedEvent("KeyDown", RoutingStrategy.Bubble, typeof(KeyEventHandler), typeof(Control));
        public static RoutedEvent PreviewKeyUpEvent = EventManager.RegisterRoutedEvent("PreviewKeyUp", RoutingStrategy.Tunnel, typeof(KeyEventHandler), typeof(Control));
        public static RoutedEvent KeyUpEvent = EventManager.RegisterRoutedEvent("KeyUp", RoutingStrategy.Bubble, typeof(KeyEventHandler), typeof(Control));

        public static RoutedEvent PreviewLostKeyboardFocusEvent = EventManager.RegisterRoutedEvent("PreviewLostKeyboardFocus", RoutingStrategy.Tunnel, typeof(KeyboardFocusChangedEventHandler), typeof(Control));
        public static RoutedEvent LostKeyboardFocusEvent = EventManager.RegisterRoutedEvent("LostKeyboardFocus", RoutingStrategy.Bubble, typeof(KeyboardFocusChangedEventHandler), typeof(Control));
        public static RoutedEvent PreviewGotKeyboardFocusEvent = EventManager.RegisterRoutedEvent("PreviewGotKeyboardFocus", RoutingStrategy.Tunnel, typeof(KeyboardFocusChangedEventHandler), typeof(Control));
        public static RoutedEvent GotKeyboardFocusEvent = EventManager.RegisterRoutedEvent("GotKeyboardFocus", RoutingStrategy.Bubble, typeof(KeyboardFocusChangedEventHandler), typeof(Control));

        public event MouseEventHandler MouseMove {
            add { AddHandler(MouseMoveEvent, value); }
            remove { RemoveHandler(MouseMoveEvent, value); }
        }

        public event MouseButtonEventHandler MouseDown {
            add { AddHandler(MouseDownEvent, value); }
            remove { RemoveHandler(MouseDownEvent, value); }
        }

        public event MouseButtonEventHandler MouseUp {
            add { AddHandler(MouseUpEvent, value); }
            remove { RemoveHandler(MouseUpEvent, value); }
        }

        public event MouseEventHandler MouseEnter {
            add { AddHandler(MouseEnterEvent, value); }
            remove { RemoveHandler(MouseEnterEvent, value); }
        }

        public event MouseEventHandler MouseLeave {
            add { AddHandler(MouseLeaveEvent, value); }
            remove { RemoveHandler(MouseLeaveEvent, value); }
        }

        public event KeyEventHandler KeyDown {
            add { AddHandler(KeyDownEvent, value); }
            remove { RemoveHandler(KeyDownEvent, value); }
        }

        public event KeyEventHandler KeyUp {
            add { AddHandler(KeyUpEvent, value); }
            remove { RemoveHandler(KeyUpEvent, value); }
        }

        public event KeyboardFocusChangedEventHandler LostKeyboardFocus {
            add { AddHandler(LostKeyboardFocusEvent, value); }
            remove { RemoveHandler(LostKeyboardFocusEvent, value); }
        }

        public event KeyboardFocusChangedEventHandler GotKeyboardFocus {
            add { AddHandler(GotKeyboardFocusEvent, value); }
            remove { RemoveHandler(GotKeyboardFocusEvent, value); }
        }

//        public void SetFocus() {
//            ConsoleApplication.Instance.FocusManager.SetFocus(this);
//        }

        /// <summary>
        /// Обладает ли на данный момент текущий контрол фокусом (т.е. принимает клавиатурный ввод)
        /// </summary>
        public bool HasFocus {
            get {
                return ConsoleApplication.Instance.FocusManager.FocusedElement == this;
            }
        }

        public void AddHandler(RoutedEvent routedEvent, Delegate @delegate) {
            EventManager.AddHandler(this, routedEvent, @delegate);
        }

        public void AddHandler(RoutedEvent routedEvent, Delegate @delegate, bool handledEventsToo) {
            EventManager.AddHandler(this, routedEvent, @delegate, handledEventsToo);
        }

        public void RaiseEvent(RoutedEvent routedEvent, RoutedEventArgs args) {
            ConsoleApplication.Instance.EventManager.QueueEvent(routedEvent, args);
        }

        public void RemoveHandler(RoutedEvent routedEvent, Delegate @delegate) {
            EventManager.RemoveHandler(this, routedEvent, @delegate);
        }

        public Control FindChildByName(string name) {
            return Children.FirstOrDefault(control => control.Name == name);
        }

        internal LayoutInfo layoutInfo = new LayoutInfo();
        internal LayoutInfo lastLayoutInfo = new LayoutInfo();

        public Visibility Visibility {
            get;
            set;
        }

        /// <summary>
        /// Just for debug.
        /// </summary>
        public Size? MeasureArgument {
            get {
                return layoutInfo.validity != LayoutValidity.Nothing ? (Size?)layoutInfo.measureArgument : null;
            }
        }
        
        public string Name {
            get;
            set;
        }

        internal readonly List<Control> Children = new List<Control>();

        public Control Parent {
            get;
            protected set;
        }

        protected void AddChild(Control child) {
            if (null == child)
                throw new ArgumentNullException("child");
            if (null != child.Parent)
                throw new ArgumentException("Specified child already has parent.");
            Children.Add(child);
            child.Parent = this;
            ConsoleApplication.Instance.FocusManager.AfterAddElementToTree(child);
            Invalidate();
        }

        protected void RemoveChild(Control child) {
            if (null == child)
                throw new ArgumentNullException("child");
            if (child.Parent != this)
                throw new InvalidOperationException("Specified control is not a child.");
            else {
                ConsoleApplication.Instance.FocusManager.BeforeRemoveElementFromTree(child);
                if (!this.Children.Remove(child))
                    throw new InvalidOperationException("Assertion failed.");
                child.Parent = null;
                Invalidate();
            }
        }

        private void initialize() {
            MinWidth = 0;
            Focusable = false;
            IsFocusScope = false;
            Visibility = Visibility.Visible;
            AddHandler(MouseEnterEvent, new MouseEventHandler(Control_MouseEnter));
            AddHandler(MouseLeaveEvent, new MouseEventHandler(Control_MouseLeave));
            AddHandler(GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(Control_GotKeyboardFocus));
            AddHandler(LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(Control_LostKeyboardFocus));
        }

        private void Control_MouseEnter(object sender, MouseEventArgs args) {
            //Debug.WriteLine("MouseEnter on control : " + Name);
        }

        private void Control_MouseLeave(object sender, MouseEventArgs args) {
            //Debug.WriteLine("MouseLeave on control : " + Name);
        }

        private void Control_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs args) {
            Debug.WriteLine(string.Format("GotKeyboardFocusEvent : OldFocus {0} NewFocus {1}",
                args.OldFocus != null ? args.OldFocus.Name : "null",
                args.NewFocus.Name));
            //args.Handled = true;
        }

        private void Control_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs args) {
            //Debug.WriteLine(string.Format("LostKeyboardFocusEvent : OldFocus {0} NewFocus {1}",
            //    args.OldFocus != null ? args.OldFocus.Name : "null",
            //    args.NewFocus.Name));
            //args.Handled = true;
        }

        public Control() {
            initialize();
        }
        
        public Control(Control parent) {
            initialize();
            //
            Parent = parent;
        }

        /// <summary>
        /// Смещение виртуального холста контрола отн-но холста родительского элемента управления.
        /// Если контрол целиком размещен в родительском элементе управления и не обрезан маргином,
        /// то ActualOffset численно равен RenderSlotRect.Location. Если же часть контрола скрыта, то
        /// ActualOffset отличается от RenderSlotRect.Location.
        /// Учитывает <see cref="Margin"/>, <see cref="HorizontalAlignment"/> и <see cref="VerticalAlignment"/>.
        /// </summary>
        public Vector ActualOffset {
            get {
                return layoutInfo.actualOffset;
            }
            private set {
                layoutInfo.actualOffset = value;
            }
        }

        internal LayoutValidity LayoutValidity {
            get {
                return layoutInfo.validity;
            }
            set {
                // todo : mb refactor this to disallow external access
                layoutInfo.validity = value;
            }
        }

        public int ActualWidth {
            get {
                return RenderSize.Width;
            }
        }

        public int ActualHeight {
            get {
                return RenderSize.Height;
            }
        }

        public int MinWidth {
            get;
            set;
        }

        private int maxWidth = int.MaxValue;
        public int MaxWidth {
            get {
                return maxWidth;
            }
            set {
                maxWidth = value;
            }
        }

        public int MinHeight {
            get;
            set;
        }

        private int maxHeight = int.MaxValue;

        /// <summary>
        /// Shows whether control can handle keyboard input or can't.
        /// </summary>
        public bool Focusable {
            get;
            set;
        }

        /// <summary>
        /// Specifies the order for keyboard focus move.
        /// Takes 0 by default.
        /// </summary>
        public int TabOrder {
            get; 
            set;
        }

        /// <summary>
        /// Показывает, может ли элемент управления выступать в роли FocusScope.
        /// </summary>
        public bool IsFocusScope { get; set; }

        public int MaxHeight {
            get {
                return maxHeight;
            }
            set {
                maxHeight = value;
            }
        }

        public int? Width {
            get;
            set;
        }

        public int? Height {
            get;
            set;
        }

        public Thickness Margin {
            get;
            set;
        }

        public Size DesiredSize {
            get {
                return layoutInfo.desiredSize;
            }
            private set {
                layoutInfo.desiredSize = value;
            }
        }

        private struct MinMax
        {
            /// <summary>
            /// Определяет реальные констрейнты для текущих значений MinHeight/MaxHeight, MinWidth/MaxWidth
            /// и Width/Height. Min-значения не могут быть null, по дефолту равны нулю, также не могут быть int.MaxValue.
            /// Max-значения тоже не могут быть null, по дефолту равны int.MaxValue.
            /// Width и Height могут быть не заданы - в этом случае они контрол будет занимать как можно большее
            /// доступное пространство.
            /// В случае конфликта приоритет имеет Min-property, затем явно заданное значение (Width или Height),
            /// и в последнюю очередь играет роль Max-property.
            /// </summary>
            internal MinMax(int minHeight, int maxHeight, int minWidth, int maxWidth, int? width, int? height) {
                this.maxHeight = maxHeight;
                this.minHeight = minHeight;
                int? l = height;

                int tmp_height = l ?? int.MaxValue;
                this.maxHeight = Math.Max(Math.Min(tmp_height, this.maxHeight), this.minHeight);

                tmp_height = l ?? 0;
                this.minHeight = Math.Max(Math.Min(this.maxHeight, tmp_height), this.minHeight);

                this.maxWidth = maxWidth;
                this.minWidth = minWidth;
                l = width;

                int tmp_width = l ?? int.MaxValue;
                this.maxWidth = Math.Max(Math.Min(tmp_width, this.maxWidth), this.minWidth);

                tmp_width = l ?? 0;
                this.minWidth = Math.Max(Math.Min(this.maxWidth, tmp_width), this.minWidth);
            }

            internal readonly int minWidth;
            internal readonly int maxWidth;
            internal readonly int minHeight;
            internal readonly int maxHeight;
        }

        public void Measure(Size availableSize) {
            if (layoutInfo.validity != LayoutValidity.Nothing)
                // nothing to do
                return;

            layoutInfo.measureArgument = availableSize;

            if (Visibility == Visibility.Collapsed) {
                layoutInfo.unclippedDesiredSize = Size.Empty;
                DesiredSize = Size.Empty;
                return;
            }

            // apply margin
            Thickness margin = Margin;
            int marginWidth = margin.Left + margin.Right;
            int marginHeight = margin.Top + margin.Bottom;

            //  parent size is what parent want us to be
            Size frameworkAvailableSize = new Size(
                Math.Max(availableSize.Width - marginWidth, 0),
                Math.Max(availableSize.Height - marginHeight, 0));

            // apply min/max/currentvalue constraints
            MinMax mm = new MinMax(MinHeight, MaxHeight, MinWidth, MaxWidth, Width, Height);

            frameworkAvailableSize.Width = Math.Max(mm.minWidth, Math.Min(frameworkAvailableSize.Width, mm.maxWidth));
            frameworkAvailableSize.Height = Math.Max(mm.minHeight, Math.Min(frameworkAvailableSize.Height, mm.maxHeight));

            Size desiredSize = MeasureOverride(frameworkAvailableSize);
            if (desiredSize.Width == int.MaxValue || desiredSize.Height == int.MaxValue) {
                throw new InvalidOperationException("MeasureOverride should not return int.MaxValue even for" +
                                                    "availableSize = {int.MaxValue, int.MaxValue} argument.");
            }

            //  maximize desiredSize with user provided min size
            desiredSize = new Size(
                Math.Max(desiredSize.Width, mm.minWidth),
                Math.Max(desiredSize.Height, mm.minHeight));

            //here is the "true minimum" desired size - the one that is
            //for sure enough for the control to render its content.
            Size unclippedDesiredSize = desiredSize;

            // User-specified max size starts to "clip" the control here. 
            //Starting from this point desiredSize could be smaller then actually
            //needed to render the whole control
            if (desiredSize.Width > mm.maxWidth) {
                desiredSize.Width = mm.maxWidth;
            }

            if (desiredSize.Height > mm.maxHeight) {
                desiredSize.Height = mm.maxHeight;
            }

            //  because of negative margins, clipped desired size may be negative.
            //  need to keep it as doubles for that reason and maximize with 0 at the 
            //  very last point - before returning desired size to the parent. 
            int clippedDesiredWidth = desiredSize.Width + marginWidth;
            int clippedDesiredHeight = desiredSize.Height + marginHeight;

            // In overconstrained scenario, parent wins and measured size of the child,
            // including any sizes set or computed, can not be larger then
            // available size. We will clip the guy later. 
            if (clippedDesiredWidth > availableSize.Width) {
                clippedDesiredWidth = availableSize.Width;
            }

            if (clippedDesiredHeight > availableSize.Height) {
                clippedDesiredHeight = availableSize.Height;
            }

            //  Note: unclippedDesiredSize is needed in ArrangeCore,
            //  because due to the layout protocol, arrange should be called 
            //  with constraints greater or equal to child's desired size
            //  returned from MeasureOverride.
            layoutInfo.unclippedDesiredSize = unclippedDesiredSize;

            DesiredSize = new Size(Math.Max(0, clippedDesiredWidth), Math.Max(0, clippedDesiredHeight));
        }
        
        /// <summary>
        /// Возвращает размеры, необходимые для размещения контрола вместе с его дочерними элементами.
        /// <br/>
        /// Если возвращаемый размер меньше availableSize, то это может быть учтено родительским контролом,
        /// и он может выделить под контрол слот, меньший, чем планировалось изначально. А может быть
        /// проигнорировано, в этом случае контрол будет размещён в слоте, большем, чем собственно
        /// контролу необходимо. Контрол должен учитывать такой вариант развития событий (если его
        /// реальное размещение превышает ожидания).
        /// <br/>
        /// Если возвращаемый размер больше availableSize, то опять же тут 2 варианта развития событий.
        /// В первом случае родительский контрол согласно логике размещения может попробовать найти
        /// дополнительное место для контрола и вызвать Measure повторно с бОльшим availableSize.
        /// Либо, если подобной логики в алгоритме размещения родительского контрола не предусмотрено,
        /// или же места нет, - возвращённый desired size будет записан в unclipped desired size и на этапе
        /// arrange контрол будет размещен в желаемых размерах, однако реально столько места он занимать не
        /// будет, и в контексте родительского контрола его рендеринг будет обрезан.
        /// <br/>
        /// Нельзя возвращать int.MaxValue в качестве width или height возвращаемого размера.
        /// <br/>
        /// При реализации панелей необходимо в обязательном порядке вызвать Measure для всех дочерних
        /// элементов, причём количество вызовов может быть любым, но последний для каждого контрола
        /// вызов должен быть выполнен именно с теми размерами, которые реально будут использоваться
        /// при размещении элемента.
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected virtual Size MeasureOverride(Size availableSize) {
            return new Size(0, 0);
        }

        /// <summary>
        /// Размещает элемент управления вместе с дочерними контролами в указанном слоте.
        /// После выполнения метода будут установлены все свойства, необходимые для рендеринга контрола.
        /// Если Arrange вызывался с размерами, меньшими чем те, которые были возвращены контролом в 
        /// MeasureOverride, то рендеринг контрола будет обрезанным.
        /// <br/>
        /// Если Arrange вызывался с размерами, превышающими запрошенные в MeasureOverride, то
        /// слот, выделенный контролу, будет больше ожидаемого, и то, как будет использовано дополнительное
        /// место - зависит от логики ArrangeOverride контрола. Если ArrangeOverride вернет старое значение
        /// (меньшее чем нынешний finalSize), то RenderSize будет меньше RenderSlotRect, и часть пространства,
        /// выделенного для размещения элемента управления, просто не будет им использоваться.
        /// <br/>
        /// Внимание ! Если вы реализуете логику панели в коде метода ArrangeOverride, то очень важно то, что нужно следить за тем,
        /// с какими аргументами вы вызываете метод Arrange для дочерних элементов. Если вы передаёте туда размеры, превышающие те,
        /// которые будут возвращены из ArrangeOverride родительского элемента управления, то это будет означать следующее: родительский
        /// контрол дочернему выделит места больше чем имеется (или ровно всё место), и дочерний контрол при рендеринге будет
        /// полностью затирать родительский элемент управления. За рамки слота родительского контрола он, конечно, не залезет
        /// (обрежется системой отрисовки), но и родительскому контролу не даст ничего нарисовать.
        /// </summary>
        /// <param name="finalRect"></param>
        public void Arrange(Rect finalRect) {
            if (layoutInfo.validity != LayoutValidity.Nothing) {
                return;
            }

            if (Visibility == Visibility.Collapsed) {
                RenderSlotRect = Rect.Empty;
                RenderSize = Size.Empty;
                layoutInfo.layoutClip = calculateLayoutClip();
                layoutInfo.validity = LayoutValidity.MeasureAndArrange;
                return;
            }

            RenderSlotRect = finalRect;

            // If LayoutConstrained==true (parent wins in layout),
            // we might get finalRect.Size smaller then UnclippedDesiredSize. 
            // Stricltly speaking, this may be the case even if LayoutConstrained==false (child wins),
            // since who knows what a particualr parent panel will try to do in error.
            // In this case we will not actually arrange a child at a smaller size,
            // since the logic of the child does not expect to receive smaller size 
            // (if it coudl deal with smaller size, it probably would accept it in MeasureOverride)
            // so lets replace the smaller arreange size with UnclippedDesiredSize 
            // and then clip the guy later. 
            // We will use at least UnclippedDesiredSize to compute arrangeSize of the child, and
            // we will use layoutSlotSize to compute alignments - so the bigger child can be aligned within 
            // smaller slot.

            // Start to compute arrange size for the child. 
            // It starts from layout slot or deisred size if layout slot is smaller then desired, 
            // and then we reduce it by margins, apply Width/Height etc, to arrive at the size
            // that child will get in its ArrangeOverride. 
            Size arrangeSize = finalRect.Size;

            Thickness margin = Margin;
            int marginWidth = margin.Left + margin.Right;
            int marginHeight = margin.Top + margin.Bottom;

            arrangeSize.Width = Math.Max(0, arrangeSize.Width - marginWidth);
            arrangeSize.Height = Math.Max(0, arrangeSize.Height - marginHeight);
            
            // Next, compare against unclipped, transformed size.
            Size unclippedDesiredSize = layoutInfo.unclippedDesiredSize;

            if (arrangeSize.Width < unclippedDesiredSize.Width) {
                arrangeSize.Width = unclippedDesiredSize.Width;
            }

            if (arrangeSize.Height < unclippedDesiredSize.Height) {
                arrangeSize.Height = unclippedDesiredSize.Height;
            }

            // Alignment==Stretch --> arrange at the slot size minus margins
            // Alignment!=Stretch --> arrange at the unclippedDesiredSize 
            if (HorizontalAlignment != HorizontalAlignment.Stretch) {
                arrangeSize.Width = unclippedDesiredSize.Width;
            }

            if (VerticalAlignment != VerticalAlignment.Stretch) {
                arrangeSize.Height = unclippedDesiredSize.Height;
            }

            //Here we use un-clipped InkSize because element does not know that it is
            //clipped by layout system and it should have as much space to render as
            //it returned from its own ArrangeOverride 
            RenderSize = ArrangeOverride(arrangeSize);

            Vector offset = computeAlignmentOffset();

            offset.X += finalRect.X + margin.Left;
            offset.Y += finalRect.Y + margin.Top;

            if (!this.ActualOffset.Equals(offset)) {
                this.ActualOffset = offset;
            }

            layoutInfo.layoutClip = calculateLayoutClip();

            layoutInfo.validity = LayoutValidity.MeasureAndArrange;
        }
        
        public HorizontalAlignment HorizontalAlignment {
            get;
            set;
        }

        public VerticalAlignment VerticalAlignment {
            get;
            set;
        }

        /// <summary>
        /// Размер, под который контрол будет рендерить свое содержимое.
        /// Может быть больше RenderSlotRect из-за случаев, когда контрол не влезает в рамки,
        /// отведенные методом Arrange. Контрол будет обрезан лайаут-системой в соответствии с RenderSlotRect.
        /// </summary>
        public Size RenderSize {
            get {
                return layoutInfo.renderSize;
            }
            private set {
                layoutInfo.renderSize = value;
            }
        }

        /// <summary>
        /// Отведенный родительским элементом управления слот для отрисовки.
        /// Задается аргументом при вызове <see cref="Arrange"/>.
        /// </summary>
        public Rect RenderSlotRect {
            get {
                return layoutInfo.renderSlotRect;
            }
            private set {
                layoutInfo.renderSlotRect = value;
            }
        }

        private Rect calculateLayoutClip() {
            Vector offset = computeAlignmentOffset();
            Size clientSize = getClientSize();
            return new Rect(-offset.X, -offset.Y, clientSize.Width, clientSize.Height);
        }

        /// <summary>
        /// Прямоугольник внутри виртуального холста контрола, в которое будет выведена графика.
        /// Все остальное будет обрезано в соответствии с установленными значениями свойств
        /// <see cref="Margin"/>, <see cref="HorizontalAlignment"/> и <see cref="VerticalAlignment"/>.
        /// </summary>
        public Rect LayoutClip {
            get {
                return layoutInfo.layoutClip;
            }
        }

        private Vector computeAlignmentOffset() {
            //
            MinMax mm = new MinMax(MinHeight, MaxHeight, MinWidth, MaxWidth, Width, Height);

            Size renderSize = RenderSize;

            //clippedInkSize differs from InkSize only what MaxWidth/Height explicitly clip the
            //otherwise good arrangement. For ex, DS<clientSize but DS>MaxWidth - in this
            //case we should initiate clip at MaxWidth and only show Top-Left portion 
            //of the element limited by Max properties. It is Top-left because in case when we
            //are clipped by container we also degrade to Top-Left, so we are consistent. 
            Size clippedInkSize = new Size(Math.Min(renderSize.Width, mm.maxWidth),
                                           Math.Min(renderSize.Height, mm.maxHeight));
            Size clientSize = getClientSize();

            return computeAlignmentOffsetCore(clientSize, clippedInkSize);
        }

        // The client size is the size of layout slot decreased by margins. 
        // This is the "window" through which we see the content of the child.
        // Alignments position ink of the child in this "window".
        // Max with 0 is neccessary because layout slot may be smaller then unclipped desired size.
        private Size getClientSize() {
            Thickness margin = Margin;
            int marginWidth = margin.Left + margin.Right;
            int marginHeight = margin.Top + margin.Bottom;

            Rect renderSlotRect = RenderSlotRect;

            return new Size(Math.Max(0, renderSlotRect.Width - marginWidth),
                            Math.Max(0, renderSlotRect.Height - marginHeight));
        }

        private Vector computeAlignmentOffsetCore(Size clientSize, Size inkSize) {
            Vector offset = new Vector();

            HorizontalAlignment ha = HorizontalAlignment;
            VerticalAlignment va = VerticalAlignment;

            //this is to degenerate Stretch to Top-Left in case when clipping is about to occur
            //if we need it to be Center instead, simply remove these 2 ifs
            if (ha == HorizontalAlignment.Stretch
                && inkSize.Width > clientSize.Width) {
                ha = HorizontalAlignment.Left;
            }

            if (va == VerticalAlignment.Stretch
                && inkSize.Height > clientSize.Height) {
                va = VerticalAlignment.Top;
            }
            //end of degeneration of Stretch to Top-Left 

            if (ha == HorizontalAlignment.Center
                || ha == HorizontalAlignment.Stretch) {
                offset.X = (clientSize.Width - inkSize.Width)/2;
            } else if (ha == HorizontalAlignment.Right) {
                offset.X = clientSize.Width - inkSize.Width;
            } else {
                offset.X = 0;
            }

            if (va == VerticalAlignment.Center
                || va == VerticalAlignment.Stretch) {
                offset.Y = (clientSize.Height - inkSize.Height)/2;
            } else if (va == VerticalAlignment.Bottom) {
                offset.Y = clientSize.Height - inkSize.Height;
            } else {
                offset.Y = 0;
            }

            return offset;
        }

        /// <summary>
        /// Реализация <see cref="ArrangeOverride"/> по умолчанию возвращает исходный finalSize.
        /// Поведение по умолчанию подходит для всех конечных контролов (так как им не нужно
        /// размещать дочерние элементы управления и их размеры зависят только от их собственного
        /// содержимого). Однако для панелей правильная реализация этого метода необходима для
        /// слаженной и корректной работы системы размещения.
        /// 
        /// В этом методе дочерний контрол должен разместить все дочерние элементы управления,
        /// вызвав для для каждого из них метод <see cref="Arrange"/>, и вернуть размеры,
        /// которые сам контрол с детьми реально занял по итогам операции размещения. Если контрол занял
        /// меньше места, а вернул больше (например, просто сделал return finalSize, хотя места занял
        /// меньше), то у контрола останется пустое свободное пространство, и надо будет
        /// его заполнить графикой. Соответственно, при Alignment = Stretch контрол будет занимать всю
        /// доступную ему площадь. Если же возвращать реальные размеры, то при Alignment = Stretch и
        /// избыточном месте контрол будет выровнен по центру автоматически (по центру RenderSlot'а,
        /// предоставленного родительским элементом управления). Если дочерний элемент не должен
        /// быть показан, нужно вызвать для него <see cref="Arrange"/> с пустым прямоугольником в качестве аргумента,
        /// в противном случае старое значение останется, и скорее всего будет мусор в буфере контрола.
        /// 
        /// Значение, возвращённое этим методом, устанавливается в качестве <see cref="RenderSize"/>
        /// контрола, и <see cref="ActualWidth"/> и <see cref="ActualHeight"/> после этого
        /// возвращают именно его.
        /// <param name="finalSize">The final area within the parent that this element
        /// should use to arrange itself and its children.</param>
        /// <returns>The actual size used.</returns>
        /// </summary>
        protected virtual Size ArrangeOverride(Size finalSize) {
            return finalSize;
        }
        
        internal void ResetValidity() {
            // copy all calculated layout info into lastLayoutInfo
            if (layoutInfo.validity == LayoutValidity.Render) {
                lastLayoutInfo.CopyValuesFrom(layoutInfo);
            }
            // clear layoutInfo.validity (and whole layoutInfo structure to avoid garbage data)
            layoutInfo.ClearValues();
            // recursively invalidate children, but without add them to queue
            foreach (Control child in Children) {
                child.ResetValidity();
            }
        }
        
        /// <summary>
        /// Добавляет этот контрол в очередь для обновления. При следующем выполнении цикла
        /// обновления UI система размещения вызовет Measure, Arrange и Render. Дочерние контролы,
        /// чьи размеры не изменились, перерисованы не будут. То же касается родительских контролов,
        /// если размеры самого контрола не изменились.
        /// </summary>
        public void Invalidate() {
            ConsoleApplication.Instance.Renderer.AddControlToInvalidationQueue(this);
        }

        public virtual Control GetTopChildAtPoint(Point point) {
            return (from child in Children
                    where child.RenderSlotRect.Contains(point)
                    select child).FirstOrDefault();
        }

        /// <summary>
        /// Переводит точку point из системы координат source в систему координат dest.
        /// В качестве source и dest можно указывать null, в этом случае за систему координат будет
        /// взята система координат экрана консоли.
        /// </summary>
        /// <param name="source">Контрол, относительно которого задан point или null если координата глобальная.</param>
        /// <param name="point">Координаты точки относительно source.</param>
        /// <param name="dest">Контрол, относительно которого необходимо вычислить координаты точки.</param>
        /// <returns></returns>
        public static Point TranslatePoint(Control source, Point point, Control dest) {
            if (source == null || dest == null) {
                if (source == null && dest != null) {
                    // translating raw point (absolute coords) into relative to dest control point
                    Control currentControl = dest;
                    for (;;) {
                        Vector actualOffset = currentControl.ActualOffset;
                        point.Offset(-actualOffset.X, -actualOffset.y);
                        if (currentControl.Parent == null) {
                            break;
                        }
                        currentControl = currentControl.Parent;
                    }
                    return point;
                } else if (source != null && dest == null) {
                    // translating point relative to source into absolute coords
                    Control currentControl = source;
                    for (;;) {
                        Vector actualOffset = currentControl.ActualOffset;
                        point.Offset(actualOffset.X, actualOffset.y);
                        if (currentControl.Parent == null)
                            break;
                        currentControl = currentControl.Parent;
                    }
                    return point;
                } else {
                    // both source and dest are null - we shouldn't to do anything
                    return point;
                }
            } else {
                // find common ancestor
                Control ancestor = FindCommonAncestor(source, dest);
                // traverse back from source to common ancestor
                Control currentControl = source;
                while (currentControl != ancestor) {
                    Vector actualOffset = currentControl.ActualOffset;
                    point.Offset(actualOffset.X, actualOffset.y);
                    currentControl = currentControl.Parent;
                }
                // traverse back from dest to common ancestor
                currentControl = dest;
                while (currentControl != ancestor) {
                    Vector actualOffset = currentControl.ActualOffset;
                    point.Offset(-actualOffset.X, -actualOffset.y);
                    currentControl = currentControl.Parent;
                }
                return point;
            }
        }

        /// <summary>
        /// Returns common ancestor for specified controls pair.
        /// If there are no common ancestor found, null will be returned.
        /// But this situation is impossible because there are only one main control in application.
        /// </summary>
        public static Control FindCommonAncestor(Control a, Control b) {
            if (null == a)
                throw new ArgumentNullException("a");
            if (null == b)
                throw new ArgumentNullException("b");
            //
            List<Control> visited = new List<Control>();
            Control refA = a;
            Control refB = b;
            bool f = true;
            for (;;) {
                if (refA == refB)
                    return refA;
                if (visited.Contains(refB))
                    return refB;
                if (visited.Contains(refA))
                    return refA;
                if (refA.Parent == null && refB.Parent == null)
                    return null;
                if (f) {
                    if (refA.Parent != null) {
                        visited.Add(refA);
                        refA = refA.Parent;
                    }
                } else {
                    if (refB.Parent != null) {
                        visited.Add(refB);
                        refB = refB.Parent;
                    }
                }
                f = !f;
            }
        }

        public override string ToString() {
            return string.Format("Control: {0}", Name);
        }

        /// <summary>
        /// Performs hit testing to a visible part of control.
        /// </summary>
        /// <param name="rawPoint"></param>
        /// <returns>True if point is on control, otherwise false.</returns>
        public bool HitTest(Point rawPoint) {
            Point point = TranslatePoint(null, rawPoint, Parent);
            // hit testing - calculate position in child according to specified layout attributes
            Vector actualOffset = ActualOffset;
            Rect renderSlotRect = RenderSlotRect;
            Rect virtualSlotRect = new Rect(new Point(actualOffset.x, actualOffset.y), RenderSize);
            if (!LayoutClip.IsEmpty) {
                Rect layoutClip = LayoutClip;
                Point location = layoutClip.Location;
                location.Offset(actualOffset.x, actualOffset.y);
                layoutClip.Location = location;
                virtualSlotRect.Intersect(layoutClip);
            }
            virtualSlotRect.Intersect(renderSlotRect);
            return virtualSlotRect.Contains(point);
        }

        /// <summary>
        /// Performs hit testing to a visible part of child control.
        /// Static version of method.
        /// </summary>
        /// <param name="rawPoint"></param>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        /// <returns>True if point is on child, false otherwise.</returns>
        public static bool HitTest(Point rawPoint, Control parent, Control child) {
            if (null == parent)
                throw new ArgumentNullException("parent");
            if (null == child)
                throw new ArgumentNullException("child");
            //
            Point point = TranslatePoint(null, rawPoint, parent);
            // hit testing - calculate position in child according to specified layout attributes
            Vector actualOffset = child.ActualOffset;
            Rect renderSlotRect = child.RenderSlotRect;
            Rect virtualSlotRect = new Rect(new Point(actualOffset.x, actualOffset.y), child.RenderSize);
            if (!child.LayoutClip.IsEmpty) {
                Rect layoutClip = child.LayoutClip;
                Point location = layoutClip.Location;
                location.Offset(actualOffset.x, actualOffset.y);
                layoutClip.Location = location;
                virtualSlotRect.Intersect(layoutClip);
            }
            virtualSlotRect.Intersect(renderSlotRect);
            return virtualSlotRect.Contains(point);
        }

        /// <summary>
        /// Проверяет, не перекрывается ли точка point контрола другими контролами.
        /// </summary>
        public bool IsPointVisible(Point point) {
            return IsPointVisible(this, point);
        }

        /// <summary>
        /// Проверяет, не перекрывается ли точка point контрола control другими контролами.
        /// </summary>
        internal static bool IsPointVisible(Control control, Point point) {
            if (null == control)
                throw new ArgumentNullException("control");
            //
            Rect layoutClip = control.LayoutClip;
            bool visible;
            if (layoutClip.IsEmpty) {
                Rect controlVirtualCanvasRect = new Rect(new Point(0, 0), control.RenderSize);
                visible = controlVirtualCanvasRect.Contains(point);
            } else {
                visible = layoutClip.Contains(point);
            }
            //
            if (!visible)
                return false;
            //
            if (null == control.Parent)
                return true;
            // recursively check the parent
            return IsPointVisible(control.Parent, TranslatePoint(control, point, control.Parent));
        }

        /// <summary>
        /// You should define your rendering logic here.
        /// </summary>
        /// <param name="buffer">Buffer where rendered content will be stored.</param>
        public virtual void Render(RenderingBuffer buffer) {
        }

        internal virtual List<Control> GetChildrenOrderedByZIndex() {
            return Children;
        }

        /// <summary>
        /// Sets the position of console cursor.
        /// </summary>
        /// <param name="point">Coords relatively to this control.</param>
        protected void SetCursorPosition(Point point) {
            ConsoleApplication.Instance.SetCursorPosition(TranslatePoint(this, point, null));
        }

        protected static void HideCursor() {
            ConsoleApplication.Instance.HideCursor();
        }

        protected static void ShowCursor() {
            ConsoleApplication.Instance.ShowCursor();
        }

        private bool cursorVisible = false;
        internal bool CursorVisible {
            get {
                return cursorVisible;
            }
            set {
                if (cursorVisible != value) {
                    cursorVisible = value;
                    if (HasFocus) {
                        ConsoleApplication.Instance.FocusManager.RefreshMouseCursor();
                    }
                }
            }
        }

        private Point cursorPosition = new Point(0, 0);
        internal Point CursorPosition {
            get {
                return cursorPosition;
            }
            set {
                if (cursorPosition != value) {
                    cursorPosition = value;
                    if (HasFocus) {
                        ConsoleApplication.Instance.FocusManager.RefreshMouseCursor();
                    }
                }
            }
        }

        /// <summary>
        /// Записывает строку в буфер, обрезая её при необходимости (ставя в конце две точки).
        /// </summary>
        /// <param name="s">Строка</param>
        /// <param name="buffer">Буфер вывода</param>
        /// <param name="x">X-координата, с которой начинать вывод строки</param>
        /// <param name="y">Y-координата</param>
        /// <param name="maxWidth">Доступная для вывода ширина (начиная с x). То есть x + maxWidth не должен
        /// превышать ActualWidth контрола</param>
        /// <param name="attr">Атрибуты</param>
        /// <returns>Количество реально выведенных в буфер пикселей, min(s.len, maxWidth)</returns>
        protected static int RenderString(string s,
            RenderingBuffer buffer,
            int x,
            int y,
            int maxWidth,
            Attr attr)
        {
            for (int i = 0; i < Math.Min(s.Length, maxWidth); i++)
            {
                char c;
                if (i + 2 < maxWidth || i < 2 || s.Length <= maxWidth)
                {
                    c = s[i];
                }
                else
                {
                    c = '.';
                }
                buffer.SetPixel(x + i, y, c, attr);
            }
            return Math.Min(s.Length, maxWidth);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged( string propertyName ) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if ( handler != null ) handler( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}
