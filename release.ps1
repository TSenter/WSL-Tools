dotnet publish Flow.Launcher.Plugin.WSL-Tools -c Release -r win-x64 --no-self-contained
Compress-Archive -LiteralPath Flow.Launcher.Plugin.WSL-Tools/bin/Release/win-x64/publish -DestinationPath Flow.Launcher.Plugin.WSL-Tools/bin/WSL-Tools.zip -Force