using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using ConsoleFramework.Controls;

namespace ConsoleFramework.Xaml
{
    public class XamlParser
    {
        private class ObjectInfo
        {
            /// <summary>
            /// Тип конструируемого объекта.
            /// </summary>
            public Type type;
            /// <summary>
            /// Объект (или null если создаётся String).
            /// </summary>
            public object obj;
            /// <summary>
            /// Название тега.
            /// </summary>
            public string name;
            /// <summary>
            /// Текущее свойство, которое задаётся тегом с точкой в имени.
            /// </summary>
            public string currentProperty;
            /// <summary>
            /// Задаётся при парсинге тегов, содержимое которых - текст.
            /// </summary>
            public string currentPropertyText;
        }

        public class TestExtension : IMarkupExtension
        {
            public TestExtension( ) {
                Property1 = String.Empty;
                Property2 = String.Empty;
                Property3 = String.Empty;
            }

            public TestExtension( String param1 ) {
                Property1 = param1;
                Property2 = String.Empty;
                Property3 = String.Empty;
            }

            public TestExtension( String param1, String param2 ) {
                Property1 = param1;
                Property2 = param2;
                Property3 = String.Empty;
            }

            public String Property1 { get; set; }

            public String Property2 { get; set; }

            public String Property3 { get; set; }

            public object ProvideValue( IMarkupExtensionContext context ) {
                return Property1 + "_" + Property2 + "_" + Property3;
            }
        }

        public class TestResolver : IMarkupExtensionsResolver
        {
            // todo :
            public Type Resolve( string name ) {
                if (name == "Test")
                    return typeof(TestExtension);
                if ( name == "Binding" )
                    return typeof ( BindingMarkupExtension );
                return null;
            }
        }

        private class MarkupExtensionContext : IMarkupExtensionContext
        {
            public string PropertyName { get; private set; }
            public object Object { get; private set; }
            public object DataContext { get; private set; }

            public MarkupExtensionContext( string propertyName,
                object obj, object dataContext) {
                PropertyName = propertyName;
                Object = obj;
                DataContext = dataContext;
            }
        }

        /// <summary>
        /// Если str начинается с одинарной открывающей фигурной скобки, то метод обрабатывает его
        /// как вызов расширения разметки, и возвращает результат, или выбрасывает исключение,
        /// если при парсинге или выполнении возникли ошибки. Если же str начинается c комбинации
        /// {}, то остаток строки возвращается просто строкой.
        /// </summary>
        private static Object processText( String text,
            String currentProperty, object currentObject, object dataContext ) {
            if ( String.IsNullOrEmpty( text ) ) return String.Empty;

            if ( text[ 0 ] != '{' ) {
                // interpret whole text as string
                return text;
            } else if (text.Length > 1 && text[1] == '}') {
                // interpret the rest as string
                return text.Length > 2 ? text.Substring(2) : String.Empty;
            } else {
                // todo : use real resolver
                MarkupExtensionsParser markupExtensionsParser = new MarkupExtensionsParser(
                    new TestResolver( ), text );
                // todo : use real context
                MarkupExtensionContext context = new MarkupExtensionContext( currentProperty, currentObject, dataContext );
                return markupExtensionsParser.ProcessMarkupExtension(context);
            }
        }

        /// <summary>
        /// Creates the object graph using provided xaml.
        /// </summary>
        /// <param name="xaml"></param>
        /// <returns></returns>
        public static object CreateFromXaml( string xaml, object dataContext ) {
            if (null == xaml) throw new ArgumentNullException("xaml");

            object result = null;

            using ( XmlReader xmlReader = XmlReader.Create( new StringReader( xaml ) ) ) {

                // stack of constructing objects
                Stack<ObjectInfo> objects = new Stack< ObjectInfo >();
                ObjectInfo top = null;

                while ( xmlReader.Read( ) ) {
                    Console.WriteLine(xmlReader.Name);

                    if ( xmlReader.NodeType == XmlNodeType.Element ) {
                        String name = xmlReader.Name;
                        
                        // одно из предопределённых названий
                        if ( name == "item" ) {
                            // predefined string object
                            top = new ObjectInfo(  )
                                {
                                    name = "item",
                                    type = typeof(string)
                                };
                            objects.Push( top );
                        } else if ( top != null && name.StartsWith( top.name + "." ) ) {
                            // property
                            if ( top.currentProperty != null )
                                throw new ApplicationException( "Illegal syntax in property value definition." );
                            string propertyName = name.Substring( objects.Peek( ).name.Length + 1 );
                            top.currentProperty = propertyName;
                        } else {
                            // object
                            Type type = resolveType( name );
                            ConstructorInfo constructorInfo = type.GetConstructor( new Type[ 0 ] );
                            if ( null == constructorInfo ) {
                                throw new ApplicationException(
                                    String.Format( "Type {0} has no default constructor.", type.FullName ) );
                            }
                            Object invoke = constructorInfo.Invoke( new object[ 0 ] );
                            top = new ObjectInfo( )
                                {
                                    name = name,
                                    obj = invoke,
                                    type = type
                                };
                            objects.Push( top );

                            // process attributes
                            if ( xmlReader.HasAttributes ) {
                                while ( xmlReader.MoveToNextAttribute( ) ) {
                                    //
                                    PropertyInfo propertyInfo = top.type.GetProperty( xmlReader.Name );
                                    Object value = processText(xmlReader.Value,
                                        xmlReader.Name,
                                        top.obj,
                                        dataContext);
                                    if ( null != value ) {
                                        object convertedValue = convertValueIfNeed( value.GetType( ),
                                                                                    propertyInfo.PropertyType, value );
                                        propertyInfo.SetValue( top.obj, convertedValue, null );
                                    }
                                    //
                                }
                                xmlReader.MoveToElement( );
                            }
                        }
                    }

                    if ( xmlReader.NodeType == XmlNodeType.Text ) {
                        // this call moves xmlReader current element forward
                        string content = xmlReader.ReadContentAsString( );
                        if ( top.name == "item" ) {
                            top.obj = content;
                        } else {
                            top.currentPropertyText = content;
                        }
                    }

                    if ( xmlReader.NodeType == XmlNodeType.EndElement ) {
                        // closed element having text content
                        if (top.currentPropertyText != null) {
                            PropertyInfo property = top.type.GetProperty(top.currentProperty);
                            Object value = processText( top.currentPropertyText,
                                top.currentProperty,
                                top.obj,
                                dataContext);
                            if ( value != null ) {
                                Object convertedValue = convertValueIfNeed( value.GetType( ),
                                                                            property.PropertyType, value );
                                property.SetValue( top.obj, convertedValue, null );
                            }
                            top.currentProperty = null;
                            top.currentPropertyText = null;
                        } else {
                            // closed element having sub-element content
                            if ( top.currentProperty != null ) {
                                // был закрыт один из тегов-свойств, дочерний элемент
                                // уже присвоен свойству, поэтому ничего делать не нужно, кроме
                                // обнуления currentProperty
                                top.currentProperty = null;
                            } else {
                                // был закрыт основной тег текущего конструируемого объекта
                                // нужно получить объект уровнем выше и присвоить себя свойству этого
                                // объекта, либо добавить в свойство-коллекцию, если это коллекция
                                ObjectInfo initialized = objects.Pop( );
                                if ( objects.Count == 0 ) {
                                    result = initialized.obj;
                                } else {
                                    top = objects.Peek( );

                                    string propertyName;
                                    if ( top.currentProperty != null ) {
                                        propertyName = top.currentProperty;
                                    } else {
                                        // todo : determine name of content property
                                        propertyName = "Content";
                                    }

                                    // use type conversion and handle collection if need
                                    PropertyInfo property = top.type.GetProperty(propertyName);
                                    Type typeArg = property.PropertyType.IsGenericType
                                        ? property.PropertyType.GetGenericArguments( )[ 0 ]
                                        : null;
                                    if (null != typeArg && typeof(ICollection<>).MakeGenericType(typeArg).IsAssignableFrom(property.PropertyType))
                                    {
                                        object collection = property.GetValue( top.obj, null );
                                        MethodInfo methodInfo = collection.GetType( ).GetMethod( "Add" );
                                        object converted = convertValueIfNeed( initialized.obj.GetType(  ), typeArg, initialized.obj );
                                        methodInfo.Invoke( collection, new[ ] { converted } );
                                    } else {
                                        property.SetValue(top.obj, convertValueIfNeed(
                                            initialized.obj.GetType(), property.PropertyType, initialized.obj), null);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Converts the value from source type to destination if need
        /// using default conversion strategies and registered type converters.
        /// </summary>
        /// <param name="source">Type of source value</param>
        /// <param name="dest">Type of destination</param>
        /// <param name="value">Source value</param>
        private static object convertValueIfNeed( Type source, Type dest, object value ) {
            if ( dest.IsAssignableFrom( source ) ) {
                return value;
            }

            // process enumerations
            if ( source == typeof ( String ) && dest.IsEnum ) {
                string[ ] enumNames = dest.GetEnumNames( );
                for ( int i = 0, len = enumNames.Length; i < len; i++ ) {
                    if ( enumNames[i] == (String) value ) {
                        return Enum.ToObject( dest, dest.GetEnumValues( ).GetValue( i ) );
                    }
                }
                throw new ApplicationException("Specified enum value not found.");
            }

            // todo : default converters for primitives
            if ( source == typeof ( string ) && dest == typeof ( bool ) ) {
                return new StringToBoolConverter( ).Convert( ( string ) value );
            }
            if ( source == typeof ( string ) && dest == typeof ( int ) ) {
                return int.Parse( ( string ) value );
            }
            throw new NotSupportedException();
        }

        private static Type resolveType( string name ) {
            // todo : scan default namespaces and connected namespaces
            switch ( name ) {
                case "Window":
                    return typeof ( Window );
                case "GroupBox":
                    return typeof ( GroupBox );
                case "ScrollViewer":
                    return typeof ( ScrollViewer );
                case "ListBox":
                    return typeof ( ListBox );
                case "Panel":
                    return typeof ( Panel );
                case "TextBox":
                    return typeof ( TextBox );
            }
            return null;
        }
    }
}
