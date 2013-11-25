using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Binding.Adapters;
using Binding.Converters;

namespace Binding
{
    /**
 * Contains converters, validators and adapters.
 *
 * @author igor.kostromin
 *         26.06.13 16:26
 */
public class BindingSettingsBase {
    public static BindingSettingsBase DEFAULT_SETTINGS ;

    static BindingSettingsBase() {
        DEFAULT_SETTINGS = new BindingSettingsBase();
        DEFAULT_SETTINGS.initializeDefault();
    }

    // Object is IBindingConverter<T1,T2>
    private Dictionary<Type, Dictionary<Type, Object>> converters = new Dictionary<Type, Dictionary<Type, Object>>(  );
    // Object is IBindingAdapter<Target>
    private Dictionary<Type, Object> adapters = new Dictionary<Type, Object>(  );

    public BindingSettingsBase() {
    }

    /**
     * Adds default set of converters and ui adapters.
     */
    public void initializeDefault() {
        addConverter( new StringToIntegerConverter() );
    }

    public  void addAdapter<T>(IBindingAdapter<T> adapter) {
        Type targetClazz = adapter.GetType( ).GetGenericArguments( )[ 0 ];
        if ( adapters.ContainsKey( targetClazz ))
            throw new ApplicationException( String.Format( "Adapter for class {0} is already registered.", targetClazz.Name ) );
        adapters.Add( targetClazz, adapter );
    }

    public Object getAdapterFor<T>(Type clazz) {
        Object adapter = adapters[ clazz ];
        if (null == adapter) throw new ApplicationException(String.Format("Adapter for class {0} not found.", clazz.Name));
        return adapter;
    }

    public void addConverter<TFirst, TSecond>( IBindingConverter<TFirst, TSecond> converter) {
        registerConverter( converter );
        registerConverter( new ReversedConverter<TSecond, TFirst>( converter ) );
    }

    private void registerConverter<TFirst, TSecond>(IBindingConverter<TFirst, TSecond> converter) {
        Type first = converter.GetType( ).GetGenericArguments( )[ 0 ];
        Type second = converter.GetType( ).GetGenericArguments( )[ 1 ];
        if (converters.ContainsKey( first )) {
            Dictionary< Type, Object > firstClassConverters = converters[ first ];
            if (firstClassConverters.ContainsKey( second )) {
                throw new ApplicationException( String.Format( "Converter for {0} -> {1} classes is already registered.", first.Name, second.Name ) );
            }
            firstClassConverters.Add( second, converter );
        } else {
            Dictionary<Type, Object> firstClassConverters = new Dictionary<Type, Object>(  );
            firstClassConverters.Add( second, converter );
            converters.Add( first, firstClassConverters );
        }
    }

    public IBindingConverter<TFirst, TSecond> getConverterFor<TFirst, TSecond>() {
        Type first = typeof ( TFirst );
        Type second = typeof ( TSecond );
        if (!converters.ContainsKey( first ))
            //throw new RuntimeException( String.format( "Converter for %s -> %s classes not found.", first.getName(), second.getName() ) );
            return null;
        Dictionary<Type, Object> firstClassConverters = converters[first ];
        if (!firstClassConverters.ContainsKey( second ))
            //throw new RuntimeException( String.format( "Converter for %s -> %s classes not found.", first.getName(), second.getName() ) );
            return null;
        return ( IBindingConverter< TFirst, TSecond > ) firstClassConverters[ second ];
    }

    private class ReversedConverter<TFirst, TSecond> : IBindingConverter<TFirst, TSecond> {

        IBindingConverter<TSecond, TFirst> converter;

        public ReversedConverter(IBindingConverter<TSecond, TFirst> converter) {
            this.converter = converter;
        }

        public ConversionResult<TSecond> convert(TFirst tFirst) {
            return converter.convertBack(tFirst);
        }

        public ConversionResult<TFirst> convertBack(TSecond tSecond) {
            return converter.convert(tSecond);
        }
    }
}

}
