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

function DownloadJre{
    $ErrorActionPreference = "Stop"
        
    
    $doc = New-Object HtmlAgilityPack.HtmlDocument
    $WebResponse = Invoke-WebRequest "http://jdk.java.net/12/"
    $doc.LoadHtml($WebResponse.Content)
    $downloadUrl =  $doc.DocumentNode.SelectSingleNode("//a[contains(@href, 'windows')]").Attributes["href"].Value
    
    Write-Host "Downloading " $downloadUrl

    $downloadPage = 'http://www.oracle.com/'

    $bak = $ProgressPreference 
    $ProgressPreference = "SilentlyContinue"
    Invoke-WebRequest $downloadPage -UseBasicParsing -UseDefaultCredentials -SessionVariable s | Out-Null
    $c = New-Object System.Net.Cookie("oraclelicense", "accept-securebackup-cookie", "/", ".oracle.com")
    $s.Cookies.Add($downloadPage, $c)
    Invoke-WebRequest $downloadUrl -UseBasicParsing -UseDefaultCredentials -WebSession $s -OutFile .\temp\jre.zip
    $ProgressPreference = $bak
    Write-Host "done." -ForegroundColor Green

    Write-Host "Extracting Java ..."

    .\tools\7z.exe x .\temp\jre.zip -otemp\
    #.\tools\7z.exe x .\temp\*.tar -otemp\
}

if ($PSVersionTable.PSVersion.Major -lt 3) {
    throw "Powershell v3 or greater is required."
}

Remove-Item ..\source\ElasticsearchInside\Executables\*.lz4

DownloadJre

$jreDir = Get-ChildItem -Recurse $directory | Where-Object { $_.PSIsContainer -and `
   $_.Name.StartsWith("jdk") } | Select-Object -First 1

Write-Host "Encoding file " $jreDir.Fullname
.\tools\LZ4Encoder.exe  $jreDir.Fullname ..\source\ElasticsearchInside\Executables\jre.lz4


DownloadElasticsearch
$elasticDir = Get-ChildItem -Recurse $directory | Where-Object { $_.PSIsContainer -and `
    $_.Name.StartsWith("elasticsearch") } | Select-Object -First 1

Write-Host "Encoding file " $elasticDir.Fullname

.\tools\LZ4Encoder.exe $elasticDir.Fullname ..\source\ElasticsearchInside\Executables\elasticsearch.lz4


Remove-Item temp -Force -Recurse 

