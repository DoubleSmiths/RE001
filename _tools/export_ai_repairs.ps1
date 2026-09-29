param([switch]$Apply)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$projectRoot = Split-Path -Parent $PSScriptRoot
$docsRoot = Join-Path $projectRoot 'Documents/AIImageAudit'
$plan = Get-Content -LiteralPath (Join-Path $docsRoot 'export-plan.json') -Raw -Encoding UTF8 | ConvertFrom-Json
foreach ($entry in $plan.entries)
{
    $source = Join-Path $docsRoot $entry.source
    $meta = (Join-Path $projectRoot $entry.target) + '.meta'
    if ((Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash -ne $entry.source_sha256) { throw "Source changed: $source" }
    if ((Get-FileHash -LiteralPath $meta -Algorithm SHA256).Hash -ne $entry.meta_sha256) { throw "Importer changed: $meta" }
}
foreach ($entry in $plan.entries)
{
    $source = [System.Drawing.Bitmap]::FromFile((Join-Path $docsRoot $entry.source))
    $image = [System.Drawing.Bitmap]::new([int]$entry.size[0], [int]$entry.size[1], [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($image)
    try
    {
        $target = Join-Path $projectRoot $entry.target
        $before = Join-Path $docsRoot ('before/' + $entry.id + '.png')
        if ($Apply -and -not (Test-Path -LiteralPath $before))
        {
            [System.IO.Directory]::CreateDirectory((Split-Path -Parent $before)) | Out-Null
            Copy-Item -LiteralPath $target -Destination $before
        }
        if (-not $Apply) { $target = Join-Path $docsRoot ('preview/' + $entry.id + '.png') }
        [System.IO.Directory]::CreateDirectory((Split-Path -Parent $target)) | Out-Null
        $graphics.Clear([System.Drawing.Color]::Transparent)
        $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
        $r = $entry.rect
        $d = $entry.dest
        $rect = [System.Drawing.Rectangle]::new([int]$r[0], [int]$r[1], [int]$r[2], [int]$r[3])
        $dest = [System.Drawing.Rectangle]::new([int]$d[0], [int]$d[1], [int]$d[2], [int]$d[3])
        $graphics.DrawImage($source, $dest, $rect, [System.Drawing.GraphicsUnit]::Pixel)
        $image.Save($target, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally { $graphics.Dispose(); $image.Dispose(); $source.Dispose() }
}
Write-Output ('Exported: ' + $plan.entries.Count)
