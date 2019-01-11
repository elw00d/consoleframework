How does XAML work
==

XAML is processed using next algorithm. Parser reads elements in sequence and converts them to CLR objects. From root element parser also takes declarations of used namespaces. Object instance is created when parser reaches next xml element (or attribute). Then, if it is attribute, its value is assigned to property of currently configuring object. If it is nested element, its content will be analyzed recursively and corresponding CLR objects will instantiated too. Assignment of constructed object to property of high-level object is executed at the moment of the end of parsing its content, i.e. when parser reaches closing xml tag. At this moment object is fully configured with all nested objects, and will be assigned to property of high-level object (or will be added to collection, if high-level object property in this context is one of supported collections). When parser reaches the end of XAML document, constructed object is returned from method as result value.

## Simple example

```
<Window>
    <Window.Content>
        <Panel>
            <TextBlock Name="text" HorizontalAlignment="Center"></TextBlock>
            <Button Name="btnMaximize" Caption="Maximize"></Button>
            <Button Name="btnRestore" Caption="Restore"></Button>
        </Panel>
    </Window.Content>
</Window>
```

What happens when parser processes this XAML document:
- `Window` instantiated, using default constructor
- We got nested element `Window.Content`. Because it starts from `Window` (as our currently configuring object), it is definition of property `Content`. If nested element will not start from `Window.`, parser will decide that it is content property definition. Content property is special property for each class. By default its name is `Content`, but can be changed using `ContentPropertyAttribute` attribute (see WPF documentation for Content properties).
- `Panel` class instantiated (using default constructor)
- We got nested element that doesn't start from `Panel.`. So, we should determine what content property is defined for `Panel` class. In our case it will be `UIElementCollection Children` property. All nested elements (TextBlock and 2 Buttons) will define the value of this property.
- `TextBlock` instance is created (default ctor again). `Name` and `HorizontalAlignment` properties are set. Type of `Name` property is *String*, so no convertors need. Type of `HorizontalAlignment` property - *enum HorizontalAlignment*, and there are we need to use standard String to Enum converter.
- `TextBlock` is created and configured, we reach closing tag, and we should decide what property to use to assign to. Currently configuring object and high-level is `Panel`, and we are defining its Content property - `Children`. It implements `IList`, so created TextBlock will be added to it using `Add` method.
- Next two buttons are processed like TextBlock
- We have reached `</Panel>` closing tag. It means that panel is fully configured and ready to be assigned to property of higher-level object - `Window.Content`. This property is regular property (not a collection), type is *Control*, so no conversion need.
- Tag `</Window.Content>` tells us about property `Window.Content` configuration is finished.
- And, tag `</Window>` finishes the Window object configuration, and it will be returned as method result.

So, this XAML is equivalent to next imperative code:

```
Window window = new Window();
Panel panel = new Panel();

TextBlock textBlock = new TextBlock();
textBlock.Name = "text";
textBlock.HorizontalAlignment = HorizontalAlignment.Center;
panel.Children.Add(textBlock);

Button button1 = new Button();
button1.Name = "btnMaximize";
button1.Caption = "Maximize";
panel.Children.Add(button1);

Button button2 = new Button();
button2.Name = "btnRestore";
button2.Caption = "Restore";
panel.Children.Add(button2);

window.Content = panel;
```

## Content properties system

It is like in WPF. By default Content property name is `Content`. If you want to change this, you should mark class with `ContentPropertyAttribute` attribute:

```csharp
[ContentProperty("Controls")]
public class Grid : Control
```

## How does conversion work and how are collections handled

Build-in type conversions are: strings to numbers, enumerations plus some build-in converters for structs (`Thickness` - for `Margin` definition). If you want to use custom converter, you could call it using `Convert` markup extension:

```xml
<Window xmlns:x="http://consoleframework.org/xaml.xsd"
        xmlns:converters="clr-namespace:Binding.Converters;assembly=Binding">
    <Window.Resources>
        <string x:Key="testItem" x:Id="testStr">5</string>
        <converters:StringToIntegerConverter x:Key="2" x:Id="str2int"></converters:StringToIntegerConverter>
    </Window.Resources>
    <Panel>
        <TextBox MaxLength="{Convert Converter={Ref str2int}, Value={Ref testStr}}"/>
    </Panel>
</Window>
```

Any collection implementing `IList`, `ICollection<T>` or `IDictionary<string, T>` is supported.
When processing closing tag if property of higher-level object is `ICollection<T>`, current object will be
added into collection using `Add(T obj)` method - instead of search of suitable converter and sequently calling
setter. So, for collections you are not need setter at all - only getter is necessary.
`IDictionary<string, T>` properties handled in similar way. Example:

```xml
<Window.Resources>
    <item x:Key="1">String</item>
    <item x:Key="2">String 2</item>
</Window.Resources>
```

When parser will reach first item closing tag, "String" value will be added to Window.Resources collection.

In case of using properties with parametrized types, parser knows about T and will try to search suitable converter for object before adding it to collection (if object type differs from T). But if property implements non-parametrized type `IList`, object will be added without any conversion.

## Namespaces

When we call parser we pass the set of default namespaces as argument. This is a list of CLR namespaces (path to the namespace + assembly name) that will be used for search of objects (by tag names) and markup extensions declared in XAML and should be instantiated. All namespaces not listed in default namespaces list should be declared in root element of XAML document.

Example:

```xml
<my:Window Name="window2" Title="Very long window name"
        xmlns:x="http://consoleframework.org/xaml.xsd"
        xmlns:my="clr-namespace:ConsoleFramework.Controls;assembly=ConsoleFramework"
        xmlns:converters="clr-namespace:Binding.Converters;assembly=Binding"
        xmlns:xaml="clr-namespace:ConsoleFramework.Xaml;assembly=ConsoleFramework">
    <!-- Here are all types and markup extensions from all listed namespaces are available to use -->
</my:Window>
```

## Creating custom type objects with ctor args

Regular objects designed to use in XAML should have default constructor. If you need to instantiate a class without default constructor, you can create factory class and use them for it. But there are built-in ObjectFactory class that can create objects of any type using constructor with specified args and assign properties dynamically like if we created it using XAML directly. For example, we have class

```
class TestClass<T>
{
    public TestClass( int intProperty ) {
        IntProperty = intProperty;
    }

    public int IntProperty { get; set; }

    public string StringProperty { get; set; }

    public T TProperty { get; set; }
}
```

Using ObjectFactory we can instantiate it in XAML:

```xml
<object TypeName="ConsoleFramework.Xaml.TestClass`1[System.String]">
    <int x:Key="1">66</int>
    <string x:Key="IntProperty">55</string>
</object>
```

`TypeName` content is resolved using *Type.GetType(string assemblyQualifiedName)* call, so it is possible to need to specify full type name (with assembly name).

When we set `x:Key` to a number `1` we tell to factory that it is ctor argument with index 1. If `x:Key` value is not a number, `x:Key` will be interpreted as property name. You can use all tools available in regular properties definition syntax: text content or any markup extension call.

How does it work: when we create ObjectFactory instance, we configure its Content property (it is `Dictionary<string, object>`). But after finishing configuration this object is replaced by factory created object. When you use primitives explicit syntax like `<string>str</string>`, it is this mechanism too. All objects implementing `IFactory` interface will be replaced to factory-created object before assignment to higher-level object property.

## Built-in attributes

##### x:Id

Allows to specify unique identifier of any instantiated inside XAML object to allow to reference it later using `Ref` markup extension.

##### x:Key

Specifies key for `IDictionary<string, T>` collections.

__<p style="color: red">Important!</p>__

To use built-in attributes (`x:Key` or `x:Id`) you should declare corresponding namespace in root XML element (and it is not CLR namespace, it is just XSD):

```
<Window xmlns:x="http://consoleframework.org/xaml.xsd">
</Window>
```

`x` prefix can be changed to any you want.

## Built-in markup extensions

##### Ref

Allows to get reference to any another object by its `x:Id`:

```
{Ref Ref=myObject}
или
{Ref myObject}
```

Forward-references are supported too (implementation is like in WPF - using fixup tokens).

##### Type

Allows to get type object (with type `Type`) by type name:

```
{xaml:Type TestProject1.Xaml.TypeExtensionTest.ObjectToCreate\, TestProject1\, Version\=1.0.0.0\, Culture\=neutral\, PublicKeyToken\=null}
```

## Differences from WPF

If you are familiar with WPF, may be it is more simple to not read whole document, but only next differences:

- No Dependency Properties
- No `InitializeComponent()` method generated in partial class, all processing runs in runtime instead
- Because no code generation, no event handlers support - neither regular CLR events nor Attached Events
- Because no code generation, no fields generated by `Name` property
- No support for including dictionaries yet (mb will be added further)
- `x:Id` instead of `x:Name`
- Possible differences in conversions handling, adding to collections - to be sure you should read corresponding document parts

## Markup extension syntax differences
- XML-tokens like a `&quot;` is not supported - if you need to escape anything, use backslash everywhere
- In single quotes you can't write unescaped symbols `=,{}` - you should escape everything too
- There are only one Data Context object passed to markup extension. In WPF it is resolved using current context because `DataContext` is Dependency property. So this constraint is because no dependencies properties support and to leave XAML concept simplified, not linked with UI concepts.