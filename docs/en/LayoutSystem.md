Layout system
=============

Layout system is similar to WPF layout system. If you are unfamiliar with it, visit http://msdn.microsoft.com/en-us/library/ms745058(v=vs.110).aspx and http://msdn.microsoft.com/en-us/library/bb613548(v=vs.110).aspx pages. Main differences from WPF are:

- No distinction between `ContentControls`, `ItemsControls` and so on. One base class is `Control`, and it can have 0, 1 or more children. All panels (`Panel`, `Grid`) inherit from `Control` too.
- No `UIElement` -> `FrameworkElement` hierarchy. `Control` class is base class of all controls.
- No transforms (in console UI it is inactual)
- Many classes are missing (`DockPanel`, `WrapPanel`, `VirtualizingPanel` for instance)

But measuring and arrangement protocol is the same. So, if you want to create a custom control, you should read about measure and arrange in WPF.

## FAQ

### Q. How to write a control that will use all available space?

You can assume that if you override `MeasureOverride` to return availableSize unmodified it will be right solution. But it will not. The fact is `MeasureOverride` can be called with infinity argument. But according to layout contract `MeasureOverride` can't return infinity. We should do next things. In `MeasureOverride` our control should return minimal required size to render itself. In `ArrangeOverride` control should return `finalSize` (without any changes). And in `Render` method control should use `ActualWidth` and `ActualHeight` to render content (it is right way for `Render` method in any case). And after that in constructor we should set default `HorizontalAlignment` and `VerticalAlignment` to `Stretch`. 

How does it works ?

- Parent asks our control to determine desired size, control returns minimal required size
- If parent has slot greater than returned size (and our alignment is `Stretch`), it will pass to `ArrangeOverride` finalSize = all available space.
- Control returns finalSize without any modification
- In `Render` method our control uses provided size
 
`Button` for example uses this method. See in debugger if you are struggling with it.