$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..\..\..')
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$output = Join-Path $PSScriptRoot 'On-Together_Day_and_Night.dll'
$stagingDirectory = Join-Path $root '.build\On-Together_Day_and_Night'
$stagedOutput = Join-Path $stagingDirectory 'On-Together_Day_and_Night.dll'
$source = Join-Path $PSScriptRoot 'src\DayAndNightPlugin.cs'

$references = @(
    (Join-Path $root 'BepInEx\core\BepInEx.dll'),
    (Join-Path $root 'OnTogether_Data\Managed\netstandard.dll'),
    (Join-Path $root 'OnTogether_Data\Managed\UnityEngine.dll'),
    (Join-Path $root 'OnTogether_Data\Managed\UnityEngine.CoreModule.dll'),
    (Join-Path $root 'OnTogether_Data\Managed\UnityEngine.InputLegacyModule.dll'),
    (Join-Path $root 'OnTogether_Data\Managed\UnityEngine.IMGUIModule.dll'),
    (Join-Path $root 'OnTogether_Data\Managed\UnityEngine.TextRenderingModule.dll'),
    (Join-Path $root 'OnTogether_Data\Managed\UnityEngine.UIModule.dll'),
    (Join-Path $root 'OnTogether_Data\Managed\UnityEngine.AudioModule.dll'),
    (Join-Path $root 'OnTogether_Data\Managed\UnityEngine.UnityWebRequestModule.dll'),
    (Join-Path $root 'OnTogether_Data\Managed\UnityEngine.UnityWebRequestAudioModule.dll'),
    (Join-Path $root 'OnTogether_Data\Managed\UnityEngine.ParticleSystemModule.dll'),
    (Join-Path $root 'OnTogether_Data\Managed\UnityEngine.PhysicsModule.dll'),
    (Join-Path $root 'OnTogether_Data\Managed\Unity.RenderPipelines.Core.Runtime.dll'),
    (Join-Path $root 'OnTogether_Data\Managed\Unity.RenderPipelines.Universal.Runtime.dll'),
    (Join-Path $root 'OnTogether_Data\Managed\Unity.RenderPipelines.GPUDriven.Runtime.dll'),
    (Join-Path $root 'OnTogether_Data\Managed\sc.stylizedwater3.runtime.dll'),
    (Join-Path $root 'OnTogether_Data\Managed\PurrNet.Runtime.dll'),
    (Join-Path $root 'OnTogether_Data\Managed\Assembly-CSharp.dll')
)

New-Item -ItemType Directory -Force -Path $stagingDirectory | Out-Null
& $compiler /nologo /target:library /optimize+ /out:$stagedOutput ($references | ForEach-Object { '/reference:' + $_ }) $source
if ($LASTEXITCODE -ne 0) { throw "csc failed with exit code $LASTEXITCODE" }

try {
    Copy-Item -LiteralPath $stagedOutput -Destination $output -Force
}
catch {
    throw "Build passed, but the game has locked $output. Close On-Together and run build.ps1 again. Staged DLL: $stagedOutput"
}
Write-Host "Built $output"
