param([switch]$Apply)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public static class MonsterCardPalette
{
    // 기존 gen_moncards.py의 밝기·채도·RGB 곱을 동일하게 적용한다. 알파는 보존한다.
    public static void Apply(Bitmap image, double brightness, double saturation, double red, double green, double blue)
    {
        var rect = new Rectangle(0, 0, image.Width, image.Height);
        var data = image.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        try
        {
            var bytes = new byte[data.Stride * data.Height];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            for (int y = 0; y < data.Height; y++)
            for (int x = 0; x < image.Width; x++)
            {
                int offset = y * data.Stride + x * 4;
                double luminance = (bytes[offset] + bytes[offset + 1] + bytes[offset + 2]) / 3.0;
                bytes[offset] = Channel(bytes[offset], luminance, saturation, brightness * blue);
                bytes[offset + 1] = Channel(bytes[offset + 1], luminance, saturation, brightness * green);
                bytes[offset + 2] = Channel(bytes[offset + 2], luminance, saturation, brightness * red);
            }
            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
        }
        finally { image.UnlockBits(data); }
    }

    private static byte Channel(byte value, double luminance, double saturation, double multiplier)
    {
        return (byte)Math.Max(0, Math.Min(255, (luminance + (value - luminance) * saturation) * multiplier));
    }
}
'@
$projectRoot = Split-Path -Parent $PSScriptRoot
$docsRoot = Join-Path $projectRoot 'Documents/MonsterRemaster'
$plan = Get-Content -LiteralPath (Join-Path $docsRoot 'frame-plan.json') -Raw -Encoding UTF8 | ConvertFrom-Json
if ($plan.failures.Count -gt 0) { throw 'Resolve frame-plan failures before exporting.' }
$targetRoot = if ($Apply) { Join-Path $projectRoot 'Assets/@Resources/Sprite/Monster/Anim' } else { Join-Path $docsRoot 'preview-frames' }

# 시트가 바뀌었으면 기존 사각형으로 자르지 않는다. 쓰기 전에 모든 입력을 확인한다.
foreach ($entry in $plan.entries)
{
    $sourceHash = (Get-FileHash -LiteralPath (Join-Path $docsRoot $entry.source) -Algorithm SHA256).Hash
    if ($sourceHash -ne $entry.source_sha256) { throw "Sheet changed; rerun inspect_monster_sheets.py: $($entry.id)" }
    if ($Apply)
    {
        foreach ($frame in $entry.frames)
        {
            $target = Join-Path (Join-Path $targetRoot $entry.id) ($frame.state + '_' + $frame.index + '.png')
            if (-not (Test-Path -LiteralPath ($target + '.meta'))) { throw "Missing existing GUID: $target" }
        }
    }
}
if ($Apply)
{
    foreach ($icon in $plan.icons)
    {
        $target = Join-Path (Join-Path $projectRoot 'Assets/@Resources/Sprite/Monster') $icon.target
        if (-not (Test-Path -LiteralPath ($target + '.meta'))) { throw "Missing existing GUID: $target" }
    }
}

# 생성 모델의 PNG를 검사된 사각형으로 분리한다. 원본 알파와 몬스터당 공통 배율을 유지한다.
foreach ($entry in $plan.entries)
{
    $source = [System.Drawing.Bitmap]::FromFile((Join-Path $docsRoot $entry.source))
    try
    {
        $targetDir = Join-Path $targetRoot $entry.id
        if ($Apply -and -not (Test-Path -LiteralPath $targetDir)) { throw "Missing existing asset directory: $targetDir" }
        [System.IO.Directory]::CreateDirectory($targetDir) | Out-Null
        foreach ($frame in $entry.frames)
        {
            $target = Join-Path $targetDir ($frame.state + '_' + $frame.index + '.png')
            if ($Apply -and -not (Test-Path -LiteralPath ($target + '.meta'))) { throw "Missing existing GUID: $target" }
            $bitmap = [System.Drawing.Bitmap]::new([int]$plan.canvas[0], [int]$plan.canvas[1], [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
            $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
            try
            {
                $graphics.Clear([System.Drawing.Color]::Transparent)
                $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
                $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
                $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
                $w = [int]$frame.size[0]
                $h = [int]$frame.size[1]
                $dest = [System.Drawing.Rectangle]::new([int][Math]::Floor(($bitmap.Width - $w) / 2), $bitmap.Height - $h, $w, $h)
                $rect = [System.Drawing.Rectangle]::new([int]$frame.rect[0], [int]$frame.rect[1], [int]$frame.rect[2], [int]$frame.rect[3])
                $graphics.DrawImage($source, $dest, $rect, [System.Drawing.GraphicsUnit]::Pixel)
                $bitmap.Save($target, [System.Drawing.Imaging.ImageFormat]::Png)
            }
            finally
            {
                $graphics.Dispose()
                $bitmap.Dispose()
            }
        }
        Write-Output ($entry.id + ': 20 frames')
    }
    finally { $source.Dispose() }
}

# 도감도 같은 Idle 그림에서 파생한다. 기존 PNG와 GUID를 유지한다.
$iconRoot = if ($Apply) { Join-Path $projectRoot 'Assets/@Resources/Sprite/Monster' } else { Join-Path $docsRoot 'preview-icons' }
foreach ($icon in $plan.icons)
{
    $entry = $plan.entries | Where-Object id -EQ $icon.id | Select-Object -First 1
    $frame = $entry.frames[0]
    $source = [System.Drawing.Bitmap]::FromFile((Join-Path $docsRoot $entry.source))
    $size = [int]$icon.size
    $bitmap = [System.Drawing.Bitmap]::new($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try
    {
        $target = Join-Path $iconRoot $icon.target
        if ($Apply -and -not (Test-Path -LiteralPath ($target + '.meta'))) { throw "Missing existing GUID: $target" }
        [System.IO.Directory]::CreateDirectory((Split-Path -Parent $target)) | Out-Null
        $scale = $size / [Math]::Max([int]$frame.rect[2], [int]$frame.rect[3])
        $w = [Math]::Max(1, [int][Math]::Floor($frame.rect[2] * $scale))
        $h = [Math]::Max(1, [int][Math]::Floor($frame.rect[3] * $scale))
        $y = if ($icon.bottom) { $size - $h } else { [int][Math]::Floor(($size - $h) / 2) }
        $graphics.Clear([System.Drawing.Color]::Transparent)
        $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
        $dest = [System.Drawing.Rectangle]::new([int][Math]::Floor(($size - $w) / 2), $y, $w, $h)
        $rect = [System.Drawing.Rectangle]::new([int]$frame.rect[0], [int]$frame.rect[1], [int]$frame.rect[2], [int]$frame.rect[3])
        $graphics.DrawImage($source, $dest, $rect, [System.Drawing.GraphicsUnit]::Pixel)
        $graphics.Dispose()
        $graphics = $null
        $grade = $icon.grade
        [MonsterCardPalette]::Apply($bitmap, $grade[0], $grade[1], $grade[2][0], $grade[2][1], $grade[2][2])
        $bitmap.Save($target, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally
    {
        if ($null -ne $graphics) { $graphics.Dispose() }
        $bitmap.Dispose()
        $source.Dispose()
    }
}
Write-Output ('Icons: ' + $plan.icons.Count)
