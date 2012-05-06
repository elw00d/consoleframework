using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ConsoleFramework.Controls;

namespace ConsoleFramework.Events
{
    /// <summary>
    /// Responsible to manage elements that has a keyboard focus.
    /// Also maintains the console mouse cursor visibility according to
    /// current focused control.
    /// </summary>
    public sealed class FocusManager {
        private readonly EventManager eventManager;

        public FocusManager(EventManager eventManager) {
            if (null == eventManager)
                throw new ArgumentNullException("eventManager");
            this.eventManager = eventManager;
        }

        /// <summary>
        /// Refreshes the display of mouse cursor according to current focused element
        /// and its cursor visibility status.
        /// 
        /// Вызывается после обновления лайаута (если были какие-то изменения),
        /// а также при смене FocusedElement либо при изменении локального состояния курсора
        /// на элементе уплавления, который сейчас удерживает фокус клавиатурного ввода.
        /// </summary>
        internal void RefreshMouseCursor() {
            if (null != focusedElement && focusedElement.CursorVisible && focusedElement.IsPointVisible(focusedElement.CursorPosition)) {
                ConsoleApplication.Instance.SetCursorPosition(Control.TranslatePoint(focusedElement, focusedElement.CursorPosition, null));
                if (!ConsoleApplication.Instance.CursorIsVisible) {
                    ConsoleApplication.Instance.ShowCursor();
                }
            } else {
                if (ConsoleApplication.Instance.CursorIsVisible) {
                    ConsoleApplication.Instance.HideCursor();
                }
            }
        }

        private Control focusedElement;

        /// <summary>
        /// Control that has a keyboard focus now.
        /// </summary>
        public Control FocusedElement {
            get {
                return focusedElement;
            }
            private set {
                if (focusedElement != value) {
                    focusedElement = value;
                    RefreshMouseCursor();
                }
            }
        }

        private bool tryChangeFocusedElementTo(Control focusedControl, bool ignorePreviewHandled = false) {
            if (focusedControl == FocusedElement) {
                return true; // do nothing
            }
            //
            Control oldFocus = FocusedElement;
            // генерируем Preview-события
            // если Handled = true хотя бы для одного из Preview-событий, то метод возвращает false
            // и фокус не меняется, а состояние Focused для измененных элементов визуального дерева
            // возвращается как было
            if (oldFocus != null) {
                KeyboardFocusChangedEventArgs previewLostArgs = new KeyboardFocusChangedEventArgs(oldFocus,
                    Control.PreviewLostKeyboardFocusEvent, oldFocus, focusedControl);
                if (eventManager.ProcessRoutedEvent(previewLostArgs.RoutedEvent, previewLostArgs) && !ignorePreviewHandled) {
                    return false;
                }
            }
            KeyboardFocusChangedEventArgs previewGotArgs = new KeyboardFocusChangedEventArgs(focusedControl,
                    Control.PreviewGotKeyboardFocusEvent, oldFocus, focusedControl);
            if (eventManager.ProcessRoutedEvent(previewGotArgs.RoutedEvent, previewGotArgs) && !ignorePreviewHandled) {
                return false;
            }

            // меняем фокусный элемент и генерируем основные события
            FocusedElement = focusedControl;
            
            if (oldFocus != null) {
                KeyboardFocusChangedEventArgs lostArgs = new KeyboardFocusChangedEventArgs(oldFocus,
                    Control.LostKeyboardFocusEvent, oldFocus, focusedControl);
                eventManager.ProcessRoutedEvent(lostArgs.RoutedEvent, lostArgs);
            }
            KeyboardFocusChangedEventArgs args = new KeyboardFocusChangedEventArgs(focusedControl,
                    Control.GotKeyboardFocusEvent, oldFocus, focusedControl);
            eventManager.ProcessRoutedEvent(args.RoutedEvent, args);
            //
            return true;
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

        /// <summary>
        /// Устанавливает клавиатурный фокус на выбранном элементе.
        /// Если при установке фокуса события PreviewLostKeyboardFocus и PreviewGotKeyboardFocus
        /// были перехвачены и Handled был установлен в true, клавиатурный фокус не изменяется, а
        /// состояние фокуса элементов возвращается к прежнему.
        /// </summary>
        /// <param name="control">Элемент управления, на который необходимо передать фокус.</param>
        /// <param name="ignoreRememberedChildrenFocus">Если true, то фокус дочерних элементов будет сброшен
        /// в дефолтный. По умолчанию фокус дочерних элементов восстановится к такому же состоянию, как и д
        /// потери элементом фокуса.</param>
        internal void SetFocus(Control control, bool ignoreRememberedChildrenFocus = false) {
            if (null == control)
                throw new ArgumentNullException("control");
            if (!isControlConnectedToRoot(control))
                throw new ArgumentException("Control must be in visual tree.", "control");
            //
            if (!control.Visible || !control.Focusable) {
                return; // do nothing
            }
            //
            List<Tuple<Control, bool>> controlsOriginalFocusState = new List<Tuple<Control, bool>>();

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
                        controlsOriginalFocusState.Add(new Tuple<Control, bool>(child, child.Focused));
                        child.Focused = false;
                    }
                }
                controlsOriginalFocusState.Add(new Tuple<Control, bool>(current, current.Focused));
                current.Focused = true;
                //
                current = current.Parent;
            }
            // set focused = true for children
            Control focusedControl = null;
            if (!failed) {
                if (ignoreRememberedChildrenFocus)
                    focusedControl = setDefaultFocusOnSubtree(control, controlsOriginalFocusState);
                else
                    focusedControl = setFocusOnSubtree(control, controlsOriginalFocusState);
                //
                if (null == focusedControl) {
                    failed = true;
                    Debug.WriteLine(string.Format("Failed to set focus on control : {0}", control.Name));
                }
            }
            //
            if (!failed) {
                if (!tryChangeFocusedElementTo(focusedControl)) {
                    failed = true;
                }
            }

            if (failed) {
                // restore original focus states
                foreach (Tuple<Control, bool> originalFocusState in controlsOriginalFocusState) {
                    originalFocusState.Item1.Focused = originalFocusState.Item2;
                }
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
        internal void AfterAddElementToTree(Control control) {
            if (control.Parent == null) {
                // this is root element
                // в этом месте установка значения Handled = true в обработчике PreviewGotKeyboardFocus
                // игнорируется (поскольку мы обязаны при инициализации назначить фокусный элемент)
                Control defaultFocus = setDefaultFocusOnSubtree(control, null);
                tryChangeFocusedElementTo(defaultFocus, true);
            } else {
                if (control.Parent.Focused) {
                    List<Tuple<Control, bool>> controlsOriginalFocusState = new List<Tuple<Control, bool>>();
                    Control defaultFocus = setDefaultFocusOnSubtree(control, controlsOriginalFocusState);
                    bool failed = defaultFocus == null;
                    if (!failed) {
                        if (control.Parent == FocusedElement) {
                            failed = !tryChangeFocusedElementTo(defaultFocus);
                        }
                    }
                    if (failed) {
                        // restore original focus states
                        foreach (Tuple<Control, bool> originalFocusState in controlsOriginalFocusState) {
                            originalFocusState.Item1.Focused = originalFocusState.Item2;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Должен быть вызван перед удалением поддерева элементов.
        /// Если удаляется поддерево, имеющее в данный момент фокус, то фокус переходит
        /// к элементу, из которого поддерево вынимается. Например, из окна вытащили панель с текстбоксом,
        /// который имел фокус ввода. Фокус автоматически будет назначен на окно.
        /// Этот юзкейс не отменяется установкой Handled = true в Preview-событиях.
        /// </summary>
        internal void BeforeRemoveElementFromTree(Control control) {
            if (null == control)
                throw new ArgumentNullException("control");
            // if removing root element
            if (null == control.Parent) {
                tryChangeFocusedElementTo(null, true);
                return;
            }
            //
            if (isFocusedElementInSubtree(control)) {
                if (!control.Parent.Focused) {
                    throw new InvalidOperationException("Assertion failed.");
                }
                tryChangeFocusedElementTo(control.Parent, true);
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
        private Control setFocusOnSubtree(Control control, List<Tuple<Control, bool>> controlsOriginalFocusState) {
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
                    return setFocusOnSubtree(alreadyFocused, controlsOriginalFocusState);
                for (int i = children.Count - 1; i >= 0; i--) {
                    Control child = children[i];
                    Control result = setFocusOnSubtree(child, controlsOriginalFocusState);
                    if (result != null)
                        return result;
                }
            }
            controlsOriginalFocusState.Add(new Tuple<Control, bool>(control, control.Focused));
            control.Focused = true;
            return control;
        }

        /// <summary>
        /// Устанавливает фокусные элементы по умолчанию в поддереве визуальных элементов, начинающемся с
        /// элемента управления control. Возвращает самый верхний контрол (который получил бы
        /// keyboard focus, если бы фокус получил control).
        /// </summary>
        private Control setDefaultFocusOnSubtree(Control control, List<Tuple<Control, bool>> controlsOriginalFocusState) {
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
                    Control result = setDefaultFocusOnSubtree(child, controlsOriginalFocusState);
                    if (result != null)
                        return result;
                }
            }
            if (null != controlsOriginalFocusState) {
                controlsOriginalFocusState.Add(new Tuple<Control, bool>(control, control.Focused));
            }
            control.Focused = true;
            return control;
        }
    }
}
