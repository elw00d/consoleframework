using System;

namespace ConsoleFramework.Core
{
    /// <summary>
    /// WPF Thickness analog but using integers not doubles.
    /// </summary>
    public struct Thickness : IEquatable<Thickness> {
        private int left;
        private int top;
        private int right;
        private int bottom;

        public int Left {
            get {
                return left;
            }
            set {
                left = value;
            }
        }

        public int Top {
            get {
                return top;
            }
            set {
                top = value;
            }
        }

        public int Right {
            get {
                return right;
            }
            set {
                right = value;
            }
        }

        public int Bottom {
            get {
                return bottom;
            }
            set {
                bottom = value;
            }
        }

        public Thickness(int uniformLenght) {
            left = top = right = bottom = uniformLenght;
        }

        public Thickness(int left, int top, int right, int bottom) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        internal bool IsZero() {
            return left == 0 && right == 0 && top == 0 && bottom == 0;
        }

        internal bool IsUniform() {
            return left == top && left == right && left == bottom;
        }

        public static bool operator ==(Thickness t1, Thickness t2) {
            return t1.left == t2.left && t1.top == t2.top && t1.right == t2.right && t1.bottom == t2.bottom;
        }

        public static bool operator !=(Thickness t1, Thickness t2) {
            return !(t1 == t2);
        }

        public bool Equals(Thickness other) {
            return other.left == left && other.top == top && other.right == right && other.bottom == bottom;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof (Thickness)) return false;
            return Equals((Thickness) obj);
        }

        public override int GetHashCode() {
            unchecked {
                int result = left;
                result = (result*397) ^ top;
                result = (result*397) ^ right;
                result = (result*397) ^ bottom;
                return result;
            }
        }

        public override string ToString() {
            return string.Format("{0},{1},{2},{3}", left, top, right, bottom);
        }
    }
}
