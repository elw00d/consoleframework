using System;
using System.Collections.Generic;
using System.Linq;
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

        public void ProcessEvent(INPUT_RECORD inputRecord, Control rootElement, Rect rootElementRect) {
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
                Point rawPosition = new Point(mouseEvent.dwMousePosition.X, mouseEvent.dwMousePosition.Y);
                Control source = findSource(rawPosition, rootElement);
                //
                if (mouseEvent.dwEventFlags == MouseEventFlags.MOUSE_MOVED) {
                    MouseButtonState leftMouseButtonState = getLeftButtonState(mouseEvent.dwButtonState);
                    MouseButtonState middleMouseButtonState = getMiddleButtonState(mouseEvent.dwButtonState);
                    MouseButtonState rightMouseButtonState = getRightButtonState(mouseEvent.dwButtonState);
                    //
                    MouseEventArgs mouseEventArgs = new MouseEventArgs(source, Control.MouseMoveEvent,
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
                }
                if (mouseEvent.dwEventFlags == MouseEventFlags.PRESSED_OR_RELEASED) {
                    //
                    MouseButtonState leftMouseButtonState = getLeftButtonState(mouseEvent.dwButtonState);
                    MouseButtonState middleMouseButtonState = getMiddleButtonState(mouseEvent.dwButtonState);
                    MouseButtonState rightMouseButtonState = getRightButtonState(mouseEvent.dwButtonState);
                    //
                    if (leftMouseButtonState != lastLeftMouseButtonState) {
                        MouseButtonEventArgs eventArgs = new MouseButtonEventArgs(source,
                            leftMouseButtonState == MouseButtonState.Pressed ? Control.PreviewMouseDownEvent : Control.PreviewMouseUpEvent,
                            rawPosition,
                            leftMouseButtonState,
                            lastMiddleMouseButtonState,
                            lastRightMouseButtonState,
                            MouseButton.Left
                            );
                        eventsQueue.Enqueue(eventArgs);
                    }
                    if (middleMouseButtonState != lastMiddleMouseButtonState) {
                        MouseButtonEventArgs eventArgs = new MouseButtonEventArgs(source,
                            middleMouseButtonState == MouseButtonState.Pressed ? Control.PreviewMouseDownEvent : Control.PreviewMouseUpEvent,
                            rawPosition,
                            lastLeftMouseButtonState,
                            middleMouseButtonState,
                            lastRightMouseButtonState,
                            MouseButton.Middle
                            );
                        eventsQueue.Enqueue(eventArgs);
                    }
                    if (rightMouseButtonState != lastRightMouseButtonState) {
                        MouseButtonEventArgs eventArgs = new MouseButtonEventArgs(source,
                            rightMouseButtonState == MouseButtonState.Pressed ? Control.PreviewMouseDownEvent : Control.PreviewMouseUpEvent,
                            rawPosition,
                            lastLeftMouseButtonState,
                            lastMiddleMouseButtonState,
                            rightMouseButtonState,
                            MouseButton.Right
                            );
                        eventsQueue.Enqueue(eventArgs);
                    }
                    //
                    lastLeftMouseButtonState = leftMouseButtonState;
                    lastMiddleMouseButtonState = middleMouseButtonState;
                    lastRightMouseButtonState = rightMouseButtonState;
                }
                // todo : add whelled and double click handling
                //Debug.WriteLine(mouseEvent.dwEventFlags);
            }
            if (inputRecord.EventType == EventType.KEY_EVENT) {
                KEY_EVENT_RECORD keyEvent = inputRecord.KeyEvent;
                KeyEventArgs eventArgs = new KeyEventArgs(findSource(rootElement),
                    keyEvent.bKeyDown ? Control.PreviewKeyDownEvent : Control.PreviewKeyUpEvent);
                eventArgs.UnicodeChar = keyEvent.UnicodeChar;
                eventArgs.bKeyDown = keyEvent.bKeyDown;
                eventArgs.dwControlKeyState = keyEvent.dwControlKeyState;
                eventArgs.wRepeatCount = keyEvent.wRepeatCount;
                eventArgs.wVirtualKeyCode = keyEvent.wVirtualKeyCode;
                eventArgs.wVirtualScanCode = keyEvent.wVirtualScanCode;
                eventsQueue.Enqueue(eventArgs);
            }
            //
            while (eventsQueue.Count != 0) {
                RoutedEventArgs routedEventArgs = eventsQueue.Dequeue();
                processRoutedEvent(routedEventArgs.RoutedEvent, routedEventArgs);
            }
        }

        private void processRoutedEvent(RoutedEvent routedEvent, RoutedEventArgs args) {
            //
            List<RoutedEventTargetInfo> subscribedTargets = getTargetsSubscribedTo(routedEvent);
            //
            if (routedEvent.RoutingStrategy == RoutingStrategy.Direct) {
                if (subscribedTargets != null) {
                    foreach (RoutedEventTargetInfo targetInfo in subscribedTargets) {
                        foreach (DelegateInfo delegateInfo in targetInfo.handlersList) {
                            if (!args.Handled || delegateInfo.handledEventsToo) {
                                if (delegateInfo.@delegate is RoutedEventHandler) {
                                    ((RoutedEventHandler) delegateInfo.@delegate).Invoke(args.Source, args);
                                } else {
                                    delegateInfo.@delegate.DynamicInvoke(args.Source, args);
                                }
                            }
                        }
                    }
                }
                return;
            }

            Control source = (Control) args.Source;
            // path to source from root element down
            List<Control> path = new List<Control>();
            Control current = source;
            while (null != current) {
                path.Insert(0, current);
                current = current.Parent;
            }

            if (routedEvent.RoutingStrategy == RoutingStrategy.Tunnel) {
                if (subscribedTargets != null) {
                    foreach (Control potentialTarget in path) {
                        Control target = potentialTarget;
                        RoutedEventTargetInfo targetInfo =
                            subscribedTargets.FirstOrDefault(info => info.target == target);
                        if (null != targetInfo) {
                            foreach (DelegateInfo delegateInfo in targetInfo.handlersList) {
                                if (!args.Handled || delegateInfo.handledEventsToo) {
                                    if (delegateInfo.@delegate is RoutedEventHandler) {
                                        ((RoutedEventHandler) delegateInfo.@delegate).Invoke(args.Source, args);
                                    } else {
                                        delegateInfo.@delegate.DynamicInvoke(args.Source, args);
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
                    MouseButtonEventArgs argsNew = new MouseButtonEventArgs(
                        args.Source, Control.MouseDownEvent,
                        ((MouseButtonEventArgs) args).RawPosition,
                        ((MouseButtonEventArgs)args).LeftButton,
                        ((MouseButtonEventArgs)args).MiddleButton,
                        ((MouseButtonEventArgs)args).RightButton,
                        ((MouseButtonEventArgs)args).ChangedButton
                        );
                    argsNew.Handled = args.Handled;
                    eventsQueue.Enqueue(argsNew);
                }
                if (routedEvent == Control.PreviewMouseUpEvent) {
                    MouseButtonEventArgs argsNew = new MouseButtonEventArgs(
                        args.Source, Control.MouseUpEvent,
                        ((MouseButtonEventArgs)args).RawPosition,
                        ((MouseButtonEventArgs)args).LeftButton,
                        ((MouseButtonEventArgs)args).MiddleButton,
                        ((MouseButtonEventArgs)args).RightButton,
                        ((MouseButtonEventArgs)args).ChangedButton
                        );
                    argsNew.Handled = args.Handled;
                    eventsQueue.Enqueue(argsNew);
                }
                if (routedEvent == Control.PreviewMouseMoveEvent) {
                    MouseEventArgs argsNew = new MouseEventArgs(
                        args.Source, Control.MouseMoveEvent,
                        ((MouseButtonEventArgs)args).RawPosition,
                        ((MouseButtonEventArgs)args).LeftButton,
                        ((MouseButtonEventArgs)args).MiddleButton,
                        ((MouseButtonEventArgs)args).RightButton
                        );
                    argsNew.Handled = args.Handled;
                    eventsQueue.Enqueue(argsNew);
                }
                // todo : add mouse wheel support

                if (routedEvent == Control.PreviewKeyDownEvent) {
                    KeyEventArgs argsNew = new KeyEventArgs(args.Source, Control.KeyDownEvent);
                    argsNew.UnicodeChar = ((KeyEventArgs) args).UnicodeChar;
                    argsNew.bKeyDown = ((KeyEventArgs)args).bKeyDown;
                    argsNew.dwControlKeyState = ((KeyEventArgs)args).dwControlKeyState;
                    argsNew.wRepeatCount = ((KeyEventArgs)args).wRepeatCount;
                    argsNew.wVirtualKeyCode = ((KeyEventArgs)args).wVirtualKeyCode;
                    argsNew.wVirtualScanCode = ((KeyEventArgs)args).wVirtualScanCode;
                    argsNew.Handled = args.Handled;
                    eventsQueue.Enqueue(argsNew);
                }
                if (routedEvent == Control.PreviewKeyUpEvent) {
                    KeyEventArgs argsNew = new KeyEventArgs(args.Source, Control.KeyUpEvent);
                    argsNew.UnicodeChar = ((KeyEventArgs)args).UnicodeChar;
                    argsNew.bKeyDown = ((KeyEventArgs)args).bKeyDown;
                    argsNew.dwControlKeyState = ((KeyEventArgs)args).dwControlKeyState;
                    argsNew.wRepeatCount = ((KeyEventArgs)args).wRepeatCount;
                    argsNew.wVirtualKeyCode = ((KeyEventArgs)args).wVirtualKeyCode;
                    argsNew.wVirtualScanCode = ((KeyEventArgs)args).wVirtualScanCode;
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
                            foreach (DelegateInfo delegateInfo in targetInfo.handlersList) {
                                if (!args.Handled || delegateInfo.handledEventsToo) {
                                    if (delegateInfo.@delegate is RoutedEventHandler) {
                                        ((RoutedEventHandler) delegateInfo.@delegate).Invoke(args.Source, args);
                                    } else {
                                        delegateInfo.@delegate.DynamicInvoke(args.Source, args);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Находит активный элемент (который находится сейчас в фокусе ввода).
        /// </summary>
        /// <param name="rootElement"></param>
        /// <returns></returns>
        private Control findSource(Control rootElement) {
            if (inputCaptureStack.Count != 0) {
                return inputCaptureStack.Peek();
            }
            if (rootElement.children.Count != 0) {
                List<Control> childrenOrderedByZIndex = rootElement.GetChildrenOrderedByZIndex();
                return findSource(childrenOrderedByZIndex[childrenOrderedByZIndex.Count - 1]);
            }
            return rootElement;
        }

        /// <summary>
        /// Находит самый верхний элемент под указателем мыши с координатами rawPoint.
        /// </summary>
        /// <param name="rawPoint"></param>
        /// <param name="rootElement"></param>
        /// <returns></returns>
        private Control findSource(Point rawPoint, Control rootElement) {
            if (inputCaptureStack.Count != 0) {
                return inputCaptureStack.Peek();
            }
            if (rootElement.children.Count != 0) {
                List<Control> childrenOrderedByZIndex = rootElement.GetChildrenOrderedByZIndex();
                for (int i = childrenOrderedByZIndex.Count - 1; i >= 0; i--) {
                    Control child = childrenOrderedByZIndex[i];
                    Point point = Control.TranslatePoint(null, rawPoint, rootElement);
                    if (child.RenderSlotRect.Contains(point)) {
                        return findSource(rawPoint, child);
                    }
                }
            }
            return rootElement;
        }
    }
}