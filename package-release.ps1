$ErrorActionPreference = 'Stop'

$gameRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..\..')

# ---------------------------------------------------------------------------
# Read the version from the manifest rather than repeating it here. The paths
# below used to hard-code 1.0.0, so bumping the manifest silently produced a
# zip still named after the previous release.
# ---------------------------------------------------------------------------
$manifestPath = Join-Path $PSScriptRoot 'manifest.json'
$manifest = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json

# Thunderstore rejects a package that breaks any of these, and its error
# messages are not always obvious, so check them here where the fix is nearby.
if ($manifest.name -notmatch '^[a-zA-Z0-9_]+$') {
    throw "manifest name '$($manifest.name)' must be letters, digits and underscores only."
}
if ($manifest.version_number -notmatch '^\d+\.\d+\.\d+$') {
    throw "manifest version_number '$($manifest.version_number)' must be major.minor.patch."
}
if ($manifest.description.Length -gt 250) {
    throw "manifest description is $($manifest.description.Length) characters; the limit is 250."
}
foreach ($dependency in $manifest.dependencies) {
    if ($dependency -notmatch '^[a-zA-Z0-9_]+-[a-zA-Z0-9_]+-\d+\.\d+\.\d+$') {
        throw "dependency '$dependency' must be Namespace-Name-Major.Minor.Patch."
    }
}

# icon.png must be exactly 256x256 or the upload is refused. PNG stores the
# dimensions big-endian at byte 16; reverse the ranges to read them here.
$iconPath = Join-Path $PSScriptRoot 'icon.png'
$iconBytes = [System.IO.File]::ReadAllBytes($iconPath)
$iconWidth = [System.BitConverter]::ToUInt32(($iconBytes[19..16]), 0)
$iconHeight = [System.BitConverter]::ToUInt32(($iconBytes[23..20]), 0)
if ($iconWidth -ne 256 -or $iconHeight -ne 256) {
    throw "icon.png is ${iconWidth}x${iconHeight}; Thunderstore requires exactly 256x256."
}

$version = $manifest.version_number
$packageName = "On-Together_Day_and_Night_$version"

# Never stage a release below BepInEx/plugins: BepInEx recursively scans that tree
# and would load the packaged DLL as a second copy of the same plugin.
$releaseRoot = Join-Path $gameRoot '.build\releases\On-Together_Day_and_Night'
$packageRoot = Join-Path $releaseRoot $packageName
$zipPath = Join-Path $releaseRoot "$packageName.zip"

# ---------------------------------------------------------------------------
# Layout. A mod manager places a package by matching the folders inside the zip
# against an install-rule tree whose root is BepInEx - BepInEx/plugins,
# BepInEx/patchers, BepInEx/config and so on. A bare top-level plugins/ folder
# matches nothing in that tree, so the manager cannot place it and leaves it
# where it fell. That is why the mod had to be moved into the plugins folder by
# hand after installing. Shipping the full BepInEx/plugins path fixes the
# managers, and also makes manual installation a straight drag of BepInEx onto
# the game folder.
# ---------------------------------------------------------------------------
$pluginDirectory = Join-Path $packageRoot 'BepInEx\plugins\On-Together_Day_and_Night'

New-Item -ItemType Directory -Force -Path $releaseRoot | Out-Null
Remove-Item -LiteralPath $packageRoot -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $zipPath -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $pluginDirectory | Out-Null

Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'On-Together_Day_and_Night.dll') -Destination $pluginDirectory
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'audio') -Destination $pluginDirectory -Recurse
Copy-Item -LiteralPath $manifestPath -Destination $packageRoot
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.md') -Destination $packageRoot
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'CHANGELOG.md') -Destination $packageRoot
Copy-Item -LiteralPath $iconPath -Destination $packageRoot

# Compress-Archive writes Windows backslashes into ZIP entry names. Some Linux archive
# tools and Thunderstore clients treat those as literal filename characters, flattening
# the folders into the package root. Write canonical ZIP paths.
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::Open(
    $zipPath, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    Get-ChildItem -LiteralPath $packageRoot -Recurse -File | ForEach-Object {
        $relativePath = $_.FullName.Substring($packageRoot.Length).TrimStart('\', '/')
        $entryName = $relativePath.Replace('\', '/')
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
            $archive, $_.FullName, $entryName,
            [System.IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
}
finally {
    $archive.Dispose()
}

$requiredEntries = @(
    'manifest.json',
    'README.md',
    'CHANGELOG.md',
    'icon.png',
    'BepInEx/plugins/On-Together_Day_and_Night/On-Together_Day_and_Night.dll',
    'BepInEx/plugins/On-Together_Day_and_Night/audio/THIRD_PARTY_AUDIO.md',
    'BepInEx/plugins/On-Together_Day_and_Night/audio/noon_cicadas_cc0.ogg',
    'BepInEx/plugins/On-Together_Day_and_Night/audio/night_crickets_frogs_cc0.ogg'
)
$verificationArchive = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $entryNames = @($verificationArchive.Entries | ForEach-Object { $_.FullName })
    $badEntries = @($entryNames | Where-Object { $_.Contains('\') })
    if ($badEntries.Count -gt 0) {
        throw "ZIP contains non-portable backslash entries: $($badEntries -join ', ')"
    }
    foreach ($requiredEntry in $requiredEntries) {
        if ($entryNames -notcontains $requiredEntry) {
            throw "ZIP is missing required entry: $requiredEntry"
        }
    }
    # Anything outside the root files and the BepInEx tree is something the
    # manager has no rule for, which is exactly the failure being fixed here.
    $strays = @($entryNames | Where-Object {
        $_ -notmatch '^(manifest\.json|README\.md|CHANGELOG\.md|icon\.png)$' -and
        $_ -notmatch '^BepInEx/'
    })
    if ($strays.Count -gt 0) {
        throw "ZIP has entries no mod manager can place: $($strays -join ', ')"
    }
    Write-Host "ZIP entries:"
    $entryNames | Sort-Object | ForEach-Object { Write-Host "  $_" }
}
finally {
    $verificationArchive.Dispose()
}
Write-Host ""
Write-Host "Packaged $zipPath"
