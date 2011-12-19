namespace ConsoleFramework.Core {
    public struct Point {
        internal int x;
        internal int y;

        public static bool operator ==(Point point1, Point point2) {
            return ((point1.X == point2.X) && (point1.Y == point2.Y));
        }

        public static bool operator !=(Point point1, Point point2) {
            return !(point1 == point2);
        }

        public static bool Equals(Point point1, Point point2) {
            return (point1.X.Equals(point2.X) && point1.Y.Equals(point2.Y));
        }

        public override bool Equals(object o) {
            if ((o == null) || !(o is Point)) {
                return false;
            }
            Point point = (Point) o;
            return Equals(this, point);
        }

        public bool Equals(Point value) {
            return Equals(this, value);
        }

        public override int GetHashCode() {
            return (this.X.GetHashCode() ^ this.Y.GetHashCode());
        }

        public int X {
            get {
                return this.x;
            }
            set {
                this.x = value;
            }
        }

        public int Y {
            get {
                return this.y;
            }
            set {
                this.y = value;
            }
        }

        public Point(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public void Offset(int offsetX, int offsetY) {
            this.x += offsetX;
            this.y += offsetY;
        }

        public static Point operator +(Point point, Vector vector) {
            return new Point(point.x + vector.x, point.y + vector.y);
        }

        public static Point Add(Point point, Vector vector) {
            return new Point(point.x + vector.x, point.y + vector.y);
        }

        public static Point operator -(Point point, Vector vector) {
            return new Point(point.x - vector.x, point.y - vector.y);
        }

        public static Point Subtract(Point point, Vector vector) {
            return new Point(point.x - vector.x, point.y - vector.y);
        }

        public static Vector operator -(Point point1, Point point2) {
            return new Vector(point1.x - point2.x, point1.y - point2.y);
        }

        public static Vector Subtract(Point point1, Point point2) {
            return new Vector(point1.x - point2.x, point1.y - point2.y);
        }

        public static explicit operator Vector(Point point) {
            return new Vector(point.x, point.y);
        }
    }
}