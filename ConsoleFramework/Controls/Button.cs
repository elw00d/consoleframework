using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleFramework.Controls
{
    public class Button : Control {
        private string caption;
        public string Caption {
            get {
                return caption;
            }
            set {
                if (caption != value) {
                    caption = value;
                }
            }
        }
    }
}
