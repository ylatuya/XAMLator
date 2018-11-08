[![Build Status](https://dev.azure.com/ylatuya/XAMLator/_apis/build/status/ylatuya.XAMLator)](https://dev.azure.com/ylatuya/XAMLator/_build/latest?definitionId=1)

# XAMLator live previewer for Xamarin.Forms

**XAMLator** is a live XAML previewer for Xamarin.Forms. Change a **XAML** view, its **code behind** or a **CSS** style sheet in the IDE and you preview it **live** in your application, in a real device or the emulator!

![monkeys](https://raw.githubusercontent.com/ylatuya/XAMLator/master/docs/monkeys.gif)

## Features:
  1. Works with any kind of Xamarin Forms project and MVVM frameworks.
  2. Live preview in Android and iOS emulators
  3. Live preview in real devices
  4. Preview in several devices at the same time.
  5. XAML live updates
  6. CSS live updates
  7. Code behind live updates
  8. Customizable previewer
  9. Support for dummy data bindings

## "But... Wait! Visual Studio has already a XAML previewer, why do I need XAMLator?"

Visual Studio already has a XAML previewer, but it has some limitations. The previewer only reads a XAML and tries to render it, whithout further context. If your view relies on any static initialization of the application, you are welcomed with a beatiful exception.

XAMLator works on a very different way, the code updates are sent to the device where the application is running and the application itself renders the view, where the applicatio is now correctly initialized. Another benefit is that you preview it on a real device and you can even preview it in several devices at the same time: an Android tablet, an iPhone X, an iPad, a desktop app... as many as you want!

## How does XAMLator compare to existing solutions?

XAMLator is like [Live XAML](https://www.livexaml.com/) but open source, hence free, and with live code behind updates!

## Vistual Studio plugin Installation

### macOS

Install the **XAMLator** add-in for VisualStudio for Mac in the **Add-in Manager**:
  1. Open Visual Studio->Extensions
  2. Search for XAMLator
  3. Install it!

The Add-in store is down, meanwhile you can install the add-in from the [CI build artifact](https://dev.azure.com/ylatuya/XAMLator/_build/latest\?definitionId\=1)

### Windows

XAMLator doens't have yet a plugin for Visual Studio, we are looking for contributors to create it!

Creating the plugin should be fairly simple following the [vs4mac example](https://github.com/ylatuya/XAMLator/tree/master/XAMLator.Client.MonoDevelop)

## Installation

1. Add the [XAMLator nuget](https://www.nuget.org/packages/XAMLator/) package to the application project, the Android, iOS, UWP or macOS project. If you are using a netstandard library to shared code, don't add the nuget to that project.

2. Initialize the server in the app initialization.

AppDelegate.cs in iOS

```csharp
public override bool FinishedLaunching(UIApplication app, NSDictionary options)
{
  global::Xamarin.Forms.Forms.Init();
  LoadApplication(new App());
#if DEBUG
  XAMLator.Server.PreviewServer.Run();
#endif
  return base.FinishedLaunching(app, options);
}
```

AppDelegate.cs in macOS

```csharp
public override void DidFinishLaunching(NSNotification notification)
{
  Forms.Init();
  LoadApplication(new App());
#if DEBUG
  XAMLator.Server.PreviewServer.Run();
#endif
  base.DidFinishLaunching(notification);
}
```

MainActivity.cs in Android

```csharp
protected override void OnCreate(Bundle bundle)
{
  TabLayoutResource = Resource.Layout.Tabbar;
  ToolbarResource = Resource.Layout.Toolbar;

  base.OnCreate(bundle);

  global::Xamarin.Forms.Forms.Init(this, bundle);
  LoadApplication(new App());
#if DEBUG
  XAMLator.Server.PreviewServer.Run();
#endif
}
```

### iOS additional setup

For iOS applications you have to pass the `--enable-repl` option to the mtouch additional arguments.
![enable-repl](https://raw.githubusercontent.com/ylatuya/XAMLator/master/docs/enable-repl.png)

### Android Emulator additional setup

To run it in the adnroid emulator you will have to reverse the TCP port 8488 so the emulator can reach the IDE when connection to localhost:8488

```bash
$ adb reverse tcp:8488 tcp:8488
```

## How to use it

Run your application in debug mode, it should start as usual.

To preview a Xamarin Forms View or Page open in the editor the .xaml or xaml.cs file.

In XAML views, changes are applied when you save the file.

In Code Behind, changes are applied as you type and one the IDE as finished analyzing the class 

For CSS update, once the CSS has been modified you have to open the view you want to preview for changes to be applied (in future versions this will be automatic)

### Previewing in multiple devices

To use the previewer in several devices at the same time you only need to start the application several times, one for each platform and device you want to preview. You can easilly do it using the "Run Item" option in the project's menu.
![multiple devices](https://raw.githubusercontent.com/ylatuya/XAMLator/master/docs/multiple-devices.png)


## Customization

XAMLator uses modal navigation to preview pages since it's the only kind of navigation that works globally in all platforms. Hierachical navigation requires the navigation to be performed from a NavigationPage, that it's not always available in all apps and it might vary depending on the MVVM framework you use. For XAMLator, it's also impossible to know how you are performing a navagitation for a given page as you could do it hierarchical, or modal, so if you happen to use this page with an hierarchical navigation, the NavigationBar will not be visible.

The navigation can be easilly customized with a new Previewer:

```csharp
public class CustomPreviewer : Previewer
	{
		public CustomPreviewer() : base(new Dictionary<Type, object> ())
		{
		}

		protected override Task ShowPreviewPage(Page previewPage)
		{
			return Application.Current.MainPage.Navigation.PushAsync(previewPage, false);
		}

		protected override Task HidePreviewPage(Page previewPage)
		{
			return Application.Current.MainPage.Navigation.PopAsync();
		}
	}
```

You can use your new previewer in the server initialization
```csharp
 XAMLator.Server.PreviewServer.Run(previewer:new CustomPreviewer ());
```

## Known issues

* Code behind updates only works if you edit the xaml.cs file. If you edit any other class, like a ViewModel, you will have to recompile.
* CSS updates do not apply automatically to the current view, you have to reopen the view in the editor to have them applied.

## Attribution

Support for code reloading was achieved thanks to [Continous](https://github.com/praeclarum/Continuous) from [@praeclarum](https://twitter.com/praeclaruma).
It was a great source to understand how Mono's evaluator works and support reloading of code behind.
Thanks!
