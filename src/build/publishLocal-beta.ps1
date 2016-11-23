.\build.ps1

$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\interface\ISocketLite.PCL\bin\Release\ISocketLite.PCL.dll')).Version.ToString(3)

nuget.exe push -Source "1iveowlNuGetRepo" -ApiKey key .\Nuget\SocketLite.PCL.$version.nupkg
