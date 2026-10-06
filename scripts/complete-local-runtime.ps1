param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '..\src\DeskBox\bin\Debug\net10.0-windows10.0.22621.0')
)

$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $OutputDirectory -PathType Container)) {
    throw "Build DeskBox before completing its private runtime: $OutputDirectory"
}

# Match the retail publish workaround for the resource DLL omitted by the
# Foundation self-contained payload. Always use the restore-locked runtime MSIX.
$assetsPath = Join-Path $PSScriptRoot '..\src\DeskBox\obj\project.assets.json'
$assets = Get-Content -LiteralPath $assetsPath -Raw | ConvertFrom-Json
$runtime = @($assets.libraries.PSObject.Properties | Where-Object Name -Like 'Microsoft.WindowsAppSDK.Runtime/*')
if ($runtime.Count -ne 1) { throw 'Expected one restored Windows App Runtime package.' }
$archivePath = $null
foreach ($packageRoot in $assets.packageFolders.PSObject.Properties.Name) {
    $candidate = Join-Path $packageRoot ($runtime[0].Value.path + '/tools/MSIX/win10-x64/Microsoft.WindowsAppRuntime.2.msix')
    if (Test-Path -LiteralPath $candidate -PathType Leaf) { $archivePath = $candidate; break }
}
if (-not $archivePath) { throw 'The restored x64 Windows App Runtime MSIX is missing.' }

$fileName = 'Microsoft.WindowsAppRuntime.Insights.Resource.dll'
$archive = [System.IO.Compression.ZipFile]::OpenRead($archivePath)
try {
    $entry = $archive.GetEntry($fileName)
    if ($null -eq $entry) { throw "Runtime resource missing: $fileName" }
    [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, (Join-Path $OutputDirectory $fileName), $true)
}
finally { $archive.Dispose() }
