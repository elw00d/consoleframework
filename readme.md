Console Framework
==

![](https://travis-ci.org/elw00d/consoleframework.svg?branch=develop)
[![NuGet](https://img.shields.io/nuget/v/Elwood.ConsoleFramework.svg)](https://www.nuget.org/packages/Elwood.ConsoleFramework)


Console framework is cross-platform toolkit that allows to develop [TUI] applications using C# and based on WPF-like concepts.

Features
--------

- Declarative markup (custom lightweight XAML implementation)
- Data binding (integrated with XAML markup)
- Retained mode rendering system
- WPF-compatible simple and flexible layout system
- A lot of controls available (including Grid, ScrollViewer, ListBox, ComboBox)
- Routed events system (compatible with WPF)
- Windows, Mac OS X and Linux (64-bit) support

![](http://gyazo.com/81e1ae92cfba8c7a1c2a98da7da75ad7.png)

Install from NuGet
--

NuGet package is available here [https://www.nuget.org/packages/Elwood.ConsoleFramework]

Build from source
--
To build a library with examples you can use standard dotnet tool:

```sh
dotnet build
```

It should work in all platforms.

Native dependencies
--
For Windows there are no native dependencies required. But in Linux and Mac OS X environments you should prepare some native dependencies to be able to execute examples. Dependencies are:

- libtermkey
- libc
- ncursesw

To build *libtermkey* go to its source code directory and simply run

```sh
make
```

After that you can copy binaries from ./libs into directory with examples. Or you can use binaries of libtermkey from zip in **/native** directory.

Libc and ncursesw are distributed in binaries in vast majority of Linux distros and OS X, so all you need is locate actual binaries and copy them in the output directory if the target machine does not have them, for example, if you are Mac OS X user, you will need to install libtermkey.

```sh
brew install --universal libtermkey
```

Console Framework expects that libraries will be available strictly by these names on Linux. If your system already has symlinks *libc.so.6* and *libncursesw.so.5*, you can skip this step.

Running examples
--
In Windows

```sh
dotnet run --project StandaloneExamples/ManyControls
```

In Linux

```sh
cd StandaloneExamples/ManyControls
dotnet build

# Copy native dependencies
unzip ../../native/libtermkey-0.18-x86_64.zip -d bin/Debug/netcoreapp2.0/

cd bin/Debug/netcoreapp2.0/
dotnet ManyControls.dll
```

Press Ctrl+D to exit application.

Running unit tests
--
```sh
dotnet test Tests
```

Development
--
There were two IDEs where I've worked with .NET Core project: Visual Studio 2017 Community and JetBrains Rider. Both of them works well with this source code.

Mono support
--
Support of Mono runtime have been discontinued. If you need library for Mono, you can download previous releases. All further development will be continued for .NET Core runtime only.


Terminal emulators in Mac OS X
--
Standard terminal emulator is not very good for console applications deals with mouse. My recommendation is to use [ITerm2]. ITerm2 provides a good emulation with mouse support. If you want to see how console framework renders in various Mac emulators, visit [http://elwood.su/2014/02/console-framework-on-mac/]

License
-------
Copyright 2011-2018 I. Kostromin

License: MIT/X11

[TUI]:http://en.wikipedia.org/wiki/Text-based_user_interface
[MacPorts]:http://www.macports.org/
[ITerm2]:http://www.iterm2.com/#/section/home
[http://elwood.su/2014/02/console-framework-on-mac/]:http://elwood.su/2014/02/console-framework-on-mac/
[https://www.nuget.org/packages/Elwood.ConsoleFramework]:https://www.nuget.org/packages/Elwood.ConsoleFramework
