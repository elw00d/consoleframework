using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Контрол, который может состоять из других контролов.
    /// Позиционирует входящие в него контролы в соответствии с внутренним поведением панели и
    /// заданными свойствами дочерних контролов.
    /// Как и все контролы, связан с виртуальным канвасом.
    /// Может быть самым первым контролом программы (окно не может, к примеру, оно может существовать
    /// только в рамках хоста окон).
    /// </summary>
    public class Panel : Control {
        private readonly List<Control> children = new List<Control>();

        public CHAR_ATTRIBUTES Background {
            get;
            set;
        }

        public void AddChild(Control control) {
            children.Add(control);
        }

        public override void Draw(int actualLeft, int actualTop, int actualWidth, int actualHeight) {
            //
        }
    }
}
