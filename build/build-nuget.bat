@echo off
cd %1
cd ..
del *.nupkg
build\tools\nuget.exe pack -exclude **\LZ4PCL.dll source\ElasticsearchInside\ElasticsearchInside.csproj
