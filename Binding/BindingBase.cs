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

/**
 * Provides data sync connection between two objects - source and target. Both source and target can be just objects,
 * but if you want to bind to object that does not implement {@link INotifyPropertyChanged},
 * you should use it as target and use appropriate adapter ({@link IBindingAdapter} implementation). One Binding instance connects
 * one source property and one target property.
 *
 * @author igor.kostromin
 *         26.06.13 15:57
 */
public class BindingBase {

    protected Object target;
    private readonly String targetProperty;
    protected INotifyPropertyChanged source;
    private readonly String sourceProperty;
    private bool bound;
    private readonly BindingMode mode;
    protected BindingMode realMode;
    private readonly BindingSettingsBase settings;

    // this may be initialized using true in inherited classes for specialized binding
    protected bool needAdapterAnyway = false;

    protected IBindingAdapter adapter;
    private PropertyInfo targetPropertyInfo;
    private PropertyInfo sourcePropertyInfo;

    // converts target to source and back
    private IBindingConverter converter;

    // used instead targetListener if target does not implement INotifyPropertyChanged
    protected Object targetListenerWrapper;

    // flags used to avoid infinite recursive loop
    private bool ignoreSourceListener;
    protected bool ignoreTargetListener;

    // collections synchronization support
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
        if (null == target) throw new ArgumentException( "target is null" );
        if (string.IsNullOrEmpty(targetProperty)) throw new ArgumentException( "targetProperty is null or empty" );
        if (null == source) throw new ArgumentException( "source is null" );
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

    /**
     * Forces a data transfer from the binding source property to the binding target property.
     */
    public void UpdateTarget() {
        if (realMode != BindingMode.OneTime && realMode != BindingMode.OneWay && realMode != BindingMode.TwoWay)
            throw new ApplicationException( String.Format( "Cannot update target in {0} binding mode.", realMode ) );
        ignoreTargetListener = true;
        try {
            Object sourceValue = sourcePropertyInfo.GetGetMethod().Invoke( 
                source, null );
            if ( sourceIsObservable ) { // work with observable list
                // we should take target list and initialize it using source items
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

                        // subscribe
                        if (sourceList != null ) {
                            ((IObservableList) sourceList).ListChanged -= sourceListChanged;
                        }
                        sourceList = (IList) sourceValue;
                        targetList = targetListNow;
                        ((IObservableList) sourceList).ListChanged += sourceListChanged;
                    } else {
                        // todo : debug : target list is null, ignoring sync operation
                    }
                }
            } else { // work with usual property
                Object converted = sourceValue;
                // convert back if need
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

    private void sourceListChanged(object sender, ListChangedEventArgs args) {
        // To avoid side effects from old listeners
        // (can be reproduced if call raisePropertyChanged inside another ObservableList handler)
        // propertyChanged will cause re-subscription to ListChanged, but
        // old list still can call ListChanged when enumerates event handlers
        if (!ReferenceEquals(sender, sourceList)) return;

        ignoreTargetListener = true;
        try {
            switch (args.Type) {
                case ListChangedEventType.ItemsInserted: {
                    for (int i = 0; i < args.Count; i++)
                        targetList.Insert(args.Index + i, sourceList[i]);
                    break;
                }
                case ListChangedEventType.ItemsRemoved: {
                    for (int i = 0; i < args.Count; i++)
                        targetList.RemoveAt(args.Index);
                    break;
                }
                case ListChangedEventType.ItemReplaced: {
                    targetList[args.Index] = sourceList[args.Index];
                    break;
                }
                default:
                    throw new InvalidOperationException();
            }
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
            switch (args.Type) {
                case ListChangedEventType.ItemsInserted: {
                        for (int i = 0; i < args.Count; i++)
                            sourceList.Insert(args.Index + i, targetList[i]);
                        break;
                    }
                case ListChangedEventType.ItemsRemoved: {
                        for (int i = 0; i < args.Count; i++)
                            sourceList.RemoveAt(args.Index);
                        break;
                    }
                case ListChangedEventType.ItemReplaced: {
                        sourceList[args.Index] = targetList[args.Index];
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }
        } finally {
            ignoreSourceListener = false;
        }
    }

    /**
     * Sends the current binding target value to the binding source property in TwoWay or OneWayToSource bindings.
     */
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
            if ( targetIsObservable ) { // work with collection
                IList sourceListNow = ( IList ) sourcePropertyInfo.GetGetMethod().Invoke(source, null);
                if (targetValue == null) {
                    if (null != sourceListNow) sourceListNow.Clear();
                } else {
                    if (null != sourceListNow) {
                        sourceListNow.Clear();
                        foreach ( object item in (IEnumerable) targetValue ) {
                            sourceListNow.Add( item );
                        }

                        // subscribe
                        if (targetList != null ) {
                            ((IObservableList) targetList).ListChanged -= targetListChanged;
                        }
                        targetList = (IList)targetValue;
                        sourceList = sourceListNow;
                        ((IObservableList) targetList).ListChanged += targetListChanged;
                    } else {
                        // todo : debug : source list is null, ignoring sync operation
                    }
                }
            } else { // work with usual property
                Object convertedValue = targetValue;
                // convert if need
                if (null != converter) {
                    ConversionResult result = converter.Convert( targetValue );
                    if (!result.Success) {
                        if (null != OnBinding)
                            OnBinding.Invoke( new BindingResult( true, false, result.FailReason ) );
                        if ( updateSourceIfBindingFails ) {
                            // will update source using null or default(T) if T is primitive
                            sourcePropertyInfo.GetSetMethod().Invoke( source, new object[] {null});
                        }
                        return;
                    }
                    convertedValue = result.Value;
                }
                // validate if need
                if (null != Validator) {
                    ValidationResult validationResult = Validator.Validate( convertedValue );
                    if (!validationResult.Valid) {
                        if (null != OnBinding)
                            OnBinding.Invoke( new BindingResult( false, true, validationResult.Message ) );
                        if ( updateSourceIfBindingFails ) {
                            // will update source using null or default(T) if T is primitive
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

    /**
     * Connects Source and Target objects.
     */
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
                    // check if converter must be reversed
                    if ( converter.FirstType.IsAssignableFrom( targetPropertyClass ) &&
                         converter.SecondType.IsAssignableFrom( sourcePropertyInfo.PropertyType ) ) {
                        // nothing to do, it's ok
                    } else if ( converter.SecondType.IsAssignableFrom( targetPropertyClass ) &&
                                converter.FirstType.IsAssignableFrom( sourcePropertyInfo.PropertyType ) ) {
                        // should be reversed
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

        // subscribe to listeners
        ConnectSourceAndTarget();

        // initial flush values
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

    /**
     * Disconnects Source and Target objects.
     */
    public void Unbind() {
        if (!this.bound) return;

        DisconnectSourceAndTarget();

        this.sourcePropertyInfo = null;
        this.targetPropertyInfo = null;

        this.bound = false;
    }

    protected void DisconnectSourceAndTarget() {
        if (realMode == BindingMode.OneWay || realMode == BindingMode.TwoWay) {
            // remove source listener
            source.PropertyChanged -= SourceListener;
        }
        if (realMode == BindingMode.OneWayToSource || realMode == BindingMode.TwoWay) {
            // remove target listener
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

    /**
     * Changes the binding Source object. If current binding state is bound,
     * the {@link #Unbind()} and {@link #Bind()} methods will be called automatically.
     * @param source New Source object
     */
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

    /**
     * Changes the binding Target object. If current binding state is bound,
     * the {@link #Unbind()} and {@link #Bind()} methods will be called automatically.
     * @param target New Target object
     */
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
