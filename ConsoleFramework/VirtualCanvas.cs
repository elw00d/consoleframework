using System;
using System.Collections.Generic;
using ConsoleFramework.Native;

namespace ConsoleFramework
{
    /// <summary>
    /// Область, доступная для рисования некому framework element'y.
    /// </summary>
    public class VirtualCanvas {
        // Связанный framework element
        private readonly FrameworkElement frameworkElement;
        // То, куда рисуем
        private readonly PhysicalCanvas canvas;
        // То, чем определяем, видно ли нас или нет, если не видно, то не рисуем в canvas
        private readonly IElementsVisibilityAware visibilityProvider;

        private readonly Dictionary<int, NestedIndexer> indexers = new Dictionary<int,NestedIndexer>();

        public VirtualCanvas(FrameworkElement element, PhysicalCanvas canvas, IElementsVisibilityAware visibilityProvider) {
            this.frameworkElement = element;
            this.canvas = canvas;
            this.visibilityProvider = visibilityProvider;
        }

        public NestedIndexer this[int x] {
            get {
                if (indexers.ContainsKey(x)) {
                    return indexers[x];
                }
                NestedIndexer res = new NestedIndexer(x, this);
                indexers[x] = res;
                return res;
            }
        }

        public sealed class NestedIndexer {
            private readonly VirtualCanvas m_this;
            private readonly int x;
            private readonly Dictionary<int, CHAR_INFO_ref> refs = new Dictionary<int,CHAR_INFO_ref>();

            public NestedIndexer(int x, VirtualCanvas mThis) {
                this.x = x;
                this.m_this = mThis;
            }

            public CHAR_INFO_ref this[int y] {
                get {
                    if (refs.ContainsKey(y)) {
                        return refs[y];
                    }
                    CHAR_INFO_ref res = new CHAR_INFO_ref(x, y, m_this);
                    refs[y] = res;
                    return res;
                }
            }

            public sealed class CHAR_INFO_ref {
                private readonly VirtualCanvas m_this;
                private readonly int x;
                private readonly int y;

                public CHAR_INFO_ref(int x, int y, VirtualCanvas mThis) {
                    this.x = x;
                    this.y = y;
                    this.m_this = mThis;
                }

                private bool isPointVisible() {
                    FrameworkElementVisibility visibility = m_this.visibilityProvider.GetElementVisibility(m_this.frameworkElement);
                    if (visibility == FrameworkElementVisibility.FullVisible) {
                        return true;
                    }
                    return visibility != FrameworkElementVisibility.Hidden &&
                           m_this.visibilityProvider.IsPointOfElementVisible(x, y, m_this.frameworkElement);
                }

                public char UnicodeChar {
                    get {
                        return m_this.canvas[x][y].UnicodeChar;
                    }
                    set {
                        if (isPointVisible()) {
                            m_this.canvas[x][y].UnicodeChar = value;
                        }
                    }
                }

                public char AsciiChar {
                    get {
                        return m_this.canvas[x][y].AsciiChar;
                    }
                    set {
                        if (isPointVisible()) {
                            m_this.canvas[x][y].AsciiChar = value;
                        }
                    }
                }

                public CHAR_ATTRIBUTES Attributes {
                    get {
                        return m_this.canvas[x][y].Attributes;
                    }
                    set {
                        if (isPointVisible()) {
                            m_this.canvas[x][y].Attributes = value;
                        }
                    }
                }
            }
        }
    }
}
