namespace Binding
{
    /// <summary>
    /// Determines how data will flow - from Source to Target,
    /// from Target to Source or both.
    /// </summary>
    public enum BindingMode
    {
        /// <summary>
        /// Data will be synchronized one time in Bind() call from Source to Target.
        /// </summary>
        OneTime,
        /// <summary>
        /// Data is synchronized in two-way mode. When Source property is changed, it will update the Target property,
        /// when Target property is changed, it will update the Source.
        /// </summary>
        TwoWay,
        /// <summary>
        /// Data is synchronized from Source to Target only.
        /// </summary>
        OneWay,
        /// <summary>
        /// Data is synchronized from Target to Source only.
        /// </summary>
        OneWayToSource,
        /// <summary>
        /// Mode is determined by adapter. By default it is TwoWay mode (if no adapter is found).
        /// </summary>
        Default
    }

}
