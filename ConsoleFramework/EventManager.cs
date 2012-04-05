using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;
using ConsoleFramework.Native;

namespace ConsoleFramework {

    public delegate void RoutedEventHandler(object sender, RoutedEventArgs e);

    public class RoutedEventArgs : EventArgs {
        private bool handled;
        private readonly object source;
        private readonly RoutedEvent routedEvent;

        public bool Handled {
            get {
                return handled;
            }
            set {
                handled = value;
            }
        }

        public object Source {
            get {
                return source;
            }
        }

        public RoutedEvent RoutedEvent {
            get {
                return routedEvent;
            }
        }

        public RoutedEventArgs (object source, RoutedEvent routedEvent) {
            this.source = source;
            this.routedEvent = routedEvent;
        }
    }

    /// <summary>
    /// Central point of events management routine.
    /// Provides events routing.
    /// </summary>
    public sealed class EventManager {
        private readonly Stack<Control> inputCaptureStack = new Stack<Control>();

        private class DelegateInfo {
            public readonly Delegate @delegate;
            public bool handledEventsToo;

            public DelegateInfo(Delegate @delegate) {
                this.@delegate = @delegate;
                this.handledEventsToo = false;
            }

            public DelegateInfo(Delegate @delegate, bool handledEventsToo) {
                this.@delegate = @delegate;
                this.handledEventsToo = handledEventsToo;
            }
        }

        private class RoutedEventTargetInfo {
            public readonly object target;
            public List<DelegateInfo> handlersList;

            public RoutedEventTargetInfo(object target) {
                if (null == target)
                    throw new ArgumentNullException("target");
                this.target = target;
            }
        }

        private class RoutedEventInfo {
            public RoutedEvent routedEvent;
            public List<RoutedEventTargetInfo> targetsList;

            public RoutedEventInfo(RoutedEvent routedEvent) {
                if (null == routedEvent)
                    throw new ArgumentNullException("routedEvent");
                this.routedEvent = routedEvent;
            }
        }

        private static readonly Dictionary<RoutedEventKey, RoutedEventInfo> routedEvents = new Dictionary<RoutedEventKey, RoutedEventInfo>();

        public static RoutedEvent RegisterRoutedEvent(string name, RoutingStrategy routingStrategy, Type handlerType, Type ownerType) {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name");
            if (null == handlerType)
                throw new ArgumentNullException("handlerType");
            if (null == ownerType)
                throw new ArgumentNullException("ownerType");
            //
            RoutedEventKey key = new RoutedEventKey(name, ownerType);
            if (routedEvents.ContainsKey(key)) {
                throw new InvalidOperationException("This routed event is already registered.");
            }
            RoutedEvent routedEvent = new RoutedEvent(handlerType, name, ownerType, routingStrategy);
            RoutedEventInfo routedEventInfo = new RoutedEventInfo(routedEvent);
            routedEvents.Add(key, routedEventInfo);
            return routedEvent;
        }

        public static void AddHandler(object target, RoutedEvent routedEvent, Delegate handler, bool handledEventsToo) {
            if (null == target)
                throw new ArgumentNullException("target");
            if (null == routedEvent)
                throw new ArgumentNullException("routedEvent");
            if (null == handler)
                throw new ArgumentNullException("handler");
            //
            RoutedEventKey key = routedEvent.Key;
            if (!routedEvents.ContainsKey(key))
                throw new ArgumentException("Specified routed event is not registered.", "routedEvent");
            RoutedEventInfo routedEventInfo = routedEvents[key];
            bool needAddTarget = true;
            if (routedEventInfo.targetsList != null) {
                RoutedEventTargetInfo targetInfo = routedEventInfo.targetsList.FirstOrDefault(info => info.target == target);
                if (null != targetInfo) {
                    if (targetInfo.handlersList == null)
                        targetInfo.handlersList = new List<DelegateInfo>();
                    targetInfo.handlersList.Add(new DelegateInfo(handler, handledEventsToo));
                    needAddTarget = false;
                }
            }
            if (needAddTarget) {
                RoutedEventTargetInfo targetInfo = new RoutedEventTargetInfo(target);
                targetInfo.handlersList = new List<DelegateInfo>();
                targetInfo.handlersList.Add(new DelegateInfo(handler, handledEventsToo));
                if (routedEventInfo.targetsList == null)
                    routedEventInfo.targetsList = new List<RoutedEventTargetInfo>();
                routedEventInfo.targetsList.Add(targetInfo);
            }
        }

        public static void RemoveHandler(object target, RoutedEvent routedEvent, Delegate handler) {
            if (null == target)
                throw new ArgumentNullException("target");
            if (null == routedEvent)
                throw new ArgumentNullException("routedEvent");
            if (null == handler)
                throw new ArgumentNullException("handler");
            //
            RoutedEventKey key = routedEvent.Key;
            if (!routedEvents.ContainsKey(key))
                throw new ArgumentException("Specified routed event is not registered.", "routedEvent");
            RoutedEventInfo routedEventInfo = routedEvents[key];
            if (routedEventInfo.targetsList == null)
                throw new InvalidOperationException("Targets list is empty.");
            RoutedEventTargetInfo targetInfo = routedEventInfo.targetsList.FirstOrDefault(info => info.target == target);
            if (null == targetInfo)
                throw new ArgumentException("Target not found in targets list of specified routed event.", "target");
            if (null == targetInfo.handlersList)
                throw new InvalidOperationException("Handlers list is empty.");
            int findIndex = targetInfo.handlersList.FindIndex(info => info.@delegate == handler);
            if (-1 == findIndex)
                throw new ArgumentException("Specified handler not found.", "handler");
            targetInfo.handlersList.RemoveAt(findIndex);
        }

        /// <summary>
        /// Возвращает список таргетов, подписанных на указанное RoutedEvent.
        /// </summary>
        private static List<RoutedEventTargetInfo> getTargetsSubscribedTo(RoutedEvent routedEvent) {
            if (null == routedEvent)
                throw new ArgumentNullException("routedEvent");
            RoutedEventKey key = routedEvent.Key;
            if (!routedEvents.ContainsKey(key))
                throw new ArgumentException("Specified routed event is not registered.", "routedEvent");
            RoutedEventInfo routedEventInfo = routedEvents[key];
            return routedEventInfo.targetsList;
        }

        public void BeginCaptureInput(Control control) {
            if (null == control) {
                throw new ArgumentNullException("control");
            }
            //
            inputCaptureStack.Push(control);
        }

        public void EndCaptureInput(Control control) {
            if (null == control) {
                throw new ArgumentNullException("control");
            }
            //
            if (inputCaptureStack.Peek() != control) {
                throw new InvalidOperationException(
                    "Last control captured the input differs from specified in argument.");
            }
            inputCaptureStack.Pop();
        }

        public void ProcessEvent(INPUT_RECORD inputRecord, Control rootElement, Rect rootElementRect) {
            //ban all input except mouse clicking
            //if (inputRecord.EventType != EventType.MOUSE_EVENT || (inputRecord.MouseEvent.dwEventFlags == MouseEventFlags.MOUSE_MOVED)) {
            //    return;
            //}
            if (inputCaptureStack.Count != 0) {
                Control capturingControl = inputCaptureStack.Peek();
                capturingControl.HandleEvent(translateInputRecord(inputRecord, capturingControl));
            } else {
                doProcessEvent(inputRecord, rootElement);
            }
        }

        private bool doProcessEvent(INPUT_RECORD inputRecord, Control control) {
            bool handled = false;
            if (control.children.Count != 0) {
                List<Control> childrenOrderedByZIndex = control.GetChildrenOrderedByZIndex();
                for (int i = childrenOrderedByZIndex.Count - 1; i >= 0; i--) {
                    Control child = childrenOrderedByZIndex[i];
                    INPUT_RECORD translatedToParent = translateInputRecord(inputRecord, control);
                    Point point = new Point(translatedToParent.MouseEvent.dwMousePosition.X, translatedToParent.MouseEvent.dwMousePosition.Y);
                    // if we found child responsible to handle this event
                    if (inputRecord.EventType != EventType.MOUSE_EVENT || child.RenderSlotRect.Contains(point)) {
                        //
                        handled = doProcessEvent(inputRecord, child);
                        break;
                        //
                    }
                }
            }
            if (!handled || control.AcceptHandledEvents) {
                handled = control.HandleEvent(translateInputRecord(inputRecord, control));
            }
            return handled;
        }

        /// <summary>
        /// Translates coordinate information to dest-relative coord system and returns modified struct.
        /// </summary>
        private INPUT_RECORD translateInputRecord(INPUT_RECORD inputRecord, Control dest) {
            if (inputRecord.EventType != EventType.MOUSE_EVENT) {
                return inputRecord;
            }
            COORD mousePosition = inputRecord.MouseEvent.dwMousePosition;
            Point translatedPoint = Control.TranslatePoint(null, new Point(mousePosition.X, mousePosition.Y), dest);
            inputRecord.MouseEvent.dwMousePosition = new COORD((short)translatedPoint.x, (short)translatedPoint.y);
            return inputRecord;
        }
    }
}