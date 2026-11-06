# Easel installer (Windows 10/11, x64)
#   irm https://g3993.github.io/easel/install.ps1 | iex
#
# Downloads the latest Easel Studio build (portable, shader library bundled),
# installs it to %LOCALAPPDATA%\Programs\Easel and adds Desktop + Start Menu
# shortcuts. Re-run any time to update. Your projects and settings are kept.
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"   # Invoke-WebRequest is 10x faster without the bar

$repo = "G3993/easel"
$url  = "https://github.com/$repo/releases/latest/download/Easel-windows-x64.zip"
$dest = Join-Path $env:LOCALAPPDATA "Programs\Easel"
$tmp  = Join-Path ([IO.Path]::GetTempPath()) ("easel-" + [guid]::NewGuid())
New-Item -ItemType Directory -Force $tmp | Out-Null

try {
    Write-Host "==> Downloading Easel (latest)..."
    Invoke-WebRequest -Uri $url -OutFile "$tmp\Easel.zip" -UseBasicParsing

    Write-Host "==> Installing to $dest"
    Get-Process Easel -ErrorAction SilentlyContinue | Stop-Process -Force
    Expand-Archive "$tmp\Easel.zip" -DestinationPath "$tmp\x" -Force
    $src = Get-ChildItem "$tmp\x" -Recurse -Filter Easel.exe | Select-Object -First 1
    if (-not $src) { throw "Easel.exe not found in the download" }
    if (Test-Path $dest) { Remove-Item $dest -Recurse -Force }
    New-Item -ItemType Directory -Force (Split-Path $dest) | Out-Null
    Move-Item $src.DirectoryName $dest
    Get-ChildItem $dest -Recurse | Unblock-File

    $exe = Join-Path $dest "Easel.exe"
    $ws = New-Object -ComObject WScript.Shell
    foreach ($lnk in @(
        (Join-Path ([Environment]::GetFolderPath("Desktop")) "Easel.lnk"),
        (Join-Path ([Environment]::GetFolderPath("Programs")) "Easel.lnk"))) {
        $s = $ws.CreateShortcut($lnk)
        $s.TargetPath = $exe
        $s.WorkingDirectory = $dest
        $s.Save()
    }

    Write-Host ""
    Write-Host "Done. Easel is in $dest (shortcut on the Desktop and in the Start Menu)."
    Write-Host "The ShaderClaw3 shader library is bundled in $dest\ShaderClaw3\shaders."
    Write-Host "NDI is optional: install the NDI Runtime from ndi.video to enable it."
    Start-Process $exe -WorkingDirectory $dest
}
finally {
    Remove-Item $tmp -Recurse -Force -ErrorAction SilentlyContinue
}
