param([string]$betaver)

if ([string]::IsNullOrEmpty($betaver)) {
	$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\interface\ISocketLite.Netstandard\bin\Release\netstandard1.0\ISocketLite.PCL.dll')).Version.ToString(3)
	}
else {
	$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\interface\ISocketLite.Netstandard\bin\Release\netstandard1.0\ISocketLite.PCL.dll')).Version.ToString(3) + "-" + $betaver
}

.\build.ps1 $version

c:\tools\nuget\Nuget.exe push .\Nuget\SocketLite.PCL.$version.nupkg -Source https://www.nuget.org