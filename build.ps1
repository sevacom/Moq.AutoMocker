$buildDir = "$PSScriptRoot\build"

$latestMsBuildKey = (
       ls -Path HKLM:\software\Microsoft\MSBuild\ToolsVersions\ |
       sort -property @{Expression={0.0 + ($_.Name | Split-Path -Leaf)}} -Descending)[0]
       
$msBuildPath = $latestMsBuildKey.GetValue("MSBuildToolsPath") + "MSBuild.exe"

$propertiesArg = '/p:Configuration=Release'

& $msBuildPath .\Moq.AutoMock.sln $propertiesArg

.\.nuget\NuGet.exe pack ".\Moq.AutoMock\Moq.AutoMock.csproj" -Properties "Configuration=Release;Platform=AnyCPU" -Symbols -IncludeReferencedProjects -OutputDirectory build


