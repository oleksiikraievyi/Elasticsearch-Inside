Push-Location $PSScriptRoot

$htmlAbilityPackDllPath = Join-Path $PSScriptRoot ".\tools\HtmlAgilityPack.dll"
[System.Reflection.Assembly]::UnsafeLoadFrom($htmlAbilityPackDllPath)
# Add-Type -Path .\tools\HtmlAgilityPack.dll

$tempDir = ".\temp"

if (!(Test-Path $tempDir)) {
    md $tempDir | Out-Null
}

function DownloadElasticsearch {
    $ErrorActionPreference = "Stop"
    
    $doc = New-Object HtmlAgilityPack.HtmlDocument

    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

    $WebResponse = Invoke-WebRequest "https://www.elastic.co/downloads/elasticsearch"

    $doc.LoadHtml($WebResponse.Content)

    $url = $doc.DocumentNode.SelectSingleNode("//a[starts-with(@class, 'zip-link')]");

    # change this line to download a specific version
    $downloadUrl = "https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-7.1.0-windows-x86_64.zip" # $url.Attributes["href"].Value
    
    Write-Host "Downloading " $downloadUrl

    $bak = $ProgressPreference 
    $ProgressPreference = "SilentlyContinue"
    Invoke-WebRequest $downloadUrl -UseBasicParsing -UseDefaultCredentials -WebSession $s -OutFile .\temp\es.zip
    $ProgressPreference = $bak
    Write-Host "done." -ForegroundColor Green

    Write-Host "Extracting Elasticsearch..."


    
    .\tools\7z.exe x .\temp\es.zip -otemp\
    
    Write-Host "done." -ForegroundColor Green

}

if ($PSVersionTable.PSVersion.Major -lt 3) {
    throw "Powershell v3 or greater is required."
}

Remove-Item ..\source\ElasticsearchInside\Executables\*.lz4

DownloadElasticsearch
$elasticDir = Get-ChildItem -Recurse $directory | Where-Object { $_.PSIsContainer -and `
    $_.Name.StartsWith("elasticsearch") } | Select-Object -First 1

Write-Host "Encoding file " $elasticDir.Fullname

.\tools\LZ4Encoder.exe $elasticDir.Fullname ..\source\ElasticsearchInside\Executables\elasticsearch.lz4


Remove-Item temp -Force -Recurse 

