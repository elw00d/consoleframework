using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace ConsoleFramework.Xaml
{
    public class XamlParser
    {
        private readonly List< string > defaultNamespaces;

        public XamlParser(List<string> defaultNamespaces ) {
            if (null == defaultNamespaces)
                throw new ArgumentNullException("defaultNamespaces");
            this.defaultNamespaces = defaultNamespaces;
        }

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
            /// <summary>
            /// Ключ, задаваемый атрибутом x:Key (если есть) - по этому ключу объект будет
            /// положен в Dictionary-свойство родительского объекта.
            /// </summary>
            public string key;
            /// <summary>
            /// Ключ, задаваемый атрибутом x:Id (если есть). По этому ключу объект будет
            /// доступен из расширений разметки по ссылкам (например, через Ref).
            /// </summary>
            public string id;
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
                if ( name == "Ref" )
                    return typeof ( RefMarkupExtension );
                if ( name == "Convert" )
                    return typeof ( ConvertMarkupExtension );
                return null;
            }
        }

        private class FixupToken : IFixupToken
        {
            /// <summary>
            /// Строковое представление расширения разметки, которое вернуло этот токен.
            /// </summary>
            public string Expression;
            /// <summary>
            /// Имя свойства, которое задано этим расширением разметки.
            /// </summary>
            public string PropertyName;
            /// <summary>
            /// Объект, свойство которого определяется расширением разметки.
            /// </summary>
            public object Object;
            /// <summary>
            /// Переданный в расширение разметки dataContext.
            /// </summary>
            public object DataContext;
            /// <summary>
            /// Список x:Id, которые не были найдены в текущем состоянии графа объектов,
            /// но которые необходимы для полного выполнения ProvideValue.
            /// </summary>
            public IEnumerable<string> Ids;
        }

        private class MarkupExtensionContext : IMarkupExtensionContext
        {
            public string PropertyName { get; private set; }
            public object Object { get; private set; }
            public object DataContext { get; private set; }
            private XamlParser self;
            private string expression;

            public object GetObjectById( string id ) {
                object value;
                return self.objectsById.TryGetValue( id, out value ) ? value : null;
            }

            /// <summary>
            /// fixupTokensAvailable = true означает, что парсинг ещё не закончен, и ещё можно
            /// создать FixupToken, false означает, что парсинг уже завершён, и новых объектов
            /// уже не появится, поэтому если расширение разметки не может обнаружить ссылку на
            /// объект, то ему уже нечего делать, кроме как завершать работу выбросом исключения.
            /// </summary>
            public bool IsFixupTokenAvailable { get { return self.objects.Count != 0; } }

            public IFixupToken GetFixupToken( IEnumerable< string > ids ) {
                if (!IsFixupTokenAvailable)
                    throw new InvalidOperationException("Fixup tokens are not available now.");
                FixupToken fixupToken = new FixupToken(  );
                fixupToken.Expression = expression;
                fixupToken.PropertyName = PropertyName;
                fixupToken.Object = Object;
                fixupToken.DataContext = DataContext;
                fixupToken.Ids = ids;
                return fixupToken;
            }

            public MarkupExtensionContext( XamlParser self,
                                           string expression,
                                           string propertyName,
                                           object obj,
                                           object dataContext ) {
                this.self = self;
                this.expression = expression;
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
        private Object processText( String text,
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
                MarkupExtensionContext context = new MarkupExtensionContext(
                    this, text, currentProperty, currentObject, dataContext);
                object providedValue = markupExtensionsParser.ProcessMarkupExtension( context );
                if ( providedValue is IFixupToken ) {
                    fixupTokens.Add( ( FixupToken ) providedValue );
                    // Null means no value will be assigned to target property
                    return null;
                }
                return providedValue;
            }
        }

        // { prefix -> namespace }
        private readonly Dictionary<String, String> namespaces = new Dictionary<string, string>();

        private object dataContext;
        /// <summary>
        /// Стек конфигурируемых объектов. На верху стека всегда лежит
        /// текущий конфигурируемый объект.
        /// </summary>
        private readonly Stack<ObjectInfo> objects = new Stack<ObjectInfo>();
        /// <summary>
        /// Возвращает текущий конфигурируемый объект или null, если такового нет.
        /// </summary>
        private ObjectInfo Top {
            get { return objects.Count > 0 ? objects.Peek( ) : null; }
        }
        // Result object
        private object result;

        /// <summary>
        /// Map { x:Id -> object } of fully configured objects available to reference from
        /// markup extensions.
        /// </summary>
        private readonly Dictionary<String, Object> objectsById = new Dictionary< string, object >();

        /// <summary>
        /// List of fixup tokens used to defer objects by id resolving if markup extension
        /// has forward references to objects declared later.
        /// </summary>
        private readonly List< FixupToken > fixupTokens = new List< FixupToken >();

        /// <summary>
        /// Creates the object graph using provided xaml.
        /// </summary>
        /// <param name="xaml"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        public object CreateFromXaml( string xaml, object dataContext ) {
            if (null == xaml) throw new ArgumentNullException("xaml");
            this.dataContext = dataContext;
            
            using ( XmlReader xmlReader = XmlReader.Create( new StringReader( xaml ) ) ) {
                while ( xmlReader.Read( ) ) {
                    if ( xmlReader.NodeType == XmlNodeType.Element ) {
                        String name = xmlReader.Name;
                        
                        // explicit property syntax
                        if ( Top != null && name.StartsWith( Top.name + "." ) ) {
                            if ( Top.currentProperty != null )
                                throw new ApplicationException( "Illegal syntax in property value definition." );
                            string propertyName = name.Substring( Top.name.Length + 1 );
                            Top.currentProperty = propertyName;
                        } else {
                            objects.Push( createObject( name ) );

                            // process attributes
                            if ( xmlReader.HasAttributes ) {
                                while ( xmlReader.MoveToNextAttribute( ) ) {
                                    //
                                    string attributePrefix = xmlReader.Prefix;
                                    string attributeName = xmlReader.LocalName;
                                    string attributeValue = xmlReader.Value;

                                    processAttribute( attributePrefix, attributeName, attributeValue );
                                    //
                                }
                                xmlReader.MoveToElement( );
                            }
                        }
                    }

                    if ( xmlReader.NodeType == XmlNodeType.Text ) {
                        // this call moves xmlReader current element forward
                        string content = xmlReader.ReadContentAsString( );
                        if ( Top.name == "item" ) {
                            Top.obj = content;
                        } else {
                            Top.currentPropertyText = content;
                        }
                    }

                    if ( xmlReader.NodeType == XmlNodeType.EndElement ) {
                        processEndElement( );
                    }
                }
            }

            // После обработки всех элементов последний раз обращаемся к
            // расширениям разметки, ожидающим свои forward-references
            processFixupTokens();

            return result;
        }

        /// <summary>
        /// todo : учитывать аргументы конструктора ?
        /// todo : добавить встроенных типов ?
        /// </summary>
        private ObjectInfo createObject( string name ) {
            // object
            if ( name == "item" ) {
                // predefined string object
                return new ObjectInfo( )
                    {
                        name = "item",
                        type = typeof ( string )
                    };
            } else {
                Type type = resolveType( name );
                ConstructorInfo constructorInfo = type.GetConstructor( new Type[ 0 ] );
                if ( null == constructorInfo ) {
                    throw new ApplicationException(
                        String.Format( "Type {0} has no default constructor.", type.FullName ) );
                }
                Object invoke = constructorInfo.Invoke( new object[ 0 ] );
                return new ObjectInfo( )
                    {
                        name = name,
                        obj = invoke,
                        type = type
                    };
            }
        }

        private void processAttribute( string attributePrefix, string attributeName, string attributeValue ) {
            // If we have found xmlns-attributes on root object, register them
            // in namespaces dictionary
            if ( attributePrefix == "xmlns" && objects.Count == 1 ) {
                namespaces.Add( attributeName, attributeValue );
            } else {
                if ( attributePrefix != string.Empty ) {
                    if ( !namespaces.ContainsKey( attributePrefix ) )
                        throw new InvalidOperationException(
                            string.Format( "Unknown prefix {0}", attributePrefix ) );
                    string namespaceUrl = namespaces[ attributePrefix ];
                    if ( namespaceUrl == "http://consoleframework.org/xaml.xsd" ) {
                        if ( attributeName == "Key" ) {
                            Top.key = attributeValue;
                        } else if ( attributeName == "Id" ) {
                            Top.id = attributeValue;
                        }
                    }
                } else {
                    // Process attribute as property assignment
                    PropertyInfo propertyInfo = Top.type.GetProperty( attributeName );
                    Object value = processText( attributeValue, attributeName, Top.obj,
                                                dataContext );
                    if ( null != value ) {
                        object convertedValue = convertValueIfNeed( value.GetType( ),
                                                                    propertyInfo.PropertyType,
                                                                    value );
                        propertyInfo.SetValue( Top.obj, convertedValue, null );
                    }
                }
            }
        }

        /// <summary>
        /// Finishes configuring current object and assigns it to property of parent object.
        /// </summary>
        private void processEndElement( ) {
            // closed element having text content
            if ( Top.currentPropertyText != null ) {
                PropertyInfo property = Top.type.GetProperty( Top.currentProperty );
                Object value = processText( Top.currentPropertyText,
                                            Top.currentProperty,
                                            Top.obj,
                                            dataContext );
                if ( value != null ) {
                    Object convertedValue = convertValueIfNeed( value.GetType( ),
                                                                property.PropertyType, value );
                    property.SetValue( Top.obj, convertedValue, null );
                }
                Top.currentProperty = null;
                Top.currentPropertyText = null;
            } else {
                // closed element having sub-element content
                if ( Top.currentProperty != null ) {
                    // был закрыт один из тегов-свойств, дочерний элемент
                    // уже присвоен свойству, поэтому ничего делать не нужно, кроме
                    // обнуления currentProperty
                    Top.currentProperty = null;
                } else {
                    // был закрыт основной тег текущего конструируемого объекта
                    // нужно получить объект уровнем выше и присвоить себя свойству этого
                    // объекта, либо добавить в свойство-коллекцию, если это коллекция
                    ObjectInfo initialized = objects.Pop( );
                    if ( objects.Count == 0 ) {
                        result = initialized.obj;
                    } else {
                        string propertyName;
                        if ( Top.currentProperty != null ) {
                            propertyName = Top.currentProperty;
                        } else {
                            // todo : determine name of content property
                            propertyName = "Content";
                        }

                        // If parent object property is ICollection<T>,
                        // add current object into them as T (will conversion if need)
                        PropertyInfo property = Top.type.GetProperty( propertyName );
                        Type typeArg1 = property.PropertyType.IsGenericType
                                            ? property.PropertyType.GetGenericArguments( )[ 0 ]
                                            : null;
                        if ( null != typeArg1 &&
                             typeof ( ICollection< > ).MakeGenericType( typeArg1 ).IsAssignableFrom( property.PropertyType ) ) {
                            object collection = property.GetValue( Top.obj, null );
                            MethodInfo methodInfo = collection.GetType( ).GetMethod( "Add" );
                            object converted = convertValueIfNeed( initialized.obj.GetType( ), typeArg1, initialized.obj );
                            methodInfo.Invoke( collection, new[ ] { converted } );
                        } else {
                            // If parent object property is IDictionary<string, T>,
                            // add current object into them (by x:Key value) 
                            // with conversion to T if need
                            Type typeArg2 = property.PropertyType.IsGenericType &&
                                            property.PropertyType.GetGenericArguments( ).Length > 1
                                                ? property.PropertyType.GetGenericArguments( )[ 1 ]
                                                : null;
                            if ( null != typeArg1 && typeArg1 == typeof ( string )
                                 && null != typeArg2 &&
                                 typeof ( IDictionary< , > ).MakeGenericType( typeArg1, typeArg2 )
                                                            .IsAssignableFrom( property.PropertyType ) ) {
                                object dictionary = property.GetValue( Top.obj, null );
                                MethodInfo methodInfo = dictionary.GetType( ).GetMethod( "Add" );
                                object converted = convertValueIfNeed( initialized.obj.GetType( ),
                                                                       typeArg2, initialized.obj );
                                if ( null == initialized.key )
                                    throw new InvalidOperationException(
                                        "Key is not specified for item of dictionary" );
                                methodInfo.Invoke( dictionary, new[ ] { initialized.key, converted } );
                            } else {
                                // Handle as property - call setter with conversion if need
                                property.SetValue( Top.obj, convertValueIfNeed(
                                    initialized.obj.GetType( ), property.PropertyType, initialized.obj ),
                                                   null );
                            }
                        }
                    }

                    // Если у объекта задан x:Id, добавить его в objectsById
                    if ( initialized.id != null ) {
                        if (objectsById.ContainsKey( initialized.id ))
                            throw new InvalidOperationException(string.Format("Object with Id={0} redefinition.", initialized.id));
                        objectsById.Add( initialized.id, initialized.obj );

                        processFixupTokens( );
                    }
                }
            }
        }

        private void processFixupTokens( ) {
            // Выполнить поиск fixup tokens, желания которых удовлетворены,
            // и вызвать расширения разметки для них снова
            List< FixupToken > tokens = new List< FixupToken >( fixupTokens );
            fixupTokens.Clear(  );
            foreach ( FixupToken token in tokens ) {
                if ( token.Ids.All( id => objectsById.ContainsKey( id ) ) ) {
                    // todo : use real resolver
                    MarkupExtensionsParser markupExtensionsParser = new MarkupExtensionsParser(
                        new TestResolver(), token.Expression);
                    MarkupExtensionContext context = new MarkupExtensionContext(
                        this, token.Expression, token.PropertyName, token.Object, token.DataContext);
                    object providedValue = markupExtensionsParser.ProcessMarkupExtension(context);
                    if ( providedValue is IFixupToken ) {
                        fixupTokens.Add( ( FixupToken ) providedValue );
                    } else {
                        // assign providedValue to property of object
                        if (null != providedValue) {
                            PropertyInfo propertyInfo = token.Object.GetType(  ).GetProperty(
                                token.PropertyName);
                            object convertedValue = convertValueIfNeed(providedValue.GetType(),
                                                                        propertyInfo.PropertyType,
                                                                        providedValue);
                            propertyInfo.SetValue(token.Object, convertedValue, null);
                        }
                    }
                } else {
                    fixupTokens.Add( token );
                }
            }
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

        /// <summary>
        /// Принимает на вход название типа и возвращает объект Type, ему соответствующий.
        /// Название типа может быть как с префиксом (qualified), так и без него.
        /// Если название типа содержит префикс, то поиск будет осуществляться в соответствующем
        /// объявленном clr-namespace. Если же название префикса не содержит, поиск будет
        /// выполняться в наборе пространств имён по умолчанию (defaultNamespaces), которые
        /// задаются в конструкторе класса XamlParser.
        /// </summary>
        private Type resolveType( string name ) {
            List< string > namespacesToScan;
            string typeName;

            if ( name.Contains( ":" ) ) {
                string prefix = name.Substring( 0, name.IndexOf( ':' ) );
                if (name.IndexOf( ':' ) + 1 >= name.Length)
                    throw new InvalidOperationException(string.Format("Invalid type name {0}", name));
                typeName = name.Substring( name.IndexOf( ':' ) + 1 );
                if ( !namespaces.ContainsKey( prefix ) )
                    throw new InvalidOperationException( string.Format( "Unknown prefix {0}", prefix ) );
                namespacesToScan = new List< string >() { namespaces[prefix] };
            } else {
                namespacesToScan = defaultNamespaces;
                typeName = name;
            }

            // Scan namespaces todo : cache types lists
            foreach ( string ns in namespacesToScan ) {
                Regex regex = new Regex( "clr-namespace:(.+);assembly=(.+)" );
                MatchCollection matchCollection = regex.Matches( ns );
                if (matchCollection.Count == 0)
                    throw new InvalidOperationException(string.Format("Invalid clr-namespace syntax: {0}", ns));
                string namespaceName = matchCollection[ 0 ].Groups[ 1 ].Value;
                string assemblyName = matchCollection[ 0 ].Groups[ 2 ].Value;

                Assembly assembly = Assembly.Load( assemblyName );
                List< Type > types = assembly.GetTypes( ).Where( type => type.Namespace == namespaceName
                    && type.Name == typeName ).ToList( );
                if (types.Count == 0) 
                    throw new InvalidOperationException(string.Format("Type {0} not found.", name));
                if (types.Count > 1)
                    throw new InvalidOperationException("Assertion error.");
                return types[ 0 ];
            }

            throw new InvalidOperationException(string.Format("Cannot resolve type {0}", name));
        }
    }
}
