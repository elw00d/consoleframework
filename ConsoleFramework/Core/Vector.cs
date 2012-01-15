using System;

namespace ConsoleFramework.Core {
    public struct Vector {

        public override string ToString() {
            return string.Format("{0};{1}", x, y);
        }

        internal int x;
        internal int y;

        public static bool operator ==(Vector vector1, Vector vector2) {
            return ((vector1.X == vector2.X) && (vector1.Y == vector2.Y));
        }

        public static bool operator !=(Vector vector1, Vector vector2) {
            return !(vector1 == vector2);
        }

        public static bool Equals(Vector vector1, Vector vector2) {
            return (vector1.X.Equals(vector2.X) && vector1.Y.Equals(vector2.Y));
        }

        public override bool Equals(object o) {
            if ((o == null) || !(o is Vector)) {
                return false;
            }
            Vector vector = (Vector) o;
            return Equals(this, vector);
        }

        public bool Equals(Vector value) {
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

        public Vector(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public double Length {
            get {
                return Math.Sqrt((this.x*this.x) + (this.y*this.y));
            }
        }

        public double LengthSquared {
            get {
                return ((this.x*this.x) + (this.y*this.y));
            }
        }

        public static double CrossProduct(Vector vector1, Vector vector2) {
            return ((vector1.x*vector2.y) - (vector1.y*vector2.x));
        }

        public static double AngleBetween(Vector vector1, Vector vector2) {
            double y = (vector1.x*vector2.y) - (vector2.x*vector1.y);
            double x = (vector1.x*vector2.x) + (vector1.y*vector2.y);
            return (Math.Atan2(y, x)*57.295779513082323);
        }

        public static Vector operator -(Vector vector) {
            return new Vector(-vector.x, -vector.y);
        }

        public void Negate() {
            this.x = -this.x;
            this.y = -this.y;
        }

        public static Vector operator +(Vector vector1, Vector vector2) {
            return new Vector(vector1.x + vector2.x, vector1.y + vector2.y);
        }

        public static Vector Add(Vector vector1, Vector vector2) {
            return new Vector(vector1.x + vector2.x, vector1.y + vector2.y);
        }

        public static Vector operator -(Vector vector1, Vector vector2) {
            return new Vector(vector1.x - vector2.x, vector1.y - vector2.y);
        }

        public static Vector Subtract(Vector vector1, Vector vector2) {
            return new Vector(vector1.x - vector2.x, vector1.y - vector2.y);
        }

        public static Point operator +(Vector vector, Point point) {
            return new Point(point.x + vector.x, point.y + vector.y);
        }

        public static Point Add(Vector vector, Point point) {
            return new Point(point.x + vector.x, point.y + vector.y);
        }

        public static Vector operator *(Vector vector, int scalar) {
            return new Vector(vector.x*scalar, vector.y*scalar);
        }

        public static Vector Multiply(Vector vector, int scalar) {
            return new Vector(vector.x*scalar, vector.y*scalar);
        }

        public static Vector operator *(int scalar, Vector vector) {
            return new Vector(vector.x*scalar, vector.y*scalar);
        }

        public static Vector Multiply(int scalar, Vector vector) {
            return new Vector(vector.x*scalar, vector.y*scalar);
        }

        public static double operator *(Vector vector1, Vector vector2) {
            return ((vector1.x*vector2.x) + (vector1.y*vector2.y));
        }

        public static double Multiply(Vector vector1, Vector vector2) {
            return ((vector1.x*vector2.x) + (vector1.y*vector2.y));
        }

        public static double Determinant(Vector vector1, Vector vector2) {
            return ((vector1.x*vector2.y) - (vector1.y*vector2.x));
        }

        public static explicit operator Point(Vector vector) {
            return new Point(vector.x, vector.y);
        }
    }
}