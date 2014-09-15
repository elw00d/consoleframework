Console Framework
==

Console framework is cross-platform toolkit that allows to develop [TUI] applications using C# and based on WPF-like concepts.

Features
--------

- Declarative markup (custom lightweight XAML implementation)
- Data binding (integrated with XAML markup)
- Retained mode rendering system
- WPF-compatible simple and flexible layout system
- A lot of controls available (including Grid, ScrollViewer, ListBox, ComboBox)
- Routed events system (compatible with WPF)
- Windows, Mac OS X and any Linux (32-bit or 64-bit) support

![](http://gyazo.com/81e1ae92cfba8c7a1c2a98da7da75ad7.png)

License
-------

Copyright 2011-2014 I. Kostromin
License: MIT/X11

Build from source
--
To build a library with examples you can use [NAnt]:
```sh
nant build
```
It works in all platforms.

Native dependencies
--
In Windows there are no native dependencies. But in Linux and Mac OS X environments you should prepare some native dependencies to be able to execute examples. Dependencies are:

- libtermkey
- libc
- ncursesw

To build *libtermkey* go to its source code directory and simply run

```sh
make
```

After that you can copy binaries from ./libs into directory with examples. Or you can use binaries of libtermkey from zip in **/native** directory.

Libc and ncursesw are distributed in binaries in vast majority of Linux distros (in Mac OS X you may need to install them manually using [MacPorts]), so all you need is locate actual binaries and create a symlinks in local directory, for example, if you are Mac OS X user, you can create next symlinks

```sh
ln -s /usr/lib/libc.dylib libc.so.6
ln -s ./libncurses.5.4.dylib libncursesw.so.5
ln -s ./libtermkey.1.dylib libtermkey.so
```

Console Framework expects that libraries will be available strictly by this names. If your system already has symlinks *libc.so.6* and *libncursesw.so.5*, you can skip this step.

Running examples
--

```sh
mono Example_HelloWorld.exe
```

Press Ctrl+D to exit application.

Terminal emulators in Mac OS X
--
Standard terminal emulator is not very good for console applications deals with mouse. My recommendation is to use [ITerm2]. ITerm2 provides a good emulation with mouse support. If you want to see how console framework renders in various Mac emulators, visit [http://elwood.su/2014/02/console-framework-on-mac/]

[TUI]:http://en.wikipedia.org/wiki/Text-based_user_interface
[NAnt]:http://nant.sourceforge.net/
[MacPorts]:http://www.macports.org/
[ITerm2]:http://www.iterm2.com/#/section/home
[http://elwood.su/2014/02/console-framework-on-mac/]:http://elwood.su/2014/02/console-framework-on-mac/