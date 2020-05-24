using System;

namespace Xaml
{
    /// <summary>
    /// Returns an object that is referenced in expression. Example:
    /// {Ref myObject} will return object with x:Id="myObject".
    /// Forward-references are supported too.
    /// </summary>
    [MarkupExtension("Ref")]
    class RefMarkupExtension : IMarkupExtension
    {
        public RefMarkupExtension( ) {
        }

        public RefMarkupExtension( string @ref ) {
            Ref = @ref;
        }

        /// <summary>
        /// String reference to ID of object to be used.
        /// </summary>
        public String Ref { get; set; }

        public object ProvideValue( IMarkupExtensionContext context ) {
            if (string.IsNullOrEmpty( Ref ))
                throw new InvalidOperationException("Ref is null or empty string.");
            
            object obj = context.GetObjectById( Ref );
            if ( null == obj ) {
                if ( context.IsFixupTokenAvailable ) {
                    return context.GetFixupToken( new string[ ] { Ref } );
                } else {
                    throw new InvalidOperationException(string.Format("Object with Id={0} not found.", Ref));
                }
            }

            return obj;
        }
    }
}
