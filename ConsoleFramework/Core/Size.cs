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
            if (this.IsEmpty) {
                return 0;
            }
            return (this.Width.GetHashCode() ^ this.Height.GetHashCode());
        }

        public Size(int width, int height) {
            if ((width < 0) || (height < 0)) {
                throw new ArgumentException("Size_WidthAndHeightCannotBeNegative");
            }
            this.width = width;
            this.height = height;
        }

        private static readonly Size max_size  = new Size(int.MaxValue, int.MaxValue);
        public static Size MaxSize {
            get {
                return max_size;
            }
        }

        public static Size Empty {
            get {
                return CreateEmptySize(  );
            }
        }

        public bool IsEmpty {
            get {
                return (this.width <= 0);
            }
        }

        public int Width {
            get {
                return this.width;
            }
            set {
                //if (this.IsEmpty) {
                //    throw new InvalidOperationException("Size_CannotModifyEmptySize");
                //}
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
                //if (this.IsEmpty) {
                //    throw new InvalidOperationException("Size_CannotModifyEmptySize");
                //}
                if (value < 0) {
                    throw new ArgumentException("Size_HeightCannotBeNegative");
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
            Size size = new Size {
                width = 0,
                height = 0
            };
            return size;
        }

        public override string ToString() {
            return string.Format("Size: {0};{1}", Width, Height);
        }
    }
}