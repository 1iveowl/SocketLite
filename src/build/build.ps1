param([string]$version)

if ([string]::IsNullOrEmpty($version)) {$version = "0.0.1"}

$msbuild = join-path -path "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin" -childpath "msbuild.exe"

&$msbuild ..\interface\ISocketLite.Netstandard\ISocketLite.Netstandard.csproj /t:Build /p:Configuration="Release"
#&$msbuild ..\main\SocketLite.Bait.Netstandard\SocketLite.Bait.Netstandard.csproj /t:Build /p:Configuration="Release"

&$msbuild ..\main\CrossPlatform\SocketLite.Android\SocketLite.Android.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\main\CrossPlatform\SocketLite.iOS\SocketLite.iOS.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\main\CrossPlatform\SocketLite.UWP\SocketLite.UWP.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\main\CrossPlatform\SocketLite.net451\SocketLite.net451.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\main\CrossPlatform\SocketLite.NetStandard13\SocketLite.NetStandard13.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\main\CrossPlatform\SocketLite.NetStandard15\SocketLite.NetStandard15.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\main\CrossPlatform\SocketLite.NetStandard20\SocketLite.NetStandard20.csproj /t:Build /p:Configuration="Release"


#$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\interface\ISocketLite.Netstandard\bin\Release\netstandard1.0\ISocketLite.PCL.dll')).Version.ToString(3) + $betaver


Remove-Item .\NuGet -Force -Recurse
New-Item -ItemType Directory -Force -Path .\NuGet
NuGet.exe pack SocketLite.nuspec -Verbosity detailed -Symbols -OutputDir "NuGet" -Version $version