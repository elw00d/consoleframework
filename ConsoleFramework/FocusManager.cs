using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ConsoleFramework.Controls;
using ConsoleFramework.Events;

namespace ConsoleFramework
{
    public class KeyboardFocusChangedEventArgs : RoutedEventArgs {
        public KeyboardFocusChangedEventArgs(object source, RoutedEvent routedEvent) : base(source, routedEvent) {
        }

        public KeyboardFocusChangedEventArgs(object source, RoutedEvent routedEvent,
            Control oldFocus, Control newFocus) : base(source, routedEvent) {
            //
            OldFocus = oldFocus;
            NewFocus = newFocus;
        }

        public Control OldFocus {
            get;
            private set;
        }

        public Control NewFocus {
            get;
            private set;
        }
    }

    public delegate void KeyboardFocusChangedEventHandler(object sender, KeyboardFocusChangedEventArgs args);

    public sealed class FocusManager {
        private readonly EventManager eventManager;

        public FocusManager(EventManager eventManager) {
            if (null == eventManager)
                throw new ArgumentNullException("eventManager");
            this.eventManager = eventManager;
        }

        public Control FocusedElement {
            get;
            private set;
        }

        private void changeFocusedElementTo(Control focusedElement) {
            if (focusedElement == FocusedElement) {
                // do nothing
                return;
            }
            Control oldFocus = FocusedElement;
            FocusedElement = focusedElement;
            //
            //if (isControlConnectedToRoot(focusedElement)) {
                if (oldFocus != null) {
                    KeyboardFocusChangedEventArgs lostArgs = new KeyboardFocusChangedEventArgs(oldFocus,
                        Control.LostKeyboardFocusEvent, oldFocus, focusedElement);
                    eventManager.ProcessRoutedEvent(lostArgs.RoutedEvent, lostArgs);
                }
                KeyboardFocusChangedEventArgs args = new KeyboardFocusChangedEventArgs(focusedElement,
                        Control.GotKeyboardFocusEvent, oldFocus, focusedElement);
                eventManager.ProcessRoutedEvent(args.RoutedEvent, args);
            //}
        }

        /// <summary>
        /// Проверяет, находится ли контрол в дереве визуальных элементов, или еще
        /// не подключен к нему. В зависимости от этого либо будут генерироваться события
        /// GotKeyboardFocus/LostKeyboardFocus, либо нет.
        /// </summary>
        private bool isControlConnectedToRoot(Control control) {
            Control rootElement = ConsoleApplication.Instance.Renderer.RootElement;
            Control current = control;
            do {
                if (current == null)
                    return false;
                if (current == rootElement)
                    return true;
                current = current.Parent;
            } while (true);
        }

        public void SetFocus(Control control) {
            if (null == control)
                throw new ArgumentNullException("control");
            // todo : check that control is part of visuals tree
            if (!control.Visible || !control.Focusable)
                // do nothing
                return;
            // set focused = true for parents
            bool failed = false;
            Control current = control;
            while (current != null) {
                if (!current.Visible || !current.Focusable) {
                    failed = true;
                    break;
                }
                //
                Control parent = current.Parent;
                if (parent != null && parent.children.Count > 1) {
                    // invariant
                    Debug.Assert(parent.children.Count(c => c.Focused) <= 1);
                    foreach (Control child in parent.children) {
                        child.Focused = false;
                    }
                }
                current.Focused = true;
                //
                current = current.Parent;
            }
            // set focused = true for children
            Control focusedControl = null;
            if (!failed) {
                focusedControl = setFocusOnSubtree(control);
                if (null == focusedControl) {
                    failed = true;
                    Debug.WriteLine(string.Format("Failed to set focus on control : {0}", control.Name));
                }
            }
            //
            if (!failed) {
                changeFocusedElementTo(focusedControl);
            }
            //
        }

        /// <summary>
        /// Должен вызываться после добавления контрола в дерево визуальных элементов.
        /// Если был добавлен Root Element, то выполняется инициализация фокуса - фокусным
        /// элементом становится самый верхний (в соответствии с z-order) focusable контрол.
        /// Если был добавлено поддерево элементов в существующее дерево элементов, и если
        /// тот элемент, куда было вставлено поддерево, имел фокус - то фокус автоматически
        /// проваливается до верхнего элемента среди вставленных.
        /// К примеру, если мы в окно, имеющее фокус ввода, вставляем панель с текстбоксом, то
        /// текстбокс автоматически получает клавиатурный фокус.
        /// </summary>
        public void AfterAddElementToTree(Control control) {
            if (control.Parent == null) {
                // this is root element
                Control defaultFocus = setDefaultFocusOnSubtree(control);
                changeFocusedElementTo(defaultFocus);
            } else {
                if (control.Parent.Focused) {
                    Control defaultFocus = setDefaultFocusOnSubtree(control);
                    if (control.Parent == FocusedElement) {
                        changeFocusedElementTo(defaultFocus);
                    }
                }
            }
        }

        /// <summary>
        /// Должен быть вызван перед удалением поддерева элементов.
        /// Если удаляется поддерево, имеющее в данный момент фокус, то фокус переходит
        /// к элементу, из которого поддерево вынимается. Например, из окна вытащили панель с текстбоксом,
        /// который имел фокус ввода. Фокус автоматически будет назначен на окно.
        /// </summary>
        public void BeforeRemoveElementFromTree(Control control) {
            if (null == control)
                throw new ArgumentNullException("control");
            // if removing root element
            if (null == control.Parent) {
                changeFocusedElementTo(null);
                return;
            }
            //
            if (isFocusedElementInSubtree(control)) {
                if (!control.Parent.Focused) {
                    throw new InvalidOperationException("Assertion failed.");
                }
                changeFocusedElementTo(control.Parent);
            }
        }

        /// <summary>
        /// Определяет, содержит ли поддерево визуальных элементов, начинающееся с control,
        /// текущий элемент, удерживающий в данных момент клавиатурный фокус - FocusedElement.
        /// </summary>
        private bool isFocusedElementInSubtree(Control control) {
            Control current = FocusedElement;
            while (null != current) {
                if (current == control)
                    return true;
                current = current.Parent;
            }
            return false;
        }

        /// <summary>
        /// Устанавливает фокусные элементы в поддереве визуальных элементов, начинающемся с
        /// элемента управления control. Возвращает самый верхний контрол (который получил бы
        /// keyboard focus, если бы фокус получил control). Учитывает свойства Focused, уже
        /// установленные у контролов (если такие есть).
        /// </summary>
        private Control setFocusOnSubtree(Control control) {
            if (null == control)
                throw new ArgumentNullException("control");
            if (!control.Visible || !control.Focusable)
                return null;
            //
            if (control.children.Count != 0) {
                List<Control> children = control.GetChildrenOrderedByZIndex();
                Debug.Assert(children.Count(c => c.Focused) <= 1);
                Control alreadyFocused = children.FirstOrDefault(c => c.Focused);
                if (null != alreadyFocused)
                    return setFocusOnSubtree(alreadyFocused);
                for (int i = children.Count - 1; i >= 0; i--) {
                    Control child = children[i];
                    Control result = setFocusOnSubtree(child);
                    if (result != null)
                        return result;
                }
            }
            control.Focused = true;
            return control;
        }

        /// <summary>
        /// Устанавливает фокусные элементы по умолчанию в поддереве визуальных элементов, начинающемся с
        /// элемента управления control. Возвращает самый верхний контрол (который получил бы
        /// keyboard focus, если бы фокус получил control).
        /// </summary>
        private Control setDefaultFocusOnSubtree(Control control) {
            if (null == control)
                throw new ArgumentNullException("control");
            if (!control.Visible || !control.Focusable)
                return null;
            //
            if (control.children.Count != 0) {
                List<Control> children = control.GetChildrenOrderedByZIndex();
                Debug.Assert(children.Count(c => c.Focused) <= 1);
                Control alreadyFocused = children.FirstOrDefault(c => c.Focused);
                if (null != alreadyFocused)
                    alreadyFocused.Focused = false;
                for (int i = children.Count - 1; i >= 0; i--) {
                    Control child = children[i];
                    Control result = setFocusOnSubtree(child);
                    if (result != null)
                        return result;
                }
            }
            control.Focused = true;
            return control;
        }
    }
}
