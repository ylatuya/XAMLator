all: pack nuget
pack:
	msbuild /p:Configuration=Release XAMLator.Client.MonoDevelop/XAMLator.Client.MonoDevelop.csproj
	/Applications/Xamarin\ Studio.app/Contents/MacOS/mdtool setup pack XAMLator.Client.MonoDevelop/bin/Release/net461/XAMLator.Client.MonoDevelop.dll
	mv XAMLator.Client.MonoDevelop.XAMLator.Client.MonoDevelop_*.mpack AddinRepo/XAMLator.Client.MonoDevelop.mpack
	/Applications/Xamarin\ Studio.app/Contents/MacOS/mdtool setup rep-build AddinRepo

.PHONY:nuget
nuget:
	msbuild /p:Configuration=Release XAMLator.Server.Net47/XAMLator.Server.Net47.csproj
	msbuild /p:Configuration=Release XAMLator.Server.Droid/XAMLator.Server.Droid.csproj
	msbuild /p:Configuration=Release XAMLator.Server.iOS/XAMLator.Server.iOS.csproj
	nuget pack nuget/XAMLator.nuspec
