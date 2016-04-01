build: setup-nuget restore

setup-nuget:
	mkdir -p .nuget
	wget -O .nuget/nuget.exe https://dist.nuget.org/win-x86-commandline/latest/nuget.exe

restore:
	mono --runtime=v4.0.30319 ".nuget/nuget.exe" Restore "src"

test: restore
	xbuild "./src/SharpRaven.build"
	mono --debug --runtime=v4.0.30319 ./src/packages/OpenCover.4.6.519/tools/OpenCover.Console.exe -register:user -target:"./src/packages/NUnit.Runners.2.6.4/tools/nunit-console.exe" -targetargs:"/noresults /noisolation /testcontainer:""./src/tests/SharpRaven.UnitTests/bin/Release/net45/SharpRaven.UnitTests.dll" -filter:"+[SharpRaven]*  -[SharpRaven]SharpRaven.Properties.*" -hideskipped:All -output:.\SharpRaven.xml
	# mono --debug --runtime=v4.0.30319 ./src/packages/NUnit.Runners.2.6.4/tools/nunit-console.exe ./src/tests/SharpRaven.UnitTests/bin/Release/net45/SharpRaven.UnitTests.dll -exclude=NuGet,NoMono -nodots
