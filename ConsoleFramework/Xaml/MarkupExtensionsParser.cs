using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ConsoleFramework.Xaml
{
    public interface IMarkupExtension
    {
        /// <summary>
        /// todo : change Object context to some specialized interface
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Object ProvideValue( Object context );
    }

    public interface IMarkupExtensionsResolver
    {
        Type Resolve( String name );
    }

    public class MarkupExtensionsParser
    {
        private IMarkupExtensionsResolver resolver;

        public MarkupExtensionsParser( IMarkupExtensionsResolver resolver ) {
            this.resolver = resolver;
        }

        private String text;
        private int index;

        private bool hasNextChar( ) {
            return index < text.Length;
        }

        private bool hasNextNChars( int n ) {
            return index + (n-1) < text.Length;
        }

        private char consumeChar( ) {
            return text[ index++ ];
        }

        private char peekNextChar( ) {
            return text[ index ];
        }

        private char peekNextNChar( int n ) {
            return text[ index - n ];
        }

        /// <summary>
        /// Если text начинается с одинарной открывающей фигурной скобки, то метод обрабатывает его
        /// как вызов расширения разметки, и возвращает результат, или выбрасывает исключение,
        /// если при парсинге или выполнении возникли ошибки. Если же text начинается c комбинации
        /// {}, то остаток строки возвращается просто строкой. Те же правила действуют при обработке 
        /// вложенных вызовов расширений разметки.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public Object ProcessText( String text, Object context ) {
            if ( String.IsNullOrEmpty( text ) ) return String.Empty;

            this.text = text;
            this.index = 0;

            if ( text[ 0 ] != '{' || text.Length > 1 && text[ 1 ] == '}' ) {
                // interpret the rest as string
                return text.Length > 2 ? text.Substring( 2 ) : String.Empty;
            } else {
                // interpret as markup extension expression
                return processMarkupExtension( context);
            }
        }

        public void processWhitespace(bool necessary = true) {
            if ( necessary ) {
                // at least one whitespace should be
                if (peekNextChar( ) != ' ') 
                    throw new InvalidOperationException(
                        String.Format("Syntax error: whitespace expected at {0}.", index));
            }
            while ( peekNextChar( ) == ' ' ) consumeChar( );
        }

        public Object processMarkupExtension( Object context) {
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

                    Object value = processMarkupExtension( context );
                    ctorArgs.Add( value );
                } else {
                    String membernameOrString = processString( );
                    if ( peekNextChar( ) == '=' ) {
                        consumeChar( );
                        object value = peekNextChar( ) == '{' 
                            ? processMarkupExtension( context )
                            : processString( );

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
            // todo : use type conversion if need
            property.SetValue( obj, value, null );
        }

        private Object construct( Type type, List< Object > ctorArgs ) {
            ConstructorInfo[] constructors = type.GetConstructors( );
            List< ConstructorInfo > constructorInfos = constructors.Where( info => info.GetParameters( ).Length == ctorArgs.Count ).ToList( );
            if ( constructorInfos.Count == 0 ) {
                throw new InvalidOperationException("No suitable constructor");
            }
            if ( constructorInfos.Count > 1 ) {
                throw new InvalidOperationException("Ambigious constructor call");
            }
            ConstructorInfo ctor = constructorInfos[ 0 ];
            ParameterInfo[] parameters = ctor.GetParameters( );
            Object[] convertedArgs = new object[ctorArgs.Count];
            for ( int i = 0; i < parameters.Length; i++ ) {
                ParameterInfo parameter = parameters[ i ];
                
                // todo : convert ctorArg to parameter type if need
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

