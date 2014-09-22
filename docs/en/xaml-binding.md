Binding in XAML
==

You can use XAML and Binding in XAML without Console Framework, in standalone application. To do this use need:

- Add reference to `Binding` project
- Copy to your project code of markup extensions in _ConsoleFramework.Xaml_ (`BindingMarkupExtension` and `ConvertMarkupExtension`)
- When call `XamlParser.CreateObjectFromXaml()`, you should set up reference in `defaultNamespaces` to namespace you used
  for `BindingMarkupExtension` and `ConvertMarkupExtension` (Or you will need to connect them always manually in XAML).
- When call `XmlParser.CreateObjectFromXaml()`, you should pass `dataContext` - object, whose class is implementing
  `INotifyPropertyChanged` and notifies about changed properties. This object will be passed to markup extensions and
  they'll be able to bind to its properties.

## Examples of usage

Simple two-way binding (TwoWay mode is default mode for objects implementing `INotifyPropertyChanged`):

```xml
<TextBox Text="{Binding Path=Title}"/>
```

Binding for data flow from source to target only. DataContext object is always _Source_, so next markup
demonstrates data flow from `dataContext` to `StatusBar`:

```xml
<StatusBar Title="{Binding Path=GroupBoxTitle, Mode=OneWay}"/>
```

If you need data to bind only one time in initialization, you can use `OneTime` mode - it works in same
direction as `OneWay` works, but fires only when data binding is being set up:

```xml
<GroupBox Title="{Binding Path=GroupBoxTitle, Mode=OneTime}"/>
```

Also, when setting up a data binding, you can set converter to be used when data flows between _Source_ and _Target_.
Data binding converters differs from XAML converters used to assign values to properties. Data binding converters
is designed for data binding exspecially and should implement `IBindingConverter` interface. Example:

```xml
<StatusBar Title="{Binding Path=StatusCode, Mode=OneWay, Converter={Ref converter}}"/>
```

If you want to use XAML converter in bindings, you can write adapter for it, or add custom property to 
`BindingMarkupExtension`. May be, it will be implemented out of the box in future versions.
