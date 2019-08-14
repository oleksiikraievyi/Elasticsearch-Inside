@echo off
cd %1
cd ..
del *.nupkg
dotnet pack source\ElasticsearchInside\ElasticsearchInside.csproj -c Release
