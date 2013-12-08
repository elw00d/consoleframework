using System;
using System.Collections.Generic;
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

    private Dictionary<Type, Dictionary<Type, IBindingConverter>> converters = new Dictionary<Type, Dictionary<Type, IBindingConverter>>();
    private Dictionary<Type, IBindingAdapter> adapters = new Dictionary<Type, IBindingAdapter>();

    public BindingSettingsBase() {
    }

    /**
     * Adds default set of converters and ui adapters.
     */
    public void initializeDefault() {
        addConverter( new StringToIntegerConverter() );
    }

    public  void addAdapter<T>(IBindingAdapter adapter) {
        Type targetClazz = adapter.getTargetType( );
        if ( adapters.ContainsKey( targetClazz ))
            throw new ApplicationException( String.Format( "Adapter for class {0} is already registered.", targetClazz.Name ) );
        adapters.Add( targetClazz, adapter );
    }

    public IBindingAdapter getAdapterFor(Type clazz) {
        IBindingAdapter adapter = adapters[ clazz ];
        if (null == adapter) throw new ApplicationException(String.Format("Adapter for class {0} not found.", clazz.Name));
        return adapter;
    }

    public void addConverter( IBindingConverter converter) {
        registerConverter( converter );
        registerConverter( new ReversedConverter( converter ) );
    }

    private void registerConverter(IBindingConverter converter) {
        Type first = converter.getFirstClazz( );
        Type second = converter.getSecondClazz( );
        if (converters.ContainsKey( first )) {
            Dictionary< Type, IBindingConverter > firstClassConverters = converters[ first ];
            if (firstClassConverters.ContainsKey( second )) {
                throw new ApplicationException( String.Format( "Converter for {0} -> {1} classes is already registered.", first.Name, second.Name ) );
            }
            firstClassConverters.Add( second, converter );
        } else {
            Dictionary<Type, IBindingConverter> firstClassConverters = new Dictionary<Type, IBindingConverter>();
            firstClassConverters.Add( second, converter );
            converters.Add( first, firstClassConverters );
        }
    }

    public IBindingConverter getConverterFor(Type first, Type second) {
        if (!converters.ContainsKey(first))
            //throw new RuntimeException( String.format( "Converter for %s -> %s classes not found.", first.getName(), second.getName() ) );
            return null;
        Dictionary<Type, IBindingConverter> firstClassConverters = converters[first];
        if (!firstClassConverters.ContainsKey(second))
            //throw new RuntimeException( String.format( "Converter for %s -> %s classes not found.", first.getName(), second.getName() ) );
            return null;
        return firstClassConverters[second];
    }
}
}
