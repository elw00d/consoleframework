using System;
using System.Collections.Generic;

namespace ConsoleFramework
{
    /// <summary>
    /// Base class for all visible elements.
    /// </summary>
    public abstract class FrameworkElement {

        public int X {
            get;set;
        }

        public int Y {
            get;set;
        }

        public int Width {
            get;set;
        }

        public int Height {
            get;set;
        }

        public FrameworkElement Parent {
            get;
            set;
        }

        public List<FrameworkElement> Childs {
            get;
            set;
        }

        protected virtual void Draw(VirtualCanvas virtualCanvas) {
        }
    }
}
