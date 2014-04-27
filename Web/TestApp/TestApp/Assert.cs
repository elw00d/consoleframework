using System;

namespace TestApp
{
    public class Assert
    {
        /// <summary>
        /// Overloads for primitives is need due to https://github.com/sq/JSIL/issues/446 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void AreEqual(double a, double b) {
            if (!a.Equals(b))
                throw new InvalidOperationException("Assertion failed.");
        }

        public static void AreEqual(int a, int b) {
            if (!a.Equals(b))
                throw new InvalidOperationException("Assertion failed.");
        }

        public static void AreEqual( object a, object b ) {
            if (!a.Equals( b ))
                throw new InvalidOperationException("Assertion failed.");
        }

        public static void IsTrue( bool assertion ) {
            if (!assertion)
                throw new InvalidOperationException("Assertion failed.");
        }
    }
}
