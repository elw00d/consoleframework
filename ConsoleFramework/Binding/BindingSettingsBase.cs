using System;
using System.Collections.Generic;
using Binding.Adapters;
using Binding.Converters;

namespace Binding
{
    /// <summary>
    /// Contains converters, validators and adapters.
    /// </summary>
    public class BindingSettingsBase {
        public static BindingSettingsBase DEFAULT_SETTINGS ;

        static BindingSettingsBase() {
            DEFAULT_SETTINGS = new BindingSettingsBase();
            DEFAULT_SETTINGS.InitializeDefault();
        }

        private readonly Dictionary<Type, Dictionary<Type, IBindingConverter>> converters = new Dictionary<Type, Dictionary<Type, IBindingConverter>>();
        private readonly Dictionary<Type, IBindingAdapter> adapters = new Dictionary<Type, IBindingAdapter>();

        public BindingSettingsBase() {
        }

        /// <summary>
        /// Adds default set of converters and ui adapters.
        /// </summary>
        public void InitializeDefault() {
            AddConverter( new StringToIntegerConverter() );
        }

        public void AddAdapter(IBindingAdapter adapter) {
            Type targetClazz = adapter.TargetType;
            if ( adapters.ContainsKey( targetClazz ))
                throw new Exception( String.Format( "Adapter for class {0} is already registered.", targetClazz.Name ) );
            adapters.Add( targetClazz, adapter );
        }

        public IBindingAdapter GetAdapterFor(Type clazz) {
            IBindingAdapter adapter = adapters[ clazz ];
            if (null == adapter) throw new Exception(String.Format("Adapter for class {0} not found.", clazz.Name));
            return adapter;
        }

        public void AddConverter( IBindingConverter converter) {
            RegisterConverter( converter );
            RegisterConverter( new ReversedConverter( converter ) );
        }

        private void RegisterConverter(IBindingConverter converter) {
            Type first = converter.FirstType;
            Type second = converter.SecondType;
            if (converters.ContainsKey( first )) {
                Dictionary< Type, IBindingConverter > firstClassConverters = converters[ first ];
                if (firstClassConverters.ContainsKey( second )) {
                    throw new Exception( String.Format( "Converter for {0} -> {1} classes is already registered.", first.Name, second.Name ) );
                }
                firstClassConverters.Add( second, converter );
            } else {
                Dictionary<Type, IBindingConverter> firstClassConverters = new Dictionary<Type, IBindingConverter>();
                firstClassConverters.Add( second, converter );
                converters.Add( first, firstClassConverters );
            }
        }

        public IBindingConverter GetConverterFor(Type first, Type second) {
            if (!converters.ContainsKey(first))
                return null;
            Dictionary<Type, IBindingConverter> firstClassConverters = converters[first];
            if (!firstClassConverters.ContainsKey(second))
                return null;
            return firstClassConverters[second];
        }
    }
}
