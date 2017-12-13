@echo off
del *.nupkg
tools\nuget pack -exclude **\LZ4PCL.dll ..\source\ElasticsearchInside\ElasticsearchInside.csproj
tools\nuget push *.nupkg -source https://www.nuget.org
 