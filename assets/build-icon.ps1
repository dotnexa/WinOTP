#requires -Version 5.1
# WinOTP icon builder — assets/icon.svg tasarımını çoklu boyutlu .ico'ya çevirir.
# Çalıştır: powershell -ExecutionPolicy Bypass -File assets\build-icon.ps1

Add-Type -AssemblyName System.Drawing

$sizes = 16, 24, 32, 48, 64, 128, 256

function New-IconBitmap {
    param([int]$Size)

    $bmp = New-Object System.Drawing.Bitmap($Size, $Size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g   = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode    = 'AntiAlias'
    $g.InterpolationMode = 'HighQualityBicubic'
    $g.PixelOffsetMode  = 'HighQuality'

    # tasarım 256x256 baz alınır, hedef boyuta ölçeklenir
    $scale = $Size / 256.0
    $g.ScaleTransform($scale, $scale)

    # yuvarlak köşeli kare
    $r = 56
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddArc(0,         0,         $r*2, $r*2, 180, 90)
    $path.AddArc(256-$r*2,  0,         $r*2, $r*2, 270, 90)
    $path.AddArc(256-$r*2,  256-$r*2,  $r*2, $r*2,   0, 90)
    $path.AddArc(0,         256-$r*2,  $r*2, $r*2,  90, 90)
    $path.CloseFigure()

    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        ([System.Drawing.PointF]::new(0,0)),
        ([System.Drawing.PointF]::new(256,256)),
        ([System.Drawing.Color]::FromArgb(255, 0x1E, 0x3A, 0x8A)),
        ([System.Drawing.Color]::FromArgb(255, 0x0E, 0xA5, 0xE9)))
    $g.FillPath($brush, $path)
    $brush.Dispose()

    # geri sayım halkası — track
    $trackColor = [System.Drawing.Color]::FromArgb(46, 255, 255, 255)
    $trackPen   = New-Object System.Drawing.Pen($trackColor, 12)
    $g.DrawEllipse($trackPen, 44, 44, 168, 168)
    $trackPen.Dispose()

    # geri sayım halkası — ilerleme yayı (~%75)
    $arcPen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 12)
    $arcPen.StartCap = 'Round'; $arcPen.EndCap = 'Round'
    $g.DrawArc($arcPen, 44, 44, 168, 168, -90, 270)
    $arcPen.Dispose()

    # ortadaki W
    $wPen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 14)
    $wPen.StartCap = 'Round'; $wPen.EndCap = 'Round'; $wPen.LineJoin = 'Round'
    $points = @(
        [System.Drawing.PointF]::new( 70,  92),
        [System.Drawing.PointF]::new( 95, 168),
        [System.Drawing.PointF]::new(128, 116),
        [System.Drawing.PointF]::new(161, 168),
        [System.Drawing.PointF]::new(186,  92)
    )
    $g.DrawLines($wPen, $points)
    $wPen.Dispose()

    $g.Dispose()
    return $bmp
}

# her boyutu PNG olarak belleğe yaz; 256'lık olanı ayrıca diske de kaydet
$entries = foreach ($size in $sizes) {
    $bmp = New-IconBitmap -Size $size
    if ($size -eq 256) {
        $markPath = Join-Path $PSScriptRoot 'icon-mark.png'
        $bmp.Save($markPath, [System.Drawing.Imaging.ImageFormat]::Png)
        Write-Host "[OK] icon-mark.png olusturuldu -> $markPath" -ForegroundColor Green
    }
    $ms  = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    [pscustomobject]@{ Size = $size; Data = $ms.ToArray() }
    $ms.Dispose()
}

# ICO dosyası: header + her boyut için 16 byte dizin girdisi + PNG verileri
$icoPath = Join-Path $PSScriptRoot 'icon.ico'
$fs = [System.IO.File]::Open($icoPath, 'Create')
$bw = New-Object System.IO.BinaryWriter($fs)

# ICONDIR
$bw.Write([uint16]0)              # reserved
$bw.Write([uint16]1)              # type = ICO
$bw.Write([uint16]$entries.Count) # image count

# ICONDIRENTRY listesi
$offset = 6 + 16 * $entries.Count
foreach ($e in $entries) {
    $dim = if ($e.Size -ge 256) { 0 } else { $e.Size }
    $bw.Write([byte]$dim)             # width
    $bw.Write([byte]$dim)             # height
    $bw.Write([byte]0)                # palette colors
    $bw.Write([byte]0)                # reserved
    $bw.Write([uint16]1)              # color planes
    $bw.Write([uint16]32)             # bits per pixel
    $bw.Write([uint32]$e.Data.Length) # data size
    $bw.Write([uint32]$offset)        # data offset
    $offset += $e.Data.Length
}

# PNG verileri
foreach ($e in $entries) { $bw.Write($e.Data) }

$bw.Close(); $fs.Close()

Write-Host "[OK] icon.ico olusturuldu -> $icoPath" -ForegroundColor Green
