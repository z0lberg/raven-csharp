build: setup-nuget restore

setup-nuget:
	mkdir -p ../.nuget
	wget -O ../.nuget/nuget.exe https://dist.nuget.org/win-x86-commandline/latest/nuget.exe

restore:
	mono --runtime=v4.0.30319 "../.nuget/nuget.exe" Restore "../src/"

test: restore
	mono --debug --runtime=v4.0.30319 "../src/packages/Cake.0.8.0/Cake.exe" ./SharpRaven.cake -Verbosity=Diagnostic -Target=NuGet-Pack