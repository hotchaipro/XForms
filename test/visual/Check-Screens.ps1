# Compares key screens of the Windows test app with saved pictures. Run it before
# committing a change that could show. It needs the desktop to itself for a minute: it
# brings the app's window to the front.
#
#     dotnet build test/XForma.Test.Win32
#     test/visual/Check-Screens.ps1            # compares
#     test/visual/Check-Screens.ps1 -Update    # saves new pictures, after looking at them
#
# Differences are saved to test/visual/differences, beside what's expected.

param(
    [switch]$Update,
    [string[]]$Pages = @('forms', 'pickers', 'tabs', 'dragdrop', 'lists'),
    [int]$Tolerance = 24,
    [double]$AllowedShare = 0.005
)

Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class CheckScreensWindow {
    [DllImport("user32.dll")] public static extern bool GetClientRect(IntPtr h, out RECT r);
    [DllImport("user32.dll")] public static extern bool ClientToScreen(IntPtr h, ref POINT p);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    public struct RECT { public int L, T, R, B; }
    public struct POINT { public int X, Y; }
}
"@
[CheckScreensWindow]::SetProcessDPIAware() | Out-Null

$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$app = Join-Path $root 'test\XForma.Test.Win32\bin\Debug\net10.0-windows\win-x64\XForma.Test.Win32.exe'
$baselines = Join-Path $PSScriptRoot 'baselines'
$differences = Join-Path $PSScriptRoot 'differences'
New-Item -ItemType Directory -Force $baselines, $differences | Out-Null

function Get-Screen([string]$page) {
    $env:XFORMA_TEST_PAGE = $page
    $process = Start-Process -FilePath $app -PassThru
    try {
        Start-Sleep -Seconds 5
        $process.Refresh()
        $window = $process.MainWindowHandle
        [CheckScreensWindow]::SetForegroundWindow($window) | Out-Null
        Start-Sleep -Milliseconds 600
        $client = New-Object CheckScreensWindow+RECT
        [CheckScreensWindow]::GetClientRect($window, [ref]$client) | Out-Null
        $origin = New-Object CheckScreensWindow+POINT
        [CheckScreensWindow]::ClientToScreen($window, [ref]$origin) | Out-Null
        $bitmap = New-Object System.Drawing.Bitmap ($client.R), ($client.B)
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        $graphics.CopyFromScreen($origin.X, $origin.Y, 0, 0, $bitmap.Size)
        $graphics.Dispose()
        return $bitmap
    }
    finally {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }
}

function Get-DifferentShare($actual, $expected) {
    if (($actual.Width -ne $expected.Width) -or ($actual.Height -ne $expected.Height)) {
        return 1.0
    }

    $different = 0
    for ($y = 0; $y -lt $actual.Height; $y += 2) {
        for ($x = 0; $x -lt $actual.Width; $x += 2) {
            $a = $actual.GetPixel($x, $y)
            $b = $expected.GetPixel($x, $y)
            if ([Math]::Max([Math]::Max([Math]::Abs($a.R - $b.R), [Math]::Abs($a.G - $b.G)), [Math]::Abs($a.B - $b.B)) -gt $Tolerance) {
                $different += 1
            }
        }
    }

    return $different / [double](($actual.Width / 2) * ($actual.Height / 2))
}

$failed = 0
foreach ($page in $Pages) {
    $path = Join-Path $baselines "$page.png"
    $actual = Get-Screen $page
    if ($Update -or -not (Test-Path $path)) {
        $actual.Save($path)
        "$page`: saved"
        continue
    }

    $expected = [System.Drawing.Bitmap]::FromFile($path)
    $share = Get-DifferentShare $actual $expected
    $expected.Dispose()
    if ($share -gt $AllowedShare) {
        $failed += 1
        $actual.Save((Join-Path $differences "$page.png"))
        "$page`: DIFFERENT ({0:P2} of the screen); see test\visual\differences\$page.png" -f $share
    }
    else {
        "$page`: same"
    }
}

if ($failed -gt 0) {
    exit 1
}
