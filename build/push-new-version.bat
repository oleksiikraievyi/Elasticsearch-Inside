@echo off
del *.nupkg
dotnet pack ..\source\ElasticsearchInside\ElasticsearchInside.csproj -c Release
tools\nuget push *.nupkg -source https://www.nuget.org -Timeout 999999
