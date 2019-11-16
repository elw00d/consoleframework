using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ConsoleFramework.Controls;
using ConsoleFramework.Core;

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

        /// <summary>
        /// Забирает фокус у текущего фокусного элемента и устанавливает фокус указанному элементу управления.
        /// Если focusedControl - null, то FocusedElement будет установлен в null и более клавиатурный
        /// ввод не будет обрабатываться до тех пор, пока фокус не будет отдан другому контролу.
        /// </summary>
        /// <param name="focusedControl"></param>
        /// <param name="ignorePreviewHandled"></param>
        /// <returns></returns>
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
            if (null != focusedControl)
            {
                KeyboardFocusChangedEventArgs previewGotArgs = new KeyboardFocusChangedEventArgs(focusedControl,
                        Control.PreviewGotKeyboardFocusEvent, oldFocus, focusedControl);
                if (eventManager.ProcessRoutedEvent(previewGotArgs.RoutedEvent, previewGotArgs) && !ignorePreviewHandled)
                {
                    return false;
                }
            }

            // меняем фокусный элемент и генерируем основные события
            FocusedElement = focusedControl;
            
            if (oldFocus != null) {
                KeyboardFocusChangedEventArgs lostArgs = new KeyboardFocusChangedEventArgs(oldFocus,
                    Control.LostKeyboardFocusEvent, oldFocus, focusedControl);
                eventManager.ProcessRoutedEvent(lostArgs.RoutedEvent, lostArgs);
            }
            if (null != focusedControl)
            {
                KeyboardFocusChangedEventArgs args = new KeyboardFocusChangedEventArgs(focusedControl,
                        Control.GotKeyboardFocusEvent, oldFocus, focusedControl);
                eventManager.ProcessRoutedEvent(args.RoutedEvent, args);
            }
            //
            return true;
        }

        private Control currentScope;
        /// <summary>
        /// Текущая область фокуса
        /// </summary>
        public Control CurrentScope
        {
            get { return currentScope; }
        }

        /// <summary>
        /// Устанавливает текущую область фокуса. Область фокуса задаётся родительским элементом scope.
        /// Все его дочерние Focusable-элементы после этого могут получать фокус.
        /// Изначально фокус получит первый Focusable элемент.
        /// Если область фокуса не содержит Focusable элементов, операция не будет выполнена.
        /// </summary>
        /// <param name="scope"></param>
        public void SetFocusScope(Control scope)
        {
            SetFocus(scope, null);
        }

        /// <summary>
        /// Находит первую подходящую область фокуса среди родительских элементов указанного
        /// контрола, и устанавливает соответствующий фокус. Первая подходящая - это первый
        /// вверх по иерархии контролов родительский контрол, у которого свойство IsFocusScope = True.
        /// Если control - null, то фокус будет убран, и клавиатурный ввод больше не будет обрабатываться
        /// (не будут генерироваться маршрутизируемые события, назначаемые текущему фокусному элементу).
        /// </summary>
        /// <param name="control"></param>
        public void SetFocus(Control control)
        {
            if (null == control)
            {
                this.currentScope = null;
                tryChangeFocusedElementTo(null);
                return;
            }

            Control closestFocusScope = findClosestScope(control);
            if (null == closestFocusScope) 
                throw new InvalidOperationException("Cannot set focus to control because no focus scope found up to visual tree");

            SetFocus(closestFocusScope, control);
        }

        /// <summary>
        /// Находит ближайший вверх по иерархии контролов элемент управления со значением IsFocusScope = True.
        /// Возвращает null, если такого элемента управления нет.
        /// </summary>
        private Control findClosestScope(Control control)
        {
            Debug.Assert( null != control );
            Control currentParent = control.Parent;
            while (currentParent != null)
            {
                if (currentParent.IsFocusScope)
                    return currentParent;

                currentParent = currentParent.Parent;
            }
            return null;
        }

        /// <summary>
        /// Устанавливает текущую область фокуса scope и передает фокус элементу управления control
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="control"></param>
        public void SetFocus(Control scope, Control control)
        {
            if (scope == null)
                throw new ArgumentNullException("scope");
            if (!scope.IsFocusScope)
                throw new ArgumentException("IsFocusScope property should be true", "scope");

            List<Control> children = getControlsInScope(scope);
            if (children.Count == 0)
            {
                if (tryChangeFocusedElementTo(null))
                    currentScope = scope;
                return;
            }

            Control tofocus;
            if (null != control)
            {
                if (!children.Contains(control))
                    throw new ArgumentException(
                        "Specified control is not a child of scope or is not visible or is not focusable");
                tofocus = control;
            }
            else
            {
                bool reinitFocus = false;
                // Try to restore focus from StoredFocus field
                if ( scope.StoredFocus != null ) {
                    // проверяем, не удалён ли StoredFocus и является ли он Visible & Focusable
                    if ( !VisualTreeHelper.IsConnectedToRoot( scope.StoredFocus ) ) {
                        reinitFocus = true;
                    } else if ( scope.StoredFocus.Visibility != Visibility.Visible ) {
                        reinitFocus = true;
                    } else if ( !scope.StoredFocus.Focusable ) {
                        reinitFocus = true;
                    }
                } else {
                    reinitFocus = true;
                }
                if (reinitFocus)
                    tofocus = children[0];
                else {
                    tofocus = scope.StoredFocus;
                }
            }

            if (tryChangeFocusedElementTo(tofocus))
            {
                currentScope = scope;
            }
        }

        /// <summary>
        /// returns visible and focusable childs of scope ordered by z-index
        /// </summary>
        private List<Control> getControlsInScope(Control scope)
        {
            List<Control> children;
            List<Control> processed = new List<Control>();
            if ( scope.Focusable ) {
                // Добавляем туда же и сам контрол, если он Focusable
                // этот кейс может быть полезен, если у Focusable контрола, который является также и
                // FocusScope, нет дочерних элементов. В этом случае фокус будет предоставлен самому
                // контролу (например, пустое модальное Focusable-окно со специальной отрисовкой)

                // Если же у Focusable & FocusScope контрола есть хотя бы 1 дочерний Focusable-контрол,
                // то он и получит фокус, на сам FocusScope-контрол уже фокуса передано никогда не будет

                children = new List< Control >( );
                children.Add( scope );
            } else {
                children = new List< Control >( scope.Children );
            }
            int i = 0;
            while (i < children.Count)
            {
                Control child = children[i];
                List<Control> nested = new List< Control >(child.Children);
                
                // Using OrderBy instead of List<T>.Sort() because the last one is unstable
                // and can reorder elements with equal keys
                nested = nested.OrderBy(control => control.TabOrder).ToList();
                
                if (nested.Count > 0)
                {
                    children.AddRange(nested);
                    children.RemoveAt(i);
                }
                else
                {
                    i++;
                }
                processed.Add(child);
            }
            List<Control> focusableAndVisible = processed.Where(
                    c => c.Visibility == Visibility.Visible && c.Focusable
                ).ToList();
            return focusableAndVisible;
        }

        public void MoveFocusNext()
        {
            if (null == currentScope)
                throw new InvalidOperationException("Focus scope isn't set");
            if (null == FocusedElement)
            {
                SetFocus(currentScope, null);
                return;
            }

            List<Control> children = getControlsInScope(currentScope);
            if (children.Count == 0)
            {
                return;
            }
            int focusedIndex = children.FindIndex(c => c == FocusedElement);
            if (focusedIndex == -1)
            {
                SetFocus(currentScope, null);
                return;
            }
            else
            {
                Control child = children[(focusedIndex + 1) % children.Count];
                tryChangeFocusedElementTo(child);
            }
        }

        public void MoveFocusPrev()
        {
            if (null == currentScope)
                throw new InvalidOperationException("Focus scope isn't set");
            if (null == FocusedElement)
            {
                SetFocus(currentScope, null);
                return;
            }

            List<Control> children = getControlsInScope(currentScope);
            if (children.Count == 0)
            {
                return;
            }
            int focusedIndex = children.FindIndex(c => c == FocusedElement);
            if (focusedIndex == -1)
            {
                SetFocus(currentScope, null);
                return;
            }
            int index = focusedIndex > 0 ? focusedIndex - 1 : children.Count - 1;
            Control child = children[index];
            tryChangeFocusedElementTo(child);
        }

        /// <summary>
        /// Должен быть вызван перед удалением поддерева элементов.
        /// Если удаляется поддерево элементов, содержащее в себе контрол, который имеет фокус,
        /// то FocusManager сбрасывает FocusedElement в null.
        /// Этот юзкейс не отменяется установкой Handled = true в Preview-событиях.
        /// </summary>
        internal void BeforeRemoveElementFromTree(Control control) {
            if (null == control)
                throw new ArgumentNullException("control");
            if (null != FocusedElement && isFocusedElementInSubtree(control)) {
                tryChangeFocusedElementTo(null, true);
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

    }
}
