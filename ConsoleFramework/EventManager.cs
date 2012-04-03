using System;
using System.Collections.Generic;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;
using ConsoleFramework.Native;

namespace ConsoleFramework {
    /// <summary>
    /// Central point of events management routine.
    /// Provides events routing.
    /// </summary>
    public sealed class EventManager {
        private readonly Stack<Control> inputCaptureStack = new Stack<Control>();

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