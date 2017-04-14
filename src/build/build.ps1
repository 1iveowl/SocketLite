param([string]$version)

if ([string]::IsNullOrEmpty($version)) {$version = "0.0.1"}

$msbuild = join-path -path "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin" -childpath "msbuild.exe"

&$msbuild ..\interface\ISocketLite.PCL\ISocketLite.PCL.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\main\SocketLite.Bait\SocketLite.Bait.csproj /t:Build /p:Configuration="Release"

&$msbuild ..\main\CrossPlatform\SocketLite.Android\SocketLite.Android.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\main\CrossPlatform\SocketLite.iOS\SocketLite.iOS.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\main\CrossPlatform\SocketLite.UWP\SocketLite.UWP.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\main\CrossPlatform\SocketLite.net451\SocketLite.net451.csproj /t:Build /p:Configuration="Release"
&$msbuild ..\main\CrossPlatform\SocketLite.Netcore\SocketLite.NetStandard15.csproj /t:Build /p:Configuration="Release"


#$version = [Reflection.AssemblyName]::GetAssemblyName((resolve-path '..\interface\ISocketLite.PCL\bin\Release\ISocketLite.PCL.dll')).Version.ToString(3) + $betaver


Remove-Item .\NuGet -Force -Recurse
New-Item -ItemType Directory -Force -Path .\NuGet
NuGet.exe pack SocketLite.nuspec -Verbosity detailed -Symbols -OutputDir "NuGet" -Version $version