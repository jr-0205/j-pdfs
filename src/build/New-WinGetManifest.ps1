param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^v\d+\.\d+\.\d+$')]
    [string]$Tag,
    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'

# WinGet caps ReleaseNotes at 10,000 characters. Longer notes keep their opening whole lines
# and end with a pointer to the full notes on the GitHub release.
function Get-WinGetReleaseNotes([string]$Body, [string]$ReleaseUrl) {
    $notes = ([string]$Body).Trim()
    if ([string]::IsNullOrWhiteSpace($notes)) { throw 'WinGet requires release notes; the GitHub release body is empty.' }
    $limit = 10000
    if ($notes.Length -le $limit) { return $notes }
    $pointer = "The full release notes are at $ReleaseUrl"
    $budget = $limit - $pointer.Length - 4
    $kept = New-Object System.Collections.Generic.List[string]
    $length = 0
    foreach ($line in ($notes -split '\r?\n')) {
        $next = $length + $line.Length + $(if ($kept.Count -eq 0) { 0 } else { 2 })
        if ($next -gt $budget) { break }
        $kept.Add($line)
        $length = $next
    }
    while ($kept.Count -gt 0 -and [string]::IsNullOrWhiteSpace($kept[$kept.Count - 1])) { $kept.RemoveAt($kept.Count - 1) }
    if ($kept.Count -eq 0) { return $pointer }
    return (($kept -join "`r`n") + "`r`n`r`n" + $pointer)
}

$version = $Tag.Substring(1)
$headers = @{
    Accept = 'application/vnd.github+json'
    'User-Agent' = 'JPDFs-WinGet-Release'
    'X-GitHub-Api-Version' = '2022-11-28'
}
if ($env:GITHUB_TOKEN) { $headers.Authorization = "Bearer $env:GITHUB_TOKEN" }
$release = Invoke-RestMethod "https://api.github.com/repos/JPDFs/JPDFs/releases/tags/$Tag" -Headers $headers
if ($release.draft -or $release.prerelease -or $release.tag_name -ne $Tag) {
    throw 'WinGet requires a published stable release matching the requested tag.'
}
$assets = @($release.assets | Where-Object { $_.name -eq 'JPDFs.exe' })
if ($assets.Count -ne 1) { throw 'The release must contain exactly one JPDFs.exe installer.' }
$url = "https://github.com/JPDFs/JPDFs/releases/download/$Tag/JPDFs.exe"
if ($assets[0].browser_download_url -ne $url) { throw 'Unexpected installer download URL.' }

$directory = [IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path -LiteralPath $directory) { throw 'Use a new output directory to avoid submitting stale manifests.' }
[IO.Directory]::CreateDirectory($directory) | Out-Null
$installerPath = "$directory.exe"
if (Test-Path -LiteralPath $installerPath) { throw 'The installer download path already exists.' }
Invoke-WebRequest -Uri $url -OutFile $installerPath -UseBasicParsing
$hash = (Get-FileHash -LiteralPath $installerPath -Algorithm SHA256).Hash
if ($assets[0].digest -and $assets[0].digest -ne "sha256:$($hash.ToLowerInvariant())") {
    throw 'Downloaded installer does not match the GitHub release digest.'
}
$date = ([datetime]$release.published_at).ToUniversalTime().ToString('yyyy-MM-dd')
$utf8 = New-Object System.Text.UTF8Encoding($false)

# The launcher installs machine-wide with /silent. Its interactive wizard can
# select a user install. WinGet must request elevation for the silent path.
$installer = @"
# Created by the JPDFs release workflow
# yaml-language-server: `$schema=https://aka.ms/winget-manifest.installer.1.12.0.schema.json

PackageIdentifier: JPDFs
PackageVersion: $version
InstallerType: exe
Scope: machine
ElevationRequirement: elevationRequired
InstallModes:
- silent
- silentWithProgress
InstallerSwitches:
  Silent: /silent
  SilentWithProgress: /silent
UpgradeBehavior: install
Dependencies:
  PackageDependencies:
  - PackageIdentifier: Microsoft.DotNet.DesktopRuntime.10
    MinimumVersion: 10.0.0
AppsAndFeaturesEntries:
- DisplayName: JPDFs
  Publisher: J-PDF's
  DisplayVersion: $version
  ProductCode: JPDFs
ReleaseDate: $date
Installers:
- Architecture: x64
  InstallerUrl: $url
  InstallerSha256: $hash
ManifestType: installer
ManifestVersion: 1.12.0
"@
$releaseNotes = Get-WinGetReleaseNotes -Body ([string]$release.body) -ReleaseUrl "https://github.com/JPDFs/JPDFs/releases/tag/$Tag"
$releaseNotes = (($releaseNotes -split '\r?\n') | ForEach-Object { "  $_" }) -join "`r`n"
$locale = @"
# Created by the JPDFs release workflow
# yaml-language-server: `$schema=https://aka.ms/winget-manifest.defaultLocale.1.12.0.schema.json

PackageIdentifier: JPDFs
PackageVersion: $version
PackageLocale: en-US
Publisher: J-PDF's
PublisherUrl: https://github.com/JPDFs
PublisherSupportUrl: https://github.com/JPDFs/JPDFs/issues
Author: J-PDF's
PackageName: JPDFs
PackageUrl: https://github.com/JPDFs/JPDFs
License: GPL-3.0
LicenseUrl: https://github.com/JPDFs/JPDFs/blob/HEAD/LICENSE
Copyright: Copyright (c) 2026 J-PDF's
ShortDescription: PDF editor for Windows. No account, no subscription, no telemetry.
Description: JPDFs is a free, open-source PDF editor for Windows. View, annotate, edit text and paragraphs, OCR, merge, split, sign and verify signatures, fill and build forms, redact, protect, compare, print and flatten PDFs, export them to Word, Excel, PowerPoint, HTML or Markdown, and repeat the work with macros, watched folders and the command line, all without an Adobe subscription. WinGet installs machine-wide; the downloadable installer also supports per-user installation, and a separate portable build is available. Document processing stays on your computer, with no account, no subscription, and no telemetry. Open source under GPLv3.
Moniker: jpdfs
Tags:
- dotnet
- gplv3
- ocr
- opensource
- pdf
- pdf-editor
- portable
- redaction
- windows
- wpf
ReleaseNotes: |-
$releaseNotes
ReleaseNotesUrl: https://github.com/JPDFs/JPDFs/releases/tag/$Tag
ManifestType: defaultLocale
ManifestVersion: 1.12.0
"@
$manifest = @"
# Created by the JPDFs release workflow
# yaml-language-server: `$schema=https://aka.ms/winget-manifest.version.1.12.0.schema.json

PackageIdentifier: JPDFs
PackageVersion: $version
DefaultLocale: en-US
ManifestType: version
ManifestVersion: 1.12.0
"@
[IO.File]::WriteAllText((Join-Path $directory 'JPDFs.installer.yaml'), ($installer -replace '\r?\n', "`r`n"), $utf8)
[IO.File]::WriteAllText((Join-Path $directory 'JPDFs.locale.en-US.yaml'), ($locale -replace '\r?\n', "`r`n"), $utf8)
[IO.File]::WriteAllText((Join-Path $directory 'JPDFs.yaml'), ($manifest -replace '\r?\n', "`r`n"), $utf8)
Write-Host "Generated WinGet manifests for $Tag with SHA256 $hash in $directory"
