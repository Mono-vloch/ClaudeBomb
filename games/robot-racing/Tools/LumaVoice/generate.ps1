# Turns LUMA's lines into voice files with a Windows text-to-speech voice.
# Normally run from Unity: Claude Bomb > Generate LUMA Voice (it writes lines.json first).
#
#   -LinesJson  { "line-id": "spoken text", ... }  (written by the Unity menu item)
#   -OutDir     where the .wav files go (Assets/Resources/LumaVoice)
#   -Voice      an installed voice, e.g. "Microsoft Zira Desktop"
#
# To use a better voice later (a recorded actor or an online voice service), just
# replace the .wav files with new ones that keep the same names.
param(
    [Parameter(Mandatory = $true)] [string] $LinesJson,
    [Parameter(Mandatory = $true)] [string] $OutDir,
    [string] $Voice = "Microsoft Zira Desktop",
    [int] $Rate = 0
)

Add-Type -AssemblyName System.Speech
$synth = New-Object System.Speech.Synthesis.SpeechSynthesizer
$synth.SelectVoice($Voice)
$synth.Rate = $Rate
$format = New-Object System.Speech.AudioFormat.SpeechAudioFormatInfo(22050, [System.Speech.AudioFormat.AudioBitsPerSample]::Sixteen, [System.Speech.AudioFormat.AudioChannel]::Mono)

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$lines = Get-Content -Raw -Encoding UTF8 $LinesJson | ConvertFrom-Json
foreach ($line in $lines.PSObject.Properties) {
    $path = Join-Path $OutDir ($line.Name + ".wav")
    $synth.SetOutputToWaveFile($path, $format)
    $synth.Speak($line.Value)
}
$synth.SetOutputToNull()
$synth.Dispose()
Write-Output ("Generated {0} voice lines with {1}" -f @($lines.PSObject.Properties).Count, $Voice)
