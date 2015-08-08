@echo off
del *.nupkg
tools\nuget pack ..\source\ElasticsearchInside\ElasticsearchInside.csproj
tools\nuget push *.nupkg
