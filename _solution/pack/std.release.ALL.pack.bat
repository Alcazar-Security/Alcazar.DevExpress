rem SET Version=1.12.6-alpha
rem SET Version=1.12.6-beta
rem SET Version=1.12.6-rc1-20200530
rem SET Version=2.1.5-D-3
rem SET Config=Debug
rem SET Config=Release

SET Version=1.0.1
SET Config=Debug

rem include this to force dependent packages to record this version as the required version
rem     <PackageVersion Condition="'$(PackageVersion)' == ''">1.25.0</PackageVersion>

rem + dotnet pack uses proprties in the csproj file
rem - nuget does not handle the icon, license, and only includes dependencies:any (uses properties from the package page)
rem All Standard 2.0 assemblies are built with nuget pack, from the csproj, and they have no nuspec

rem (1) Alcazar.DevExpress
dotnet pack C:\prog\Alcazar\Alcazar.DevExpress\Alcazar.DevExpress.TagHelpers\Alcazar.DevExpress.TagHelpers.csproj --configuration %Config% --no-build -p:Version=%Version% -p:NuspecBasePath=C:\prog\Alcazar\Alcazar.DevExpress\_solution -p:PackageOutputPath=C:\prog\_bin\Pack

pause
