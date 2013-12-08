using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Binding.Adapters;
using Binding.Converters;
using Binding.Observables;
using Binding.Validators;

namespace Binding
{
    /// <summary>
    /// todo : mb rename to OnSourceUpdatedHandler ? and create OnTargetUpdatedHandler too
    /// </summary>
    /// <param name="result"></param>
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
    protected bool targetIsUi;

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

    private IBindingValidator validator;

    // collections synchronization support
    private bool sourceIsObservable;
    private bool targetIsObservable;
    protected SourceListListener sourceListListener;
    protected IObservableList sourceList;
    protected IObservableList targetList;
    protected TargetListListener targetListListener;

    private bool updateSourceIfBindingFails = true;

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
    /// Event will be invoked when data go from target to source.
    /// </summary>
    public event OnBindingHandler OnBinding;
    
    /**
     * Returns validator.
     */
    public IBindingValidator getValidator() {
        return validator;
    }

    /**
     * Sets the validator.
     */
    public void setValidator( IBindingValidator validator ) {
        this.validator = validator;
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
    public void updateTarget() {
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
                    targetListNow = (IList) targetPropertyInfo.GetGetMethod().Invoke(target, null);
                } else {
                    targetListNow = ( IList ) adapter.getValue( target, targetProperty );
                }
                if ( sourceValue == null ) {
                    if (null != targetListNow ) targetListNow.Clear();
                } else {
                    if (null != targetListNow) {
                        targetListNow.Clear();
                        foreach ( Object x in ((IList) sourceValue) ) {
                            targetListNow.Add( x );
                        }

                        // subscribe to source list
                        if (sourceList != null ) {
                            sourceListListener.ban = true;
                            sourceList.removeObservableListListener(sourceListListener);
                            sourceList = null;
                        }
                        sourceList = (IObservableList) sourceValue;
                        sourceListListener = new SourceListListener(this, targetListNow);
                        sourceList.addObservableListListener(sourceListListener);
                    } else {
                        // todo : debug : target list is null, ignoring sync operation
                    }
                }
            } else { // work with usual property
                Object converted = sourceValue;
                // convert back if need
                if (null != converter) {
                    ConversionResult result = converter.convertBack( sourceValue );
                    if (!result.success) {
                        return;
                    }
                    converted = result.value;
                }
                //
                if (adapter == null)
                    targetPropertyInfo.GetSetMethod().Invoke( target, new object[]{converted});
                else
                    adapter.setValue( target, targetProperty, converted );
            }
        } finally {
            ignoreTargetListener = false;
        }
    }

    protected class TargetListListener : IObservableListListener {
        // to avoid side effects from old listeners
        // (can be reproduced if call raisePropertyChanged inside ObservableList handler)
        bool ban = false;
        IList sourceList;
        private BindingBase self;

        public TargetListListener(BindingBase self, IList sourceList) {
            this.self = self;
            this.sourceList = sourceList;
        }

        public void listElementsAdded(IObservableList list, int index, int length) {
            if (ban) return;
            self.ignoreSourceListener = true;
            try {
                for (int i = index; i < list.Count; i++) sourceList.Add(list[i]);
            } finally {
                self.ignoreSourceListener = false;
            }
        }

        public void listElementsRemoved(IObservableList list, int index, IList oldElements) {
            if (ban) return;
            self.ignoreSourceListener = true;
            try {
                foreach (Object item in oldElements)
                    sourceList.Remove(item);
            } finally {
                self.ignoreSourceListener = false;
            }
        }

        public void listElementReplaced(IObservableList list, int index, Object oldElement) {
            if (ban) return;
            self.ignoreSourceListener = true;
            try {
                sourceList[index] = list[index];
            } finally {
                self.ignoreSourceListener = false;
            }
        }
    }

    protected class SourceListListener : IObservableListListener {
        // to avoid side effects from old listeners
        // (can be reproduced if call raisePropertyChanged inside ObservableList handler)
        public bool ban = false;
        IList targetList;
        private BindingBase self;

        public SourceListListener(BindingBase self, IList targetList) {
            this.targetList = targetList;
            this.self = self;
        }

        public void listElementsAdded(IObservableList list, int index, int length) {
            if (ban) return;
            self.ignoreTargetListener = true;
            try {
                for (int i = index; i < list.Count; i++) targetList.Insert(index, list[i]);
            } finally {
                self.ignoreTargetListener = false;
            }
        }

        public void listElementsRemoved(IObservableList list, int index, IList oldElements) {
            if (ban) return;
            self.ignoreTargetListener = true;
            try {
                foreach (Object item in oldElements)
                    targetList.Remove(item);
            } finally {
                self.ignoreTargetListener = false;
            }
        }

        public void listElementReplaced(IObservableList list, int index, Object oldElement) {
            if (ban) return;
            self.ignoreTargetListener = true;
            try {
                targetList[index] = list[index];
            } finally {
                self.ignoreTargetListener = false;
            }
        }
    }

    /**
     * Sends the current binding target value to the binding source property in TwoWay or OneWayToSource bindings.
     */
    public void updateSource() {
        if (realMode != BindingMode.OneWayToSource && realMode != BindingMode.TwoWay)
            throw new ApplicationException( String.Format( "Cannot update source in {0} binding mode.", realMode ) );
        ignoreSourceListener = true;
        try {
            Object targetValue;
            if ( null == adapter )
                targetValue = targetPropertyInfo.GetGetMethod( ).Invoke( target, null );
            else {

                targetValue = adapter.getValue( target, targetProperty );
            }
            //
            if ( targetIsObservable ) { // work with collection
                IList sourceListNow = (IList) sourcePropertyInfo.GetGetMethod().Invoke(source, null);
                if (targetValue == null) {
                    if (null != sourceListNow) sourceListNow.Clear();
                } else {
                    if (null != sourceListNow) {
                        sourceListNow.Clear();
                        foreach ( object item in (IList) targetValue ) {
                            sourceListNow.Add( item );
                        }

                        // subscribe to source list
                        if (targetList != null ) {
                            sourceListListener.ban = true;
                            targetList.removeObservableListListener(sourceListListener);
                            targetList = null;
                        }
                        targetList = (IObservableList) targetValue;
                        targetListListener = new TargetListListener(this, sourceListNow);
                        targetList.addObservableListListener(targetListListener);
                    } else {
                        // todo : debug : source list is null, ignoring sync operation
                    }
                }
            } else { // work with usual property
                Object convertedValue = targetValue;
                // convert if need
                if (null != converter) {
                    ConversionResult result = converter.convert( targetValue );
                    if (!result.success) {
                        if (null != OnBinding)
                            OnBinding.Invoke( new BindingResult( true, false, result.failReason ) );
                        if ( updateSourceIfBindingFails ) {
                            // will update source using null or default(T) if T is primitive
                            sourcePropertyInfo.GetSetMethod().Invoke( source, new object[] {null});
                        }
                        return;
                    }
                    convertedValue = result.value;
                }
                // validate if need
                if (null != validator) {
                    ValidationResult validationResult = validator.validate( convertedValue );
                    if (!validationResult.valid) {
                        if (null != OnBinding)
                            OnBinding.Invoke( new BindingResult( false, true, validationResult.message ) );
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
    public void bind() {
        // Resolve binding mode and search converter if need
        if (targetIsUi) {
            adapter = settings.getAdapterFor(target.GetType());
            realMode = mode == BindingMode.Default ? adapter.getDefaultMode() : mode;
        } else {
            realMode = mode == BindingMode.Default ? BindingMode.TwoWay : mode;
            if (realMode == BindingMode.TwoWay || realMode == BindingMode.OneWayToSource) {
                if (! (target is INotifyPropertyChanged))
                    adapter = settings.getAdapterFor( target.GetType() );
            }
        }

        // Get properties info and check if they are collections
        sourcePropertyInfo = source.GetType( ).GetProperty( sourceProperty );
        if ( null == adapter )
            targetPropertyInfo = target.GetType( ).GetProperty( targetProperty );

        Type targetPropertyClass = (null == adapter) ?
            targetPropertyInfo.PropertyType : adapter.getTargetPropertyClazz(targetProperty);

        sourceIsObservable = typeof(IObservableList).IsAssignableFrom( sourcePropertyInfo.PropertyType );
        targetIsObservable = typeof(IObservableList).IsAssignableFrom( targetPropertyClass );

        // We need converter if data will flow from non-observable property to property of another class
        if (targetPropertyClass != sourcePropertyInfo.PropertyType) {
            bool needConverter = false;
            if (realMode == BindingMode.OneTime || realMode == BindingMode.OneWay || realMode == BindingMode.TwoWay)
                needConverter |= !sourceIsObservable;
            if (realMode == BindingMode.OneWayToSource || realMode == BindingMode.TwoWay)
                needConverter |= !targetIsObservable;
            //
            if (needConverter) {
                converter = settings.getConverterFor( targetPropertyClass, sourcePropertyInfo.PropertyType );
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
        connectSourceAndTarget();

        // initial flush values
        if ( realMode == BindingMode.OneTime || realMode == BindingMode.OneWay || realMode == BindingMode.TwoWay)
            updateTarget();
        if (realMode == BindingMode.OneWayToSource || realMode == BindingMode.TwoWay)
            updateSource();

        this.bound = true;
    }

    protected void connectSourceAndTarget() {
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
                    targetListenerWrapper = adapter.addPropertyChangedListener(target, TargetListener);
                }
                break;
            case BindingMode.TwoWay:
                source.PropertyChanged += SourceListener;
                //
                if (null == adapter) {
                    ((INotifyPropertyChanged)target).PropertyChanged += TargetListener;
                } else {
                    targetListenerWrapper = adapter.addPropertyChangedListener(target, TargetListener);
                }
                break;
        }
    }

    private void TargetListener( object sender, PropertyChangedEventArgs args ) {
        if (!ignoreTargetListener && args.PropertyName == targetProperty)
            updateSource();
    }

    private void SourceListener( object sender, PropertyChangedEventArgs args ) {
        if (!ignoreSourceListener && args.PropertyName == sourceProperty)
            updateTarget();
    }

    /**
     * Disconnects Source and Target objects.
     */
    public void unbind() {
        if (!this.bound) return;

        disconnectSourceAndTarget();

        this.sourcePropertyInfo = null;
        this.targetPropertyInfo = null;

        this.adapter = null;
        this.converter = null;

        this.bound = false;
    }

    protected void disconnectSourceAndTarget() {
        if (realMode == BindingMode.OneWay || realMode == BindingMode.TwoWay) {
            // remove source listener
            source.PropertyChanged -= SourceListener;
        }
        if (realMode == BindingMode.OneWayToSource || realMode == BindingMode.TwoWay) {
            // remove target listener
            if (adapter == null) {
                ((INotifyPropertyChanged)target).PropertyChanged -= TargetListener;
            } else {
                adapter.removePropertyChangedListener( target, targetListenerWrapper );
                targetListenerWrapper = null;
            }
        }

        if (sourceList != null) {
            sourceList.removeObservableListListener(sourceListListener);
            sourceList = null;
        }
        if (targetList != null) {
            targetList.removeObservableListListener(targetListListener);
            targetList = null;
        }
    }

    /**
     * Changes the binding Source object. If current binding state is bound,
     * the {@link #unbind()} and {@link #bind()} methods will be called automatically.
     * @param source New Source object
     */
    public void setSource(INotifyPropertyChanged source) {
        if (null == source) throw new ArgumentNullException( "source" );
        if (bound) {
            unbind();
            this.source = source;
            bind();
        } else {
            this.source = source;
        }
    }

    /**
     * Changes the binding Target object. If current binding state is bound,
     * the {@link #unbind()} and {@link #bind()} methods will be called automatically.
     * @param target New Target object
     */
    public void setTarget(Object target) {
        if (null == target) throw new ArgumentNullException( "target" );
        if (bound) {
            unbind();
            this.target = target;
            bind();
        } else {
            this.target = target;
        }
    }
}

}
