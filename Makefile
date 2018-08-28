pack:
	# FIXME: Too many open files bug, build with the IDE
	# msbuild /p:Configuration=Release XAMLator.Client.MonoDevelop/XAMLator.Client.MonoDevelop.csproj
	/Applications/Xamarin\ Studio.app/Contents/MacOS/mdtool setup pack XAMLator.Client.MonoDevelop/bin/Release/net461/XAMLator.Client.MonoDevelop.dll
	mv XAMLator.Client.MonoDevelop.XAMLator.Client.MonoDevelop_*.mpack AddinRepo/XAMLator.Client.MonoDevelop.mpack
	/Applications/Xamarin\ Studio.app/Contents/MacOS/mdtool setup rep-build AddinRepo
