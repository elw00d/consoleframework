using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Xaml
{
    /// <summary>
    /// Marker of fixup token - object can be returned from
    /// markup extension context. Token allows to resolve forward references in XAML.
    /// </summary>
    public interface IFixupToken
    {
    }

    /// <summary>
    /// Контекст, доступный расширению разметки.
    /// </summary>
    public interface IMarkupExtensionContext
    {
        /// <summary>
        /// Имя свойства, которое определяется при помощи расширения разметки.
        /// </summary>
        String PropertyName { get; }

        /// <summary>
        /// Ссылка на конфигурируемый объект.
        /// </summary>
        Object Object { get; }

        /// <summary>
        /// Возвращает активный для конфигурируемого объекта DataContext.
        /// Если у текущего конфигурируемого объекта нет собственного DataContext'a,
        /// будет взят контекст объекта выше по иерархии контролов, и так до главного элемента
        /// дерева контролов.
        /// </summary>
        Object DataContext { get; }

        /// <summary>
        /// Returns already created object with specified x:Id attribute value or null if object with
        /// this x:Id is not constructed yet. To resolve forward references use fixup tokens mechanism.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Object GetObjectById( String id );

        /// <summary>
        /// Gets a value that determines whether calling GetFixupToken is available
        /// in order to resolve a name into a token for forward resolution.
        /// </summary>
        bool IsFixupTokenAvailable { get; }

        /// <summary>
        /// Returns an object that can correct for certain markup patterns that produce forward references.
        /// </summary>
        /// <param name="ids">A collection of ids that are possible forward references.</param>
        /// <returns>An object that provides a token for lookup behavior to be evaluated later.</returns>
        IFixupToken GetFixupToken(IEnumerable<String> ids);
    }

    /// <summary>
    /// todo : comment
    /// </summary>
    public interface IMarkupExtension
    {
        /// <summary>
        /// If ProvideValue returns null, it will not be assigned to object property.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Object ProvideValue(IMarkupExtensionContext context);
    }

    public interface IMarkupExtensionsResolver
    {
        Type Resolve( String name );
    }

    public class MarkupExtensionsParser
    {
        private readonly IMarkupExtensionsResolver resolver;

        public MarkupExtensionsParser( IMarkupExtensionsResolver resolver, String text ) {
            this.resolver = resolver;
            this.text = text;
        }

        private String text;
        private int index;

        private bool hasNextChar( ) {
            return index < text.Length;
        }

        private char consumeChar( ) {
            return text[ index++ ];
        }

        private char peekNextChar( ) {
            return text[ index ];
        }

        public Object ProcessMarkupExtension( IMarkupExtensionContext context ) {
            // interpret as markup extension expression
            object result = processMarkupExtensionCore(context);
            if ( result is IFixupToken ) return result;

            if ( hasNextChar( ) ) {
                throw new InvalidOperationException(
                    String.Format("Syntax error: unexpected characters at {0}", index));
            }

            return result;
        }

        /// <summary>
        /// Consumes all whitespace characters. If necessary is true, at least one
        /// whitespace character should be consumed.
        /// </summary>
        private void processWhitespace(bool necessary = true) {
            if ( necessary ) {
                // at least one whitespace should be
                if (peekNextChar( ) != ' ') 
                    throw new InvalidOperationException(
                        String.Format("Syntax error: whitespace expected at {0}.", index));
            }
            while ( peekNextChar( ) == ' ' ) consumeChar( );
        }
        
        /// <summary>
        /// Recursive method. Consumes next characters as markup extension definition.
        /// Resolves type, ctor arguments and properties of markup extension,
        /// constructs and initializes it, and returns ProvideValue method result.
        /// </summary>
        /// <param name="context">Context object passed to ProvideValue method.</param>
        private Object processMarkupExtensionCore( IMarkupExtensionContext context) {
            if (consumeChar( ) != '{')
                throw new InvalidOperationException("Syntax error: '{{' token expected at 0.");
            processWhitespace( false );
            String markupExtensionName = processQualifiedName( );
            if ( markupExtensionName.Length == 0 )
                throw new InvalidOperationException( "Syntax error: markup extension name is empty." );
            processWhitespace( );

            Type type = resolver.Resolve(markupExtensionName);

            Object obj = null;
            List<Object> ctorArgs = new List< object >();

            for ( ;; ) {
                if ( peekNextChar( ) == '{' ) {
                    // inner markup extension processing

                    // syntax error if ctor arg defined after any property
                    if ( obj != null ) 
                        throw new InvalidOperationException("Syntax error: constructor argument" +
                                                            " cannot be after property assignment.");

                    Object value = processMarkupExtensionCore( context );
                    if ( value is IFixupToken )
                        return value;
                    ctorArgs.Add( value );
                } else {
                    String membernameOrString = processString( );
                    
                    if (membernameOrString.Length == 0)
                        throw new InvalidOperationException(
                            String.Format("Syntax error: member name or string expected at {0}",
                                index));

                    if ( peekNextChar( ) == '=' ) {
                        consumeChar( );
                        object value = peekNextChar( ) == '{' 
                            ? processMarkupExtensionCore( context )
                            : processString( );

                        if ( value is IFixupToken ) return value;

                        // construct object if not constructed yet
                        if ( obj == null ) obj = construct(type, ctorArgs);

                        // assign value to specified member
                        assignProperty( type, obj, membernameOrString, value );
                    } else if ( peekNextChar( ) == ',' || peekNextChar( ) == '}' ) {

                        // syntax error if ctor arg defined after any property
                        if (obj != null)
                            throw new InvalidOperationException("Syntax error: constructor argument" +
                                                                " cannot be after property assignment.");

                        // store membernameOrString as string argument of ctor
                        ctorArgs.Add( membernameOrString );

                    } else {
                        // it is '{' token, throw syntax error
                        throw new InvalidOperationException( 
                            String.Format("Syntax error : unexpected '{{' token at {0}.",
                            index) );
                    }
                }

                // after ctor arg or property assignment should be , or }
                if ( peekNextChar( ) == ',' ) {
                    consumeChar( );
                } else if ( peekNextChar( ) == '}' ) {
                    consumeChar( );

                    // construct object
                    if ( obj == null ) obj = construct( type, ctorArgs );

                    // markup extension is finished
                    break;
                } else {
                    // it is '{' token (without whitespace), throw syntax error
                    throw new InvalidOperationException(
                        String.Format( "Syntax error : unexpected '{{' token at {0}.",
                                       index ) );
                }

                processWhitespace( false );
            }

            return ((IMarkupExtension) obj).ProvideValue( context );
        }

        private void assignProperty( Type type, Object obj, string propertyName, object value ) {
            PropertyInfo property = type.GetProperty( propertyName);
            property.SetValue( obj, value, null );
        }

        /// <summary>
        /// Constructs object of specified type using specified ctor arguments list.
        /// </summary>
        private Object construct( Type type, List< Object > ctorArgs ) {
            ConstructorInfo[] constructors = type.GetConstructors( );
            List< ConstructorInfo > constructorInfos = constructors.Where( info => info.GetParameters( ).Length == ctorArgs.Count ).ToList( );
            if ( constructorInfos.Count == 0 ) {
                throw new InvalidOperationException("No suitable constructor");
            }
            if ( constructorInfos.Count > 1 ) {
                throw new InvalidOperationException("Ambiguous constructor call");
            }
            ConstructorInfo ctor = constructorInfos[ 0 ];
            ParameterInfo[] parameters = ctor.GetParameters( );
            Object[] convertedArgs = new object[ctorArgs.Count];
            for ( int i = 0; i < parameters.Length; i++ ) {
                convertedArgs[ i ] = ctorArgs[ i ];
            }
            return ctor.Invoke( convertedArgs );
        }

        /// <summary>
        /// Возвращает строку, в которой могут содержаться любые символы кроме {},=.
        /// Как только встречается один из этих символов без экранирования обратным слешем,
        /// парсинг прекращается.
        /// </summary>
        private string processString( ) {
            StringBuilder sb = new StringBuilder();
            bool escaping = false;
            for ( ;; ) {
                if ( !hasNextChar( ) ) {
                    if (escaping) throw new InvalidOperationException("Invalid syntax.");
                    break;
                }
                char c = peekNextChar( );
                if ( escaping ) {
                    sb.Append( c );
                    consumeChar( );
                    escaping = false;
                } else {
                    if ( c == '\\' ) {
                        escaping = true;
                        consumeChar( );
                    } else {
                        if ( c == '{' || c == '}' || c == ',' || c == '=' ) {
                            // break without consuming it
                            break;
                        } else {
                            sb.Append( c );
                            consumeChar( );
                        }
                    }
                }
            }
            return sb.ToString();
        }

        private string processQualifiedName( ) {
            StringBuilder sb = new StringBuilder();
            for ( ;; ) {
                char c = peekNextChar( );
                if ( c != ':' && !Char.IsLetterOrDigit( c ) ) {
                    break;
                }
                consumeChar( );
                sb.Append( c );
            }
            return sb.ToString( );
        }
    }
}

