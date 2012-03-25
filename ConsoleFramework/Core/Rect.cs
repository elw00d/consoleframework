using System;

namespace ConsoleFramework.Core {

    public struct Rect : IFormattable {
        internal int x;
        internal int y;
        internal int width;
        internal int height;
        private static readonly Rect s_empty;

        public static bool operator ==(Rect rect1, Rect rect2) {
            return ((((rect1.X == rect2.X) && (rect1.Y == rect2.Y)) && (rect1.Width == rect2.Width)) &&
                    (rect1.Height == rect2.Height));
        }

        public static bool operator !=(Rect rect1, Rect rect2) {
            return !(rect1 == rect2);
        }

        public static bool Equals(Rect rect1, Rect rect2) {
            if (rect1.IsEmpty) {
                return rect2.IsEmpty;
            }
            return (((rect1.X.Equals(rect2.X) && rect1.Y.Equals(rect2.Y)) && rect1.Width.Equals(rect2.Width)) &&
                    rect1.Height.Equals(rect2.Height));
        }

        public override bool Equals(object o) {
            if ((o == null) || !(o is Rect)) {
                return false;
            }
            Rect rect = (Rect) o;
            return Equals(this, rect);
        }

        public bool Equals(Rect value) {
            return Equals(this, value);
        }

        public override int GetHashCode() {
            if (this.IsEmpty) {
                return 0;
            }
            return (((this.X.GetHashCode() ^ this.Y.GetHashCode()) ^ this.Width.GetHashCode()) ^
                    this.Height.GetHashCode());
        }

        public override string ToString() {
            return this.ConvertToString(null, null);
        }

        public string ToString(IFormatProvider provider) {
            return this.ConvertToString(null, provider);
        }

        string IFormattable.ToString(string format, IFormatProvider provider) {
            return this.ConvertToString(format, provider);
        }

        internal string ConvertToString(string format, IFormatProvider provider) {
            if (this.IsEmpty) {
                return "Empty";
            }
            const char numericListSeparator = ',';
            return string.Format(provider,
                                 "{1:" + format + "}{0}{2:" + format + "}{0}{3:" + format + "}{0}{4:" + format + "}",
                                 new object[] {
                                     numericListSeparator, this.x, this.y, this.width, this.height
                                 });
        }

        public Rect(Point location, Size size) {
            if (size.IsEmpty) {
                this = s_empty;
            } else {
                this.x = location.x;
                this.y = location.y;
                this.width = size.width;
                this.height = size.height;
            }
        }

        public Rect(int x, int y, int width, int height) {
            if ((width < 0) || (height < 0)) {
                throw new ArgumentException("Size_WidthAndHeightCannotBeNegative");
            }
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public Rect(Point point1, Point point2) {
            this.x = Math.Min(point1.x, point2.x);
            this.y = Math.Min(point1.y, point2.y);
            this.width = Math.Max((Math.Max(point1.x, point2.x) - this.x), 0);
            this.height = Math.Max((Math.Max(point1.y, point2.y) - this.y), 0);
        }

        public Rect(Point point, Vector vector) : this(point, point + vector) {
        }

        public Rect(Size size) {
            if (size.IsEmpty) {
                this = s_empty;
            } else {
                this.x = this.y = 0;
                this.width = size.Width;
                this.height = size.Height;
            }
        }

        public static Rect Empty {
            get {
                return s_empty;
            }
        }

        public bool IsEmpty {
            get {
                return this.width == 0 && this.height == 0;
            }
        }

        public Point Location {
            get {
                return new Point(this.x, this.y);
            }
            set {
                if (this.IsEmpty) {
                    throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
                }
                this.x = value.x;
                this.y = value.y;
            }
        }

        public Size Size {
            get {
                if (this.IsEmpty) {
                    return Size.Empty;
                }
                return new Size(this.width, this.height);
            }
            set {
                if (value.IsEmpty) {
                    this = s_empty;
                } else {
                    if (this.IsEmpty) {
                        throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
                    }
                    this.width = value.width;
                    this.height = value.height;
                }
            }
        }

        public int X {
            get {
                return this.x;
            }
            set {
                if (this.IsEmpty) {
                    throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
                }
                this.x = value;
            }
        }

        public int Y {
            get {
                return this.y;
            }
            set {
                if (this.IsEmpty) {
                    throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
                }
                this.y = value;
            }
        }

        public int Width {
            get {
                return this.width;
            }
            set {
                if (this.IsEmpty) {
                    throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
                }
                if (value < 0) {
                    throw new ArgumentException("Size_WidthCannotBeNegative");
                }
                this.width = value;
            }
        }

        public int Height {
            get {
                return this.height;
            }
            set {
                if (this.IsEmpty) {
                    throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
                }
                if (value < 0) {
                    throw new ArgumentException("Size_HeightCannotBeNegative");
                }
                this.height = value;
            }
        }

        public int Left {
            get {
                return this.x;
            }
        }

        public int Top {
            get {
                return this.y;
            }
        }

        public int Right {
            get {
                if (this.IsEmpty) {
                    return 0;
                }
                return (this.x + this.width);
            }
        }

        public int Bottom {
            get {
                if (this.IsEmpty) {
                    return 0;
                }
                return (this.y + this.height);
            }
        }

        public Point TopLeft {
            get {
                return new Point(this.Left, this.Top);
            }
        }

        public Point TopRight {
            get {
                return new Point(this.Right, this.Top);
            }
        }

        public Point BottomLeft {
            get {
                return new Point(this.Left, this.Bottom);
            }
        }

        public Point BottomRight {
            get {
                return new Point(this.Right, this.Bottom);
            }
        }

        public bool Contains(Point point) {
            return this.Contains(point.x, point.y);
        }

        public bool Contains(int _x, int _y) {
            if (this.IsEmpty) {
                return false;
            }
            return this.ContainsInternal(_x, _y);
        }

        public bool Contains(Rect rect) {
            if (this.IsEmpty || rect.IsEmpty) {
                return false;
            }
            return ((((this.x <= rect.x) && (this.y <= rect.y)) && ((this.x + this.width) >= (rect.x + rect.width))) &&
                    ((this.y + this.height) >= (rect.y + rect.height)));
        }

        public bool IntersectsWith(Rect rect) {
            if (this.IsEmpty || rect.IsEmpty) {
                return false;
            }
            return ((((rect.Left <= this.Right) && (rect.Right >= this.Left)) && (rect.Top <= this.Bottom)) &&
                    (rect.Bottom >= this.Top));
        }

        public void Intersect(Rect rect) {
            if (!this.IntersectsWith(rect)) {
                this = Empty;
            } else {
                int num = Math.Max(this.Left, rect.Left);
                int num2 = Math.Max(this.Top, rect.Top);
                this.width = Math.Max((Math.Min(this.Right, rect.Right) - num), 0);
                this.height = Math.Max((Math.Min(this.Bottom, rect.Bottom) - num2), 0);
                this.x = num;
                this.y = num2;
            }
        }

        public static Rect Intersect(Rect rect1, Rect rect2) {
            rect1.Intersect(rect2);
            return rect1;
        }

        public void Union(Rect rect) {
            if (this.IsEmpty) {
                this = rect;
            } else if (!rect.IsEmpty) {
                int num = Math.Min(this.Left, rect.Left);
                int num2 = Math.Min(this.Top, rect.Top);
                if ((rect.Width == int.MaxValue) || (this.Width == int.MaxValue)) {
                    this.width = int.MaxValue;
                } else {
                    int num3 = Math.Max(this.Right, rect.Right);
                    this.width = Math.Max((num3 - num), 0);
                }
                if ((rect.Height == int.MaxValue) || (this.Height == int.MaxValue)) {
                    this.height = int.MaxValue;
                } else {
                    int num4 = Math.Max(this.Bottom, rect.Bottom);
                    this.height = Math.Max((num4 - num2), 0);
                }
                this.x = num;
                this.y = num2;
            }
        }

        public static Rect Union(Rect rect1, Rect rect2) {
            rect1.Union(rect2);
            return rect1;
        }

        public void Union(Point point) {
            this.Union(new Rect(point, point));
        }

        public static Rect Union(Rect rect, Point point) {
            rect.Union(new Rect(point, point));
            return rect;
        }

        public void Offset(Vector offsetVector) {
            if (this.IsEmpty) {
                throw new InvalidOperationException("Rect_CannotCallMethod");
            }
            this.x += offsetVector.x;
            this.y += offsetVector.y;
        }

        public void Offset(int offsetX, int offsetY) {
            if (this.IsEmpty) {
                throw new InvalidOperationException("Rect_CannotCallMethod");
            }
            this.x += offsetX;
            this.y += offsetY;
        }

        public static Rect Offset(Rect rect, Vector offsetVector) {
            rect.Offset(offsetVector.X, offsetVector.Y);
            return rect;
        }

        public static Rect Offset(Rect rect, int offsetX, int offsetY) {
            rect.Offset(offsetX, offsetY);
            return rect;
        }

        public void Inflate(Size size) {
            this.Inflate(size.width, size.height);
        }

        public void Inflate(int _width, int _height) {
            if (this.IsEmpty) {
                throw new InvalidOperationException("Rect_CannotCallMethod");
            }
            this.x -= _width;
            this.y -= _height;
            this.width += _width;
            this.width += _width;
            this.height += _height;
            this.height += _height;
            if ((this.width < 0) || (this.height < 0)) {
                this = s_empty;
            }
        }

        public static Rect Inflate(Rect rect, Size size) {
            rect.Inflate(size.width, size.height);
            return rect;
        }

        public static Rect Inflate(Rect rect, int width, int height) {
            rect.Inflate(width, height);
            return rect;
        }


        private bool ContainsInternal(int _x, int _y) {
            // исправлено нестрогое условие на строгое
            // чтобы в rect(1;1;1;1) попадал только 1 пиксель (1;1) а не 4 пикселя (1;1)-(2;2)
            return ((((_x >= this.x) && ((_x - this.width) < this.x)) && (_y >= this.y)) && ((_y - this.height) < this.y));
            //return ((((_x >= this.x) && ((_x - this.width) <= this.x)) && (_y >= this.y)) && ((_y - this.height) <= this.y));
        }

        private static Rect CreateEmptyRect() {
            Rect rect = new Rect {
                x = 0,
                y = 0,
                width = 0,
                height = 0
            };
            return rect;
        }

        static Rect() {
            s_empty = CreateEmptyRect();
        }
    }
}
