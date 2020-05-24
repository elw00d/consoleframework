using System;
using System.ComponentModel;

namespace Binding.Adapters
{
    /// <summary>
    /// Adapter allows use third-party objects (that don't implement INotifyPropertyChange directly)
    /// in data binding scenarios.
    /// </summary>
    public interface IBindingAdapter
    {
        /// <summary>
        /// Returns supported class object.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Returns class object of virtual property that can be used as binding Target.
        /// </summary>
        /// <param name="targetProperty"></param>
        /// <returns></returns>
        Type GetTargetPropertyClazz( String targetProperty );
       
        /// <summary>
        /// Sets value of target property. You should implement this method if you will use
        /// binding in source-to-target flow (BindingMode.OneTime, BindingMode.OneWay, BindingMode.TwoWay).
        /// If you will use BindingMode.OneWayToSource only, it is not necessary to implement this method.
        /// </summary>
        /// <param name="target">Target object</param>
        /// <param name="targetProperty">Property name</param>
        /// <param name="value">Value to be set</param>
        void SetValue( Object target, String targetProperty, Object value );

        /// <summary>
        /// Gets the value of target property. You should implement this method if you will use
        /// binding in target-to-source flow (BindingMode.OneWayToSource, BindingMode.TwoWay).
        /// If you will use BindingMode.OneTime or BindingMode.OneWay, it is not necessary to implement this method.
        /// </summary>
        /// <param name="target">Target object</param>
        /// <param name="targetProperty">Property name</param>
        /// <returns></returns>
        Object GetValue(Object target, String targetProperty);

        /// <summary>
        /// Subscribes to target object property change event. You should implement this method if you will use
        /// binding in target-to-source flow (BindingMode.OneWayToSource, BindingMode.TwoWay).
        /// If you will use BindingMode.OneTime or BindingMode.OneWay, it is not necessary to implement this method.
        /// </summary>
        /// <param name="target">Target object</param>
        /// <param name="listener">Listener to be subscribed</param>
        /// <returns>Listener wrapper object or null if there is no wrapper need</returns>
        Object AddPropertyChangedListener(Object target, PropertyChangedEventHandler listener);

        /// <summary>
        /// Unsubscribes property changed listener from target object. You should implement this method if you will use
        /// binding in target-to-source flow (BindingMode.OneWayToSource, BindingMode.TwoWay).
        /// If you will use BindingMode.OneTime or BindingMode.OneWay, it is not necessary to implement this method.
        /// </summary>
        /// <param name="target">Target object</param>
        /// <param name="listenerWrapper">Listener wrapper to be unsubscribed or null if no wrapper was returned when subscribed</param>
        void RemovePropertyChangedListener(Object target, Object listenerWrapper);

        /// <summary>
        /// Returns default BindingMode for this Target class. This mode will be
        /// used if Binding instance is created without explicit BindingMode specification.
        /// You cannot return BindingMode.Default from this method.
        /// </summary>
        BindingMode DefaultMode { get; }
    }
}