using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    public class Panel
    {
        public virtual void Draw() {
            //
        }
    }
}
