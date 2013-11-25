using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Binding.Converters
{
    /**
 * Converter between String and Integer.
 *
 * @author igor.kostromin
 *         26.06.13 19:37
 */
public class StringToIntegerConverter : IBindingConverter<String, int> {
    public ConversionResult<int> convert(String s) {
        try {
            if (s == null) return new ConversionResult<int>( false, "String is null");
            int value = int.Parse(s);
            return new ConversionResult<int>(value);
        } catch (FormatException e) {
            return new ConversionResult<int>(false, "Incorrect number");
        }
    }

    public ConversionResult<String> convertBack(int integer) {
        return new ConversionResult<String>(integer.ToString(CultureInfo.InvariantCulture) );
    }


}

}
