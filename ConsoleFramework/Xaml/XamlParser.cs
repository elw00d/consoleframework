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

        /// <summary>
        /// Creates the object graph using provided xaml.
        /// </summary>
        /// <param name="xaml"></param>
        /// <returns></returns>
        public static object CreateFromXaml( string xaml ) {
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
                                    object value = convertValueIfNeed( typeof ( String ), 
                                        propertyInfo.PropertyType, xmlReader.Value );
                                    propertyInfo.SetValue( top.obj, value, null );
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
                            string content = top.currentPropertyText;
                            PropertyInfo property = top.type.GetProperty( top.currentProperty );
                            property.SetValue( top.obj, convertValueIfNeed( typeof ( string ),
                                property.PropertyType, content ), null );
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
            }
            return null;
        }
    }
}
