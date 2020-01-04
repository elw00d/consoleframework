using System;

namespace ConsoleFramework.Core {
    public struct Size {
        internal int width;
        internal int height;

        public static bool operator ==(Size size1, Size size2) {
            return ((size1.Width == size2.Width) && (size1.Height == size2.Height));
        }

        public static bool operator !=(Size size1, Size size2) {
            return !(size1 == size2);
        }

        public static bool Equals(Size size1, Size size2) {
            if (size1.IsEmpty) {
                return size2.IsEmpty;
            }
            return (size1.Width.Equals(size2.Width) && size1.Height.Equals(size2.Height));
        }

        public override bool Equals(object o) {
            if ((o == null) || !(o is Size)) {
                return false;
            }
            Size size = (Size) o;
            return Equals(this, size);
        }

        public bool Equals(Size value) {
            return Equals(this, value);
        }

        public override int GetHashCode() {
            if (IsEmpty) {
                return 0;
            }
            return Width.GetHashCode() ^ Height.GetHashCode();
        }

        public Size(int width, int height) {
            if (width < 0 || height < 0) {
                throw new ArgumentException("Width and height cannot be negative");
            }
            this.width = width;
            this.height = height;
        }

        public static Size MaxSize { get; } = new Size(int.MaxValue, int.MaxValue);

        public static Size Empty => CreateEmptySize();

        public bool IsEmpty => width <= 0;

        public int Width {
            get => width;
            set {
                //if (this.IsEmpty) {
                //    throw new InvalidOperationException("Size_CannotModifyEmptySize");
                //}
                if (value < 0) {
                    throw new ArgumentException("Width cannot be negative");
                }
                width = value;
            }
        }

        public int Height {
            get => this.height;
            set {
                //if (this.IsEmpty) {
                //    throw new InvalidOperationException("Size_CannotModifyEmptySize");
                //}
                if (value < 0) {
                    throw new ArgumentException("Height cannot be negative");
                }
                this.height = value;
            }
        }

        public static explicit operator Vector(Size size) {
            return new Vector(size.width, size.height);
        }

        public static explicit operator Point(Size size) {
            return new Point(size.width, size.height);
        }

        private static Size CreateEmptySize() {
            return new Size {
                width = 0,
                height = 0
            };
        }

        public override string ToString() {
            return $"Size: {Width};{Height}";
        }
    }
}