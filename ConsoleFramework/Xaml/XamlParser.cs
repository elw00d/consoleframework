using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using ConsoleFramework.Controls;

namespace ConsoleFramework.Xaml
{
    public class XamlParser
    {
        private class ObjectInfo
        {
            public Type type;
            public object obj;
            public string name;
            /// <summary>
            /// Текущее свойство, которое задаётся через точку.
            /// </summary>
            public string currentProperty;

            public string currentPropertyContent;
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
                                    // todo :
                                    Console.WriteLine(xmlReader.Name, xmlReader.Value);
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
                            top.currentPropertyContent = content;
                        }
                    }

                    if ( xmlReader.NodeType == XmlNodeType.EndElement ) {
                        // closed element having text content
                        if (top.currentPropertyContent != null) {
                            string content = top.currentPropertyContent;
                            PropertyInfo property = top.type.GetProperty( top.currentProperty );
                            Type typeArg = property.PropertyType.IsGenericType
                                    ? property.PropertyType.GetGenericArguments()[0]
                                    : null;
                            if (null == typeArg || !typeof ( ICollection< > ).MakeGenericType( typeArg )
                                                          .IsAssignableFrom( property.PropertyType ) ) {
                                if ( property.PropertyType == typeof ( string ) ) {
                                    property.SetValue( top.obj, content, null );
                                } else {
                                    property.SetValue( top.obj, convertValue( typeof ( string ),
                                                                              property.PropertyType, content ), null );
                                }
                            }
                            top.currentProperty = null;
                            top.currentPropertyContent = null;
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

                                    // use type conversion and handle collection if need
                                    PropertyInfo property = top.type.GetProperty( top.currentProperty );
                                    Type typeArg = property.PropertyType.IsGenericType
                                        ? property.PropertyType.GetGenericArguments( )[ 0 ]
                                        : null;
                                    if (null != typeArg && typeof(ICollection<>).MakeGenericType(typeArg).IsAssignableFrom(property.PropertyType))
                                    {
                                        object collection = property.GetValue( top.obj, null );
                                        MethodInfo methodInfo = collection.GetType( ).GetMethod( "Add" );
                                        if ( initialized.obj.GetType( ) == typeArg ) {
                                            methodInfo.Invoke( collection, new object[ ] { initialized.obj } );
                                        } else {
                                            object converted = convertValue( initialized.obj.GetType(  ), typeArg, initialized.obj );
                                            methodInfo.Invoke( collection, new object[ ] { converted } );
                                        }
                                    } else {
                                        if ( property.PropertyType.IsInstanceOfType(initialized.obj) ) {
                                            property.SetValue( top.obj, initialized.obj, null );
                                        } else {
                                            object converted = convertValue(initialized.obj.GetType(  ), property.PropertyType, initialized.obj);
                                            property.SetValue( top.obj, converted, null );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static object convertValue( Type source, Type dest, object value ) {
            // todo :
            if ( source == typeof ( string ) && dest == typeof ( bool ) ) {
                return new StringToBoolConverter( ).Convert( ( string ) value );
            }
            throw new NotSupportedException();
        }

        private static Type resolveType( string name ) {
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
