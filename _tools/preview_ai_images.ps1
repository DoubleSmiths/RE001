$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$projectRoot = Split-Path -Parent $PSScriptRoot
$auditRoot = Join-Path $projectRoot 'Documents/AIImageAudit'
$inventory = Get-Content -LiteralPath (Join-Path $auditRoot 'inventory.json') -Raw -Encoding UTF8 | ConvertFrom-Json
foreach ($group in ($inventory.entries | Group-Object group))
{
    $items = $group.Group
    $perPage = 24
    for ($offset = 0; $offset -lt $items.Count; $offset += $perPage)
    {
        $count = [Math]::Min($perPage, $items.Count - $offset)
        $rows = [int][Math]::Ceiling($count / 4)
        $canvas = [System.Drawing.Bitmap]::new(1280, $rows * 220)
        $graphics = [System.Drawing.Graphics]::FromImage($canvas)
        $font = [System.Drawing.Font]::new('Arial', 11)
        try
        {
            $graphics.Clear([System.Drawing.Color]::FromArgb(225, 230, 235))
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
            $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
            for ($index = 0; $index -lt $count; $index++)
            {
                $item = $items[$offset + $index]
                $source = [System.Drawing.Bitmap]::FromFile((Join-Path $projectRoot $item.path))
                try
                {
                    $x = ($index % 4) * 320
                    $y = [int][Math]::Floor($index / 4) * 220
                    $scale = [Math]::Min(300 / $source.Width, 176 / $source.Height)
                    $w = [int]($source.Width * $scale)
                    $h = [int]($source.Height * $scale)
                    $graphics.DrawString($item.name, $font, [System.Drawing.Brushes]::Black, $x + 8, $y + 4)
                    $dest = [System.Drawing.Rectangle]::new($x + [int](160 - $w / 2), $y + 32 + [int]((176 - $h) / 2), $w, $h)
                    $graphics.DrawImage($source, $dest)
                }
                finally { $source.Dispose() }
            }
            $name = $group.Name + '-' + ([int]($offset / $perPage) + 1) + '.png'
            $canvas.Save((Join-Path $auditRoot $name), [System.Drawing.Imaging.ImageFormat]::Png)
        }
        finally { $graphics.Dispose(); $font.Dispose(); $canvas.Dispose() }
    }
}
