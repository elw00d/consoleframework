using System;

namespace Binding.Converters
{
    /// <summary>
    /// Provides value conversion logic from first class to second and back.
    /// </summary>
    public interface IBindingConverter
    {
        /// <summary>
        /// Returns class object for TFirst class.
        /// </summary>
        Type FirstType { get; }

        /// <summary>
        /// Returns class object for TSecond class.
        /// </summary>
        Type SecondType { get; }

        /// <summary>
        /// Converts value from TFirst class to TSecond.
        /// </summary>
        ConversionResult Convert(Object first);

        /// <summary>
        /// Converts value from TSecond class to TFirst.
        /// </summary>
        ConversionResult ConvertBack(Object second);
    }

}
