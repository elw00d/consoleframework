using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;
using ConsoleFramework.Native;

namespace ConsoleFramework.Events {

    /// <summary>
    /// Central point of events management routine.
    /// Provides events routing.
    /// </summary>
    public sealed class EventManager {
        private readonly Stack<Control> inputCaptureStack = new Stack<Control>();

        private class DelegateInfo {
            public readonly Delegate @delegate;
            public readonly bool handledEventsToo;

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
            public List<RoutedEventTargetInfo> targetsList;

            public RoutedEventInfo(RoutedEvent routedEvent) {
                if (null == routedEvent)
                    throw new ArgumentNullException("routedEvent");
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

        public static void AddHandler(object target, RoutedEvent routedEvent, Delegate handler) {
            AddHandler(target, routedEvent, handler, false);
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

        private readonly Queue<RoutedEventArgs> eventsQueue = new Queue<RoutedEventArgs>();

        private MouseButtonState getLeftButtonState(MOUSE_BUTTON_STATE rawState) {
            return (rawState & MOUSE_BUTTON_STATE.FROM_LEFT_1ST_BUTTON_PRESSED) ==
                   MOUSE_BUTTON_STATE.FROM_LEFT_1ST_BUTTON_PRESSED
                       ? MouseButtonState.Pressed
                       : MouseButtonState.Released;
        }

        private MouseButtonState getMiddleButtonState(MOUSE_BUTTON_STATE rawState) {
            return (rawState & MOUSE_BUTTON_STATE.FROM_LEFT_2ND_BUTTON_PRESSED) ==
                   MOUSE_BUTTON_STATE.FROM_LEFT_2ND_BUTTON_PRESSED
                       ? MouseButtonState.Pressed
                       : MouseButtonState.Released;
        }

        private MouseButtonState getRightButtonState(MOUSE_BUTTON_STATE rawState) {
            return (rawState & MOUSE_BUTTON_STATE.RIGHTMOST_BUTTON_PRESSED) ==
                   MOUSE_BUTTON_STATE.RIGHTMOST_BUTTON_PRESSED
                       ? MouseButtonState.Pressed
                       : MouseButtonState.Released;
        }

        private MouseButtonState lastLeftMouseButtonState = MouseButtonState.Released;
        private MouseButtonState lastMiddleMouseButtonState = MouseButtonState.Released;
        private MouseButtonState lastRightMouseButtonState = MouseButtonState.Released;

        private readonly List<Control> prevMouseOverStack = new List<Control>();

        private Point lastMousePosition;

        // Auto-repeating mouse left click when holding pressed button
        private bool autoRepeatTimerRunning = false;
        private Timer timer;
        private MouseButtonEventArgs lastMousePressEventArgs;

        private void startAutoRepeatTimer( MouseButtonEventArgs eventArgs ) {
            lastMousePressEventArgs = eventArgs;
            timer = new Timer( state => {
                ConsoleApplication.Instance.RunOnUiThread( ( ) => {
                    if (autoRepeatTimerRunning) {
                        eventsQueue.Enqueue( new MouseButtonEventArgs(
                            lastMousePressEventArgs.Source,
                            Control.MouseDownEvent,
                            lastMousePosition,
                            lastMousePressEventArgs.LeftButton,
                            lastMousePressEventArgs.MiddleButton,
                            lastMousePressEventArgs.RightButton,
                            MouseButton.Left,
                            1,
                            true
                        ) );
                    }
                } );
                // todo : make this constants configurable
            }, null, TimeSpan.FromMilliseconds( 300 ), TimeSpan.FromMilliseconds(100) );
            autoRepeatTimerRunning = true;
        }

        private void stopAutoRepeatTimer( ) {
            timer.Dispose( );
            timer = null;
            autoRepeatTimerRunning = false;
            lastMousePressEventArgs = null;
        }

        public void ParseInputEvent(INPUT_RECORD inputRecord, Control rootElement) {
            if (inputRecord.EventType == EventType.MOUSE_EVENT) {
                MOUSE_EVENT_RECORD mouseEvent = inputRecord.MouseEvent;

                if (mouseEvent.dwEventFlags != MouseEventFlags.PRESSED_OR_RELEASED &&
                    mouseEvent.dwEventFlags != MouseEventFlags.MOUSE_MOVED &&
                    mouseEvent.dwEventFlags != MouseEventFlags.DOUBLE_CLICK &&
                    mouseEvent.dwEventFlags != MouseEventFlags.MOUSE_WHEELED &&
                    mouseEvent.dwEventFlags != MouseEventFlags.MOUSE_HWHEELED) {
                    //
                    throw new InvalidOperationException("Flags combination in mouse event was not expected.");
                }
                Point rawPosition;
                if (mouseEvent.dwEventFlags == MouseEventFlags.MOUSE_MOVED ||
                    mouseEvent.dwEventFlags == MouseEventFlags.PRESSED_OR_RELEASED) {
                    rawPosition = new Point(mouseEvent.dwMousePosition.X, mouseEvent.dwMousePosition.Y);
                    lastMousePosition = rawPosition;
                } else {
                    // При событии MOUSE_WHEELED в Windows некорректно устанавливается mouseEvent.dwMousePosition
                    // Поэтому для определения элемента, над которым производится прокручивание колёсика, мы
                    // вынуждены сохранять координаты, полученные при предыдущем событии мыши
                    rawPosition = lastMousePosition;
                }

                Control topMost = VisualTreeHelper.FindTopControlUnderMouse(rootElement,
                    Control.TranslatePoint(null, rawPosition, rootElement));

                // если мышь захвачена контролом, то события перемещения мыши доставляются только ему,
                // события, связанные с нажатием мыши - тоже доставляются только ему, вместо того
                // контрола, над которым событие было зарегистрировано. Такой механизм необходим,
                // например, для корректной обработки перемещений окон (вверх или в стороны)
                Control source = (inputCaptureStack.Count != 0) ? inputCaptureStack.Peek() : topMost;

                // No sense to further process event with no source control
                if ( source == null ) return;
                
                if (mouseEvent.dwEventFlags == MouseEventFlags.MOUSE_MOVED) {
                    MouseButtonState leftMouseButtonState = getLeftButtonState(mouseEvent.dwButtonState);
                    MouseButtonState middleMouseButtonState = getMiddleButtonState(mouseEvent.dwButtonState);
                    MouseButtonState rightMouseButtonState = getRightButtonState(mouseEvent.dwButtonState);
                    //
                    MouseEventArgs mouseEventArgs = new MouseEventArgs(source, Control.PreviewMouseMoveEvent,
                                                                       rawPosition,
                                                                       leftMouseButtonState,
                                                                       middleMouseButtonState,
                                                                       rightMouseButtonState
                        );
                    eventsQueue.Enqueue(mouseEventArgs);
                    //
                    lastLeftMouseButtonState = leftMouseButtonState;
                    lastMiddleMouseButtonState = middleMouseButtonState;
                    lastRightMouseButtonState = rightMouseButtonState;

                    // detect mouse enter / mouse leave events

                    // path to source from root element down
                    List<Control> mouseOverStack = new List<Control>();
                    Control current = topMost;
                    while (null != current) {
                        mouseOverStack.Insert(0, current);
                        current = current.Parent;
                    }

                    int index;
                    for (index = 0; index < Math.Min(mouseOverStack.Count, prevMouseOverStack.Count); index++) {
                        if (mouseOverStack[index] != prevMouseOverStack[index])
                            break;
                    }

                    for (int i = prevMouseOverStack.Count - 1; i >= index; i-- ) {
                        Control control = prevMouseOverStack[i];
                        MouseEventArgs args = new MouseEventArgs(control, Control.MouseLeaveEvent,
                                                                    rawPosition,
                                                                    leftMouseButtonState,
                                                                    middleMouseButtonState,
                                                                    rightMouseButtonState
                            );
                        eventsQueue.Enqueue(args);
                    }

                    for (int i = index; i < mouseOverStack.Count; i++ ) {
                        // enqueue MouseEnter event
                        Control control = mouseOverStack[i];
                        MouseEventArgs args = new MouseEventArgs(control, Control.MouseEnterEvent,
                                                                    rawPosition,
                                                                    leftMouseButtonState,
                                                                    middleMouseButtonState,
                                                                    rightMouseButtonState
                            );
                        eventsQueue.Enqueue(args);
                    }

                    prevMouseOverStack.Clear();
                    prevMouseOverStack.AddRange(mouseOverStack);
                }
                if (mouseEvent.dwEventFlags == MouseEventFlags.PRESSED_OR_RELEASED) {
                    //
                    MouseButtonState leftMouseButtonState = getLeftButtonState(mouseEvent.dwButtonState);
                    MouseButtonState middleMouseButtonState = getMiddleButtonState(mouseEvent.dwButtonState);
                    MouseButtonState rightMouseButtonState = getRightButtonState(mouseEvent.dwButtonState);
                    //
                    MouseButtonEventArgs eventArgs = null;
                    if (leftMouseButtonState != lastLeftMouseButtonState) {
                        eventArgs = new MouseButtonEventArgs(source,
                            leftMouseButtonState == MouseButtonState.Pressed ? Control.PreviewMouseDownEvent : Control.PreviewMouseUpEvent,
                            rawPosition,
                            leftMouseButtonState,
                            lastMiddleMouseButtonState,
                            lastRightMouseButtonState,
                            MouseButton.Left
                            );
                    }
                    if (middleMouseButtonState != lastMiddleMouseButtonState) {
                        eventArgs = new MouseButtonEventArgs(source,
                            middleMouseButtonState == MouseButtonState.Pressed ? Control.PreviewMouseDownEvent : Control.PreviewMouseUpEvent,
                            rawPosition,
                            lastLeftMouseButtonState,
                            middleMouseButtonState,
                            lastRightMouseButtonState,
                            MouseButton.Middle
                            );
                    }
                    if (rightMouseButtonState != lastRightMouseButtonState) {
                        eventArgs = new MouseButtonEventArgs(source,
                            rightMouseButtonState == MouseButtonState.Pressed ? Control.PreviewMouseDownEvent : Control.PreviewMouseUpEvent,
                            rawPosition,
                            lastLeftMouseButtonState,
                            lastMiddleMouseButtonState,
                            rightMouseButtonState,
                            MouseButton.Right
                            );
                    }
                    if (eventArgs != null) eventsQueue.Enqueue(eventArgs);
                    //
                    lastLeftMouseButtonState = leftMouseButtonState;
                    lastMiddleMouseButtonState = middleMouseButtonState;
                    lastRightMouseButtonState = rightMouseButtonState;

                    if ( leftMouseButtonState == MouseButtonState.Pressed ) {
                        if ( eventArgs != null && !autoRepeatTimerRunning ) {
                            startAutoRepeatTimer( eventArgs );
                        }
                    } else {
                        if (eventArgs != null && autoRepeatTimerRunning) {
                            stopAutoRepeatTimer( );
                        }
                    }
                }

                if (mouseEvent.dwEventFlags == MouseEventFlags.MOUSE_WHEELED) {
                    MouseWheelEventArgs args = new MouseWheelEventArgs(
                        topMost,
                        Control.PreviewMouseWheelEvent,
                        rawPosition,
                        lastLeftMouseButtonState, lastMiddleMouseButtonState, 
                        lastRightMouseButtonState,
                        mouseEvent.dwButtonState > 0 ? 1 : -1
                    );
                    eventsQueue.Enqueue( args );
                }
            }
            if (inputRecord.EventType == EventType.KEY_EVENT) {
                KEY_EVENT_RECORD keyEvent = inputRecord.KeyEvent;
                KeyEventArgs eventArgs = new KeyEventArgs(
                    ConsoleApplication.Instance.FocusManager.FocusedElement,
                    keyEvent.bKeyDown ? Control.PreviewKeyDownEvent : Control.PreviewKeyUpEvent);
                eventArgs.UnicodeChar = keyEvent.UnicodeChar;
                eventArgs.bKeyDown = keyEvent.bKeyDown;
                eventArgs.dwControlKeyState = keyEvent.dwControlKeyState;
                eventArgs.wRepeatCount = keyEvent.wRepeatCount;
                eventArgs.wVirtualKeyCode = keyEvent.wVirtualKeyCode;
                eventArgs.wVirtualScanCode = keyEvent.wVirtualScanCode;
                eventsQueue.Enqueue(eventArgs);
            }
        }

        /// <summary>
        /// Processes all routed events in queue.
        /// </summary>
        public void ProcessEvents( ) {
            while (eventsQueue.Count != 0) {
                RoutedEventArgs routedEventArgs = eventsQueue.Dequeue();
                processRoutedEvent(routedEventArgs.RoutedEvent, routedEventArgs);
            }
        }

        public bool IsQueueEmpty( ) {
            return eventsQueue.Count == 0;
        }

        // todo : think about remove it
        internal bool ProcessRoutedEvent(RoutedEvent routedEvent, RoutedEventArgs args) {
            if (null == routedEvent)
                throw new ArgumentNullException("routedEvent");
            if (null == args)
                throw new ArgumentNullException("args");
            //
            return processRoutedEvent(routedEvent, args);
        }

        private static bool isControlAllowedToReceiveEvents( Control control, Control capturingControl ) {
            Control c = control;
            while ( true ) {
                if ( c == capturingControl ) return true;
                if ( c == null ) return false;
                c = c.Parent;
            }
        }

        private bool processRoutedEvent(RoutedEvent routedEvent, RoutedEventArgs args) {
            //
            List<RoutedEventTargetInfo> subscribedTargets = getTargetsSubscribedTo(routedEvent);

            Control capturingControl = inputCaptureStack.Count != 0 ? inputCaptureStack.Peek() : null;
            //
            if (routedEvent.RoutingStrategy == RoutingStrategy.Direct) {
                if (null == subscribedTargets)
                    return false;
                //
                RoutedEventTargetInfo targetInfo =
                    subscribedTargets.FirstOrDefault(info => info.target == args.Source);
                if (null == targetInfo)
                    return false;

                // если имеется контрол, захватывающий события, события получает только он сам
                // и его дочерние контролы
                if ( capturingControl != null ) {
                    if ( !(args.Source is Control) ) return false;
                    if ( !isControlAllowedToReceiveEvents( ( Control ) args.Source, capturingControl ) )
                        return false;
                }

                // copy handlersList to local list to avoid modifications when enumerating
                foreach (DelegateInfo delegateInfo in new List< DelegateInfo >(targetInfo.handlersList)) {
                    if (!args.Handled || delegateInfo.handledEventsToo) {
                        if (delegateInfo.@delegate is RoutedEventHandler) {
                            ((RoutedEventHandler) delegateInfo.@delegate).Invoke(targetInfo.target, args);
                        } else {
                            delegateInfo.@delegate.DynamicInvoke(targetInfo.target, args);
                        }
                    }
                }
            }

            Control source = (Control) args.Source;
            // path to source from root element down to Source
            List<Control> path = new List<Control>();
            Control current = source;
            while (null != current) {
                // та же логика с контролом, захватившим обработку сообщений
                // если имеется контрол, захватывающий события, события получает только он сам
                // и его дочерние контролы
                if ( capturingControl == null || isControlAllowedToReceiveEvents( current, capturingControl ) ) {
                    path.Insert( 0, current );
                    current = current.Parent;
                } else {
                    break;
                }
            }

            if (routedEvent.RoutingStrategy == RoutingStrategy.Tunnel) {
                if (subscribedTargets != null) {
                    foreach (Control potentialTarget in path) {
                        Control target = potentialTarget;
                        RoutedEventTargetInfo targetInfo =
                            subscribedTargets.FirstOrDefault(info => info.target == target);
                        if (null != targetInfo) {
                            foreach (DelegateInfo delegateInfo in new List< DelegateInfo >(targetInfo.handlersList)) {
                                if (!args.Handled || delegateInfo.handledEventsToo) {
                                    if (delegateInfo.@delegate is RoutedEventHandler) {
                                        ((RoutedEventHandler) delegateInfo.@delegate).Invoke(target, args);
                                    } else {
                                        delegateInfo.@delegate.DynamicInvoke(target, args);
                                    }
                                }
                            }
                        }
                    }
                }
                // для парных Preview-событий запускаем соответствующие настоящие события,
                // сохраняя при этом Handled (если Preview событие помечено как Handled=true,
                // то и настоящее событие будет маршрутизировано с Handled=true)
                if (routedEvent == Control.PreviewMouseDownEvent) {
                    MouseButtonEventArgs mouseArgs = ( ( MouseButtonEventArgs ) args );
                    MouseButtonEventArgs argsNew = new MouseButtonEventArgs(
                        args.Source, Control.MouseDownEvent, mouseArgs.RawPosition,
                        mouseArgs.LeftButton, mouseArgs.MiddleButton, mouseArgs.RightButton,
                        mouseArgs.ChangedButton
                    );
                    argsNew.Handled = args.Handled;
                    eventsQueue.Enqueue(argsNew);
                }
                if (routedEvent == Control.PreviewMouseUpEvent) {
                    MouseButtonEventArgs mouseArgs = ( ( MouseButtonEventArgs ) args );
                    MouseButtonEventArgs argsNew = new MouseButtonEventArgs(
                        args.Source, Control.MouseUpEvent, mouseArgs.RawPosition,
                        mouseArgs.LeftButton, mouseArgs.MiddleButton, mouseArgs.RightButton,
                        mouseArgs.ChangedButton
                    );
                    argsNew.Handled = args.Handled;
                    eventsQueue.Enqueue(argsNew);
                }
                if (routedEvent == Control.PreviewMouseMoveEvent) {
                    MouseEventArgs mouseArgs = ( ( MouseEventArgs ) args );
                    MouseEventArgs argsNew = new MouseEventArgs(
                        args.Source, Control.MouseMoveEvent, mouseArgs.RawPosition,
                        mouseArgs.LeftButton, mouseArgs.MiddleButton, mouseArgs.RightButton
                    );
                    argsNew.Handled = args.Handled;
                    eventsQueue.Enqueue(argsNew);
                }
                if ( routedEvent == Control.PreviewMouseWheelEvent ) {
                    MouseWheelEventArgs oldArgs = ((MouseWheelEventArgs)args);
                    MouseEventArgs argsNew = new MouseWheelEventArgs(
                        args.Source, Control.MouseWheelEvent, oldArgs.RawPosition,
                        oldArgs.LeftButton, oldArgs.MiddleButton, oldArgs.RightButton,
                        oldArgs.Delta
                    );
                    argsNew.Handled = args.Handled;
                    eventsQueue.Enqueue(argsNew);
                }

                if (routedEvent == Control.PreviewKeyDownEvent) {
                    KeyEventArgs argsNew = new KeyEventArgs(args.Source, Control.KeyDownEvent);
                    KeyEventArgs keyEventArgs = ( ( KeyEventArgs ) args );
                    argsNew.UnicodeChar = keyEventArgs.UnicodeChar;
                    argsNew.bKeyDown = keyEventArgs.bKeyDown;
                    argsNew.dwControlKeyState = keyEventArgs.dwControlKeyState;
                    argsNew.wRepeatCount = keyEventArgs.wRepeatCount;
                    argsNew.wVirtualKeyCode = keyEventArgs.wVirtualKeyCode;
                    argsNew.wVirtualScanCode = keyEventArgs.wVirtualScanCode;
                    argsNew.Handled = args.Handled;
                    eventsQueue.Enqueue(argsNew);
                }
                if (routedEvent == Control.PreviewKeyUpEvent) {
                    KeyEventArgs argsNew = new KeyEventArgs(args.Source, Control.KeyUpEvent);
                    KeyEventArgs keyEventArgs = ( ( KeyEventArgs ) args );
                    argsNew.UnicodeChar = keyEventArgs.UnicodeChar;
                    argsNew.bKeyDown = keyEventArgs.bKeyDown;
                    argsNew.dwControlKeyState = keyEventArgs.dwControlKeyState;
                    argsNew.wRepeatCount = keyEventArgs.wRepeatCount;
                    argsNew.wVirtualKeyCode = keyEventArgs.wVirtualKeyCode;
                    argsNew.wVirtualScanCode = keyEventArgs.wVirtualScanCode;
                    argsNew.Handled = args.Handled;
                    eventsQueue.Enqueue(argsNew);
                }
            }

            if (routedEvent.RoutingStrategy == RoutingStrategy.Bubble) {
                if (subscribedTargets != null) {
                    for (int i = path.Count - 1; i >= 0; i--) {
                        Control target = path[i];
                        RoutedEventTargetInfo targetInfo =
                            subscribedTargets.FirstOrDefault(info => info.target == target);
                        if (null != targetInfo) {
                            //
                            foreach (DelegateInfo delegateInfo in new List< DelegateInfo >(targetInfo.handlersList)) {
                                if (!args.Handled || delegateInfo.handledEventsToo) {
                                    if (delegateInfo.@delegate is RoutedEventHandler) {
                                        ((RoutedEventHandler) delegateInfo.@delegate).Invoke(target, args);
                                    } else {
                                        delegateInfo.@delegate.DynamicInvoke(target, args);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return args.Handled;
        }

        /// <summary>
        /// Adds specified routed event to event queue. This event will be processed in next pass.
        /// </summary>
        internal void QueueEvent(RoutedEvent routedEvent, RoutedEventArgs args) {
            if (routedEvent != args.RoutedEvent)
                throw new ArgumentException("Routed event doesn't match to routedEvent passed.", "args");
            this.eventsQueue.Enqueue(args);
        }
    }
}