rem SET Version=1.12.6-alpha
rem SET Version=1.12.6-beta
rem SET Version=1.12.6-rc1-20200530

SET Version=4.5.7

C:\prog_data\_Tools\NuGet\nuget push -Source "Alcazar (vs.com)" -ApiKey my-key C:\prog\_bin\Pack\Alcazar.Common.%Version%.nupkg
C:\prog_data\_Tools\NuGet\nuget push -Source "Alcazar (vs.com)" -ApiKey my-key C:\prog\_bin\Pack\Alcazar.Common.*.%Version%.nupkg
C:\prog_data\_Tools\NuGet\nuget push -Source "Alcazar (vs.com)" -ApiKey my-key C:\prog\_bin\Pack\Alcazar.Data.*.%Version%.nupkg
C:\prog_data\_Tools\NuGet\nuget push -Source "Alcazar (vs.com)" -ApiKey my-key C:\prog\_bin\Pack\Alcazar.Document.*.%Version%.nupkg
C:\prog_data\_Tools\NuGet\nuget push -Source "Alcazar (vs.com)" -ApiKey my-key C:\prog\_bin\Pack\Alcazar.Web.%Version%.nupkg
C:\prog_data\_Tools\NuGet\nuget push -Source "Alcazar (vs.com)" -ApiKey my-key C:\prog\_bin\Pack\Alcazar.Web.Mvc.%Version%.nupkg
C:\prog_data\_Tools\NuGet\nuget push -Source "Alcazar (vs.com)" -ApiKey my-key C:\prog\_bin\Pack\Alcazar.Web.Mvc.*.%Version%.nupkg
  
C:\prog_data\_Tools\NuGet\nuget push -Source "Alcazar (vs.com)" -ApiKey my-key C:\prog\_bin\Pack\Alcazar.Web.Content.%Version%.nupkg
C:\prog_data\_Tools\NuGet\nuget push -Source "Alcazar (vs.com)" -ApiKey my-key C:\prog\_bin\Pack\Alcazar.Web.Content.Met*.%Version%.nupkg
rem C:\prog_data\_Tools\NuGet\nuget push -Source "Alcazar (visualstudio.com)" -ApiKey my-key C:\prog\_bin\Pack\Alcazar.Web.Content.Static.%Version%.nupkg

rem Alcazar.Template.* - obsolete
rem C:\prog_data\_Tools\NuGet\nuget push -Source "Alcazar (visualstudio.com)" -ApiKey my-key C:\prog\_bin\Pack\Alcazar.Template.*.%Version%.nupkg
pause
