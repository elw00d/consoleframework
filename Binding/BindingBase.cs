using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Binding.Adapters;
using Binding.Converters;
using Binding.Observables;
using Binding.Validators;
using ListChangedEventArgs = Binding.Observables.ListChangedEventArgs;

namespace Binding
{
    /// <summary>
    /// Handler of binding operation when data is transferred from Target to Source.
    /// </summary>
    public delegate void OnBindingHandler(BindingResult result);

    /// <summary>
    /// Provides data sync connection between two objects - source and target. Both source and target can be just objects,
    /// but if you want to bind to object that does not implement <see cref="INotifyPropertyChanged"/>,
    /// you should use it as target and use appropriate adapter (<see cref="IBindingAdapter"/> implementation). One Binding instance connects
    /// one source property and one target property.
    /// </summary>
    public class BindingBase {

        protected Object target;
        private readonly String targetProperty;
        protected INotifyPropertyChanged source;
        private readonly String sourceProperty;
        private bool bound;
        private readonly BindingMode mode;
        protected BindingMode realMode;
        private readonly BindingSettingsBase settings;

        // This may be initialized using true in inherited classes for specialized binding
        protected bool needAdapterAnyway = false;

        protected IBindingAdapter adapter;
        private PropertyInfo targetPropertyInfo;
        private PropertyInfo sourcePropertyInfo;

        // Converts target to source and back
        private IBindingConverter converter;

        // Used instead targetListener if target does not implement INotifyPropertyChanged
        protected Object targetListenerWrapper;

        // Flags used to avoid infinite recursive loop
        private bool ignoreSourceListener;
        protected bool ignoreTargetListener;

        // Collections synchronization support
        private bool sourceIsObservable;
        private IList targetList;

        private bool targetIsObservable;
        private IList sourceList;

        private bool updateSourceIfBindingFails = true;
        private IBindingValidator validator;

        /// <summary>
        /// If target value conversion or validation fails, the source property will be set to null
        /// if this flag is set to true. Otherwise the source property setter won't be called.
        /// Default value is true
        /// </summary>
        public bool UpdateSourceIfBindingFails {
            get { return updateSourceIfBindingFails; }
            set { updateSourceIfBindingFails = value; }
        }

        /// <summary>
        /// Event will be invoked when data goes from Target to Source.
        /// </summary>
        public event OnBindingHandler OnBinding;

        /// <summary>
        /// Validator triggered when data flows from Target to Source.
        /// </summary>
        public IBindingValidator Validator {
            get { return validator; }
            set {
                if (bound) throw new InvalidOperationException("Cannot change validator when binding is active.");
                validator = value;
            }
        }

        /// <summary>
        /// BindingAdapter used as bridge to Target if Target doesn't
        /// implement INotifyPropertyChanged.
        /// </summary>
        public IBindingAdapter Adapter {
            get {return adapter;}
            set {
                if (bound) throw new InvalidOperationException("Cannot change adapter when binding is active.");
                adapter = value;
            }
        }

        /// <summary>
        /// Converter used for values conversion between Source and Target if
        /// declared properties types are different.
        /// </summary>
        public IBindingConverter Converter {
            get {return converter;}
            set {
                if (bound) throw new InvalidOperationException("Cannot change converter when binding is active.");
                converter = value;
            }
        }

        public BindingBase( Object target, String targetProperty, INotifyPropertyChanged source, String sourceProperty ):
            this(target, targetProperty, source, sourceProperty, BindingMode.Default ) {
        }

        public BindingBase( Object target, String targetProperty, INotifyPropertyChanged source,
                            String sourceProperty, BindingMode mode ):
            this(target, targetProperty, source, sourceProperty, mode, BindingSettingsBase.DEFAULT_SETTINGS) {
        }

        public BindingBase( Object target, String targetProperty, INotifyPropertyChanged source,
                            String sourceProperty, BindingMode mode, BindingSettingsBase settings ) {
            if (null == target) throw new ArgumentNullException( "target" );
            if (string.IsNullOrEmpty(targetProperty)) throw new ArgumentException( "targetProperty is null or empty" );
            if (null == source) throw new ArgumentNullException( "source" );
            if (string.IsNullOrEmpty( sourceProperty )) throw new ArgumentException( "sourceProperty is null or empty" );
            //
            this.target = target;
            this.targetProperty = targetProperty;
            this.source = source;
            this.sourceProperty = sourceProperty;
            this.mode = mode;
            this.bound = false;
            this.settings = settings;
        }

        /// <summary>
        /// Forces a data transfer from the binding source property to the binding target property.
        /// </summary>
        public void UpdateTarget() {
            if (realMode != BindingMode.OneTime && realMode != BindingMode.OneWay && realMode != BindingMode.TwoWay)
                throw new ApplicationException( String.Format( "Cannot update target in {0} binding mode.", realMode ) );
            ignoreTargetListener = true;
            try {
                Object sourceValue = sourcePropertyInfo.GetGetMethod().Invoke( 
                    source, null );
                if ( sourceIsObservable ) { // work with observable list
                    // We should take target list and initialize it using source items
                    IList targetListNow;
                    if (adapter == null) {
                        targetListNow = ( IList ) targetPropertyInfo.GetGetMethod().Invoke(target, null);
                    } else {
                        targetListNow = ( IList ) adapter.GetValue(target, targetProperty);
                    }
                    if ( sourceValue == null ) {
                        if (null != targetListNow ) targetListNow.Clear();
                    } else {
                        if (null != targetListNow) {
                            targetListNow.Clear();
                            foreach ( Object x in ((IEnumerable) sourceValue) ) {
                                targetListNow.Add( x );
                            }

                            // Subscribe
                            if (sourceList != null ) {
                                ((IObservableList) sourceList).ListChanged -= sourceListChanged;
                            }
                            sourceList = (IList) sourceValue;
                            targetList = targetListNow;
                            ((IObservableList) sourceList).ListChanged += sourceListChanged;
                        } else {
                            // Nothing to do : target list is null, ignoring sync operation
                        }
                    }
                } else { // Work with usual property
                    Object converted = sourceValue;
                    // Convert back if need
                    if (null != converter) {
                        ConversionResult result = converter.ConvertBack( sourceValue );
                        if (!result.Success) {
                            return;
                        }
                        converted = result.Value;
                    }
                    //
                    if (adapter == null)
                        targetPropertyInfo.GetSetMethod().Invoke( target, new object[]{converted});
                    else
                        adapter.SetValue( target, targetProperty, converted );
                }
            } finally {
                ignoreTargetListener = false;
            }
        }

        /// <summary>
        /// Synchronizes changes of srcList, applying them to destList.
        /// Changes are described in args.
        /// </summary>
        public static void ApplyChanges(IList destList, IList srcList, ListChangedEventArgs args) {
            switch (args.Type) {
                case ListChangedEventType.ItemsInserted: {
                    for (int i = 0; i < args.Count; i++) {
                        destList.Insert(args.Index + i, srcList[args.Index + i]);
                    }
                    break;
                }
                case ListChangedEventType.ItemsRemoved:
                    for (int i = 0; i < args.Count; i++)
                        destList.RemoveAt(args.Index);
                    break;
                case ListChangedEventType.ItemReplaced: {
                    destList[args.Index] = srcList[args.Index];
                    break;
                }
            }
        }

        private void sourceListChanged(object sender, ListChangedEventArgs args) {
            // To avoid side effects from old listeners
            // (can be reproduced if call raisePropertyChanged inside another ObservableList handler)
            // propertyChanged will cause re-subscription to ListChanged, but
            // old list still can call ListChanged when enumerates event handlers
            if (!ReferenceEquals(sender, sourceList)) return;

            ignoreTargetListener = true;
            try {
                ApplyChanges(targetList, sourceList, args);
            } finally {
                ignoreTargetListener = false;
            }
        }

        private void targetListChanged(object sender, ListChangedEventArgs args) {
            // To avoid side effects from old listeners
            // (can be reproduced if call raisePropertyChanged inside another ObservableList handler)
            // propertyChanged will cause re-subscription to ListChanged, but
            // old list still can call ListChanged when enumerates event handlers
            if (!ReferenceEquals(sender, targetList)) return;

            ignoreSourceListener = true;
            try {
                ApplyChanges(sourceList, targetList, args);
            } finally {
                ignoreSourceListener = false;
            }
        }

        /// <summary>
        /// Sends the current binding target value to the binding source property in TwoWay or OneWayToSource bindings.
        /// </summary>
        public void UpdateSource() {
            if (realMode != BindingMode.OneWayToSource && realMode != BindingMode.TwoWay)
                throw new ApplicationException( String.Format( "Cannot update source in {0} binding mode.", realMode ) );
            ignoreSourceListener = true;
            try {
                Object targetValue;
                if ( null == adapter )
                    targetValue = targetPropertyInfo.GetGetMethod( ).Invoke( target, null );
                else {
                    targetValue = adapter.GetValue( target, targetProperty );
                }
                //
                if ( targetIsObservable ) { // Work with collection
                    IList sourceListNow = ( IList ) sourcePropertyInfo.GetGetMethod().Invoke(source, null);
                    if (targetValue == null) {
                        if (null != sourceListNow) sourceListNow.Clear();
                    } else {
                        if (null != sourceListNow) {
                            sourceListNow.Clear();
                            foreach ( object item in (IEnumerable) targetValue ) {
                                sourceListNow.Add( item );
                            }

                            // Subscribe
                            if (targetList != null ) {
                                ((IObservableList) targetList).ListChanged -= targetListChanged;
                            }
                            targetList = (IList)targetValue;
                            sourceList = sourceListNow;
                            ((IObservableList) targetList).ListChanged += targetListChanged;
                        } else {
                            // Nothing to do : source list is null, ignoring sync operation
                        }
                    }
                } else { // Work with usual property
                    Object convertedValue = targetValue;
                    // Convert if need
                    if (null != converter) {
                        ConversionResult result = converter.Convert( targetValue );
                        if (!result.Success) {
                            if (null != OnBinding)
                                OnBinding.Invoke( new BindingResult( true, false, result.FailReason ) );
                            if ( updateSourceIfBindingFails ) {
                                // Will update source using null or default(T) if T is primitive
                                sourcePropertyInfo.GetSetMethod().Invoke( source, new object[] {null});
                            }
                            return;
                        }
                        convertedValue = result.Value;
                    }
                    // Validate if need
                    if (null != Validator) {
                        ValidationResult validationResult = Validator.Validate( convertedValue );
                        if (!validationResult.Valid) {
                            if (null != OnBinding)
                                OnBinding.Invoke( new BindingResult( false, true, validationResult.Message ) );
                            if ( updateSourceIfBindingFails ) {
                                // Will update source using null or default(T) if T is primitive
                                sourcePropertyInfo.GetSetMethod().Invoke( source, new object[]{ null});
                            }
                            return;
                        }
                    }
                    sourcePropertyInfo.GetSetMethod().Invoke( source, new object[] {convertedValue} );
                    if (null != OnBinding)
                        OnBinding.Invoke(new BindingResult(false));
                    //
                }
            } finally {
                ignoreSourceListener =false;
            }
        }

        /// <summary>
        /// Connects Source and Target objects.
        /// </summary>
        public void Bind() {
            // Resolve binding mode and search converter if need
            if (needAdapterAnyway) {
                if (adapter == null)
                    adapter = settings.GetAdapterFor(target.GetType());
                realMode = mode == BindingMode.Default ? adapter.DefaultMode : mode;
            } else {
                realMode = mode == BindingMode.Default ? BindingMode.TwoWay : mode;
                if (realMode == BindingMode.TwoWay || realMode == BindingMode.OneWayToSource) {
                    if (! (target is INotifyPropertyChanged))
                        if (adapter == null)
                            adapter = settings.GetAdapterFor( target.GetType() );
                }
            }

            // Get properties info and check if they are collections
            sourcePropertyInfo = source.GetType( ).GetProperty( sourceProperty );
            if ( null == adapter )
                targetPropertyInfo = target.GetType( ).GetProperty( targetProperty );

            Type targetPropertyClass = (null == adapter) ?
                targetPropertyInfo.PropertyType : adapter.GetTargetPropertyClazz(targetProperty);

            sourceIsObservable = typeof(IObservableList).IsAssignableFrom( sourcePropertyInfo.PropertyType );
            targetIsObservable = typeof(IObservableList).IsAssignableFrom(targetPropertyClass);

            // We need converter if data will flow from non-observable property to property of another class
            if (targetPropertyClass != sourcePropertyInfo.PropertyType) {
                bool needConverter = false;
                if (realMode == BindingMode.OneTime || realMode == BindingMode.OneWay || realMode == BindingMode.TwoWay)
                    needConverter |= !sourceIsObservable;
                if (realMode == BindingMode.OneWayToSource || realMode == BindingMode.TwoWay)
                    needConverter |= !targetIsObservable;
                //
                if (needConverter) {
                    if ( converter == null )
                        converter = settings.GetConverterFor( targetPropertyClass, sourcePropertyInfo.PropertyType );
                    else {
                        // Check if converter must be reversed
                        if ( converter.FirstType.IsAssignableFrom( targetPropertyClass ) &&
                             converter.SecondType.IsAssignableFrom( sourcePropertyInfo.PropertyType ) ) {
                            // Nothing to do, it's ok
                        } else if ( converter.SecondType.IsAssignableFrom( targetPropertyClass ) &&
                                    converter.FirstType.IsAssignableFrom( sourcePropertyInfo.PropertyType ) ) {
                            // Should be reversed
                            converter = new ReversedConverter( converter );
                        } else {
                            throw new Exception("Provided converter doesn't support conversion between " +
                                                "specified properties.");
                        }
                    }
                    if (converter == null )
                        throw new Exception( String.Format("Converter for {0} -> {1} classes not found.",
                                targetPropertyClass.Name, sourcePropertyInfo.PropertyType.Name) );
                }
            }

            // Verify properties getters and setters for specified binding mode
            if (realMode == BindingMode.OneTime || realMode == BindingMode.OneWay || realMode == BindingMode.TwoWay) {
                if (sourcePropertyInfo.GetGetMethod() == null) throw new Exception( "Source property getter not found" );
                if (sourceIsObservable) {
                    if (null == adapter && targetPropertyInfo.GetGetMethod() == null) throw new Exception( "Target property getter not found" );
                    if (!typeof(IList).IsAssignableFrom( targetPropertyClass ))
                        throw new Exception( "Target property class have to implement IList" );
                } else {
                    if (null == adapter && targetPropertyInfo.GetSetMethod() == null)
                        throw new Exception( "Target property setter not found" );
                }
            }
            if (realMode == BindingMode.OneWayToSource || realMode == BindingMode.TwoWay) {
                if ( null == adapter && targetPropertyInfo.GetGetMethod() == null)
                    throw new Exception( "Target property getter not found" );
                if ( targetIsObservable) {
                    if (sourcePropertyInfo.GetGetMethod() == null) throw new Exception( "Source property getter not found" );
                    if (!typeof(IList).IsAssignableFrom( sourcePropertyInfo.PropertyType ))
                        throw new Exception( "Source property class have to implement IList" );
                } else {
                    if (sourcePropertyInfo.GetSetMethod() == null ) throw new Exception( "Source property setter not found" );
                }
            }

            // Subscribe to listeners
            ConnectSourceAndTarget();

            // Initial flush values
            if ( realMode == BindingMode.OneTime || realMode == BindingMode.OneWay || realMode == BindingMode.TwoWay)
                UpdateTarget();
            if (realMode == BindingMode.OneWayToSource || realMode == BindingMode.TwoWay)
                UpdateSource();

            this.bound = true;
        }

        protected void ConnectSourceAndTarget() {
            switch ( realMode ) {
                case BindingMode.OneTime:
                    break;
                case BindingMode.OneWay:
                    source.PropertyChanged += SourceListener;
                    break;
                case BindingMode.OneWayToSource:
                    if (null == adapter) {
                        ((INotifyPropertyChanged)target).PropertyChanged += TargetListener;
                    } else {
                        targetListenerWrapper = adapter.AddPropertyChangedListener(target, TargetListener);
                    }
                    break;
                case BindingMode.TwoWay:
                    source.PropertyChanged += SourceListener;
                    //
                    if (null == adapter) {
                        ((INotifyPropertyChanged)target).PropertyChanged += TargetListener;
                    } else {
                        targetListenerWrapper = adapter.AddPropertyChangedListener(target, TargetListener);
                    }
                    break;
            }
        }

        private void TargetListener( object sender, PropertyChangedEventArgs args ) {
            if (!ignoreTargetListener && args.PropertyName == targetProperty)
                UpdateSource();
        }

        private void SourceListener( object sender, PropertyChangedEventArgs args ) {
            if (!ignoreSourceListener && args.PropertyName == sourceProperty)
                UpdateTarget();
        }

        /// <summary>
        /// Disconnects Source and Target objects.
        /// </summary>
        public void Unbind() {
            if (!this.bound) return;

            DisconnectSourceAndTarget();

            this.sourcePropertyInfo = null;
            this.targetPropertyInfo = null;

            this.bound = false;
        }

        protected void DisconnectSourceAndTarget() {
            if (realMode == BindingMode.OneWay || realMode == BindingMode.TwoWay) {
                // Remove source listener
                source.PropertyChanged -= SourceListener;
            }
            if (realMode == BindingMode.OneWayToSource || realMode == BindingMode.TwoWay) {
                // Remove target listener
                if (adapter == null) {
                    ((INotifyPropertyChanged)target).PropertyChanged -= TargetListener;
                } else {
                    adapter.RemovePropertyChangedListener( target, targetListenerWrapper );
                    targetListenerWrapper = null;
                }
            }

            if (sourceList != null && sourceIsObservable) {
                ((IObservableList)sourceList).ListChanged -= sourceListChanged;
                sourceList = null;
            }
            if (targetList != null && targetIsObservable) {
                ((IObservableList)targetList).ListChanged -= targetListChanged;
                targetList = null;
            }
        }

        /// <summary>
        /// Changes the binding Source object. If current binding state is bound,
        /// the <see cref="Unbind"/> and <see cref="Bind"/> methods will be called automatically.
        /// <param name="source">New Source object</param>
        /// </summary>
        public void SetSource(INotifyPropertyChanged source) {
            if (null == source) throw new ArgumentNullException( "source" );
            if (bound) {
                Unbind();
                this.source = source;
                Bind();
            } else {
                this.source = source;
            }
        }

        /// <summary>
        /// Changes the binding Target object. If current binding state is bound,
        /// the <see cref="Unbind"/> and <see cref="Bind"/> methods will be called automatically.
        /// @param target New Target object
        /// </summary>
        public void SetTarget(Object target) {
            if (null == target) throw new ArgumentNullException( "target" );
            if (bound) {
                Unbind();
                this.target = target;
                Bind();
            } else {
                this.target = target;
            }
        }
    }

}
