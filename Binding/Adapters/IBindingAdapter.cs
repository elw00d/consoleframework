using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binding.Adapters
{
/**
 * Adapter allows use thirt-party objects (that don't implement INotifyPropertyChange directly)
 * in data binding scenarios.
 *
 * @author igor.kostromin
 *         26.06.13 17:50
 */

    public interface IBindingAdapter< TTarget >
    {
        /**
     * Returns class object of virtual property that can be used as binding Target.
     */
        Type getTargetPropertyClazz( String targetProperty );

        /**
     * Sets value of target property. You should implement this method if you will use
     * binding in source-to-target flow (BindingMode.OneTime, BindingMode.OneWay, BindingMode.TwoWay).
     * If you will use BindingMode.OneWayToSource only, it is not necessary to implement this method.
     *
     * @param target Target object
     * @param targetProperty Property name
     * @param value Value to be set
     * @param <TValue> Value type argument
     */
        void setValue< TValue >( TTarget target, String targetProperty, TValue value );

        /**
     * Gets the value of target property. You should implement this method if you will use
     * binding in target-to-source flow (BindingMode.OneWayToSource, BindingMode.TwoWay).
     * If you will use BindingMode.OneTime or BindingMode.OneWay, it is not necessary to implement this method.
     *
     * @param target Target object
     * @param targetProperty Property name
     * @param <TValue> Value type argument
     */
        TValue getValue< TValue >( TTarget target, String targetProperty );

        /**
     * Subscribes to target object property change event. You should implement this method if you will use
     * binding in target-to-source flow (BindingMode.OneWayToSource, BindingMode.TwoWay).
     * If you will use BindingMode.OneTime or BindingMode.OneWay, it is not necessary to implement this method.
     *
     * @param target Target object
     * @param listener Listener to be subscribed
     * @return Listener wrapper object or null if there is no wrapper need
     */
        Object addPropertyChangedListener( TTarget target, IPropertyChangedListener listener );

        /**
     * Unsubscribes property changed listener from target object. You should implement this method if you will use
     * binding in target-to-source flow (BindingMode.OneWayToSource, BindingMode.TwoWay).
     * If you will use BindingMode.OneTime or BindingMode.OneWay, it is not necessary to implement this method.
     *
     * @param target Target object
     * @param listenerWrapper Listener wrapper to be unsubscribed or null if no wrapper was returned when subscribed
     */
        void removePropertyChangedListener( TTarget target, Object listenerWrapper );

        /**
     * Returns default BindingMode for this Target class. This mode will be
     * used if Binding instance is created without explicit BindingMode specification.
     * You cannot return BindingMode.Default from this method.
     */
        BindingMode getDefaultMode( );
    }
}