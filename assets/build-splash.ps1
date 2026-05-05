#requires -Version 5.1
# WinOTP splash builder — assets/splash.png üretir.
# Çalıştır: powershell -ExecutionPolicy Bypass -File assets\build-splash.ps1

Add-Type -AssemblyName System.Drawing

$W = 480
$H = 320

$bmp = New-Object System.Drawing.Bitmap($W, $H, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g   = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode    = 'AntiAlias'
$g.InterpolationMode = 'HighQualityBicubic'
$g.PixelOffsetMode  = 'HighQuality'
$g.TextRenderingHint = 'AntiAliasGridFit'

# arka plan: yumusak yuvarlak kosseli kare + gradyan
$r = 18
$bg = New-Object System.Drawing.Drawing2D.GraphicsPath
$bg.AddArc(0,       0,       $r*2, $r*2, 180, 90)
$bg.AddArc($W-$r*2, 0,       $r*2, $r*2, 270, 90)
$bg.AddArc($W-$r*2, $H-$r*2, $r*2, $r*2,   0, 90)
$bg.AddArc(0,       $H-$r*2, $r*2, $r*2,  90, 90)
$bg.CloseFigure()

$bgBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    ([System.Drawing.PointF]::new(0,0)),
    ([System.Drawing.PointF]::new($W,$H)),
    ([System.Drawing.Color]::FromArgb(255, 0x0F, 0x17, 0x2A)),
    ([System.Drawing.Color]::FromArgb(255, 0x0E, 0xA5, 0xE9)))
$g.FillPath($bgBrush, $bg)
$bgBrush.Dispose()

# --- Ortadaki ikon (140x140), splash'in ust kismina hizali ---
$iconSize = 140
$iconX    = ($W - $iconSize) / 2
$iconY    = 50

$state = $g.Save()
$g.TranslateTransform($iconX, $iconY)
$scale = $iconSize / 256.0
$g.ScaleTransform($scale, $scale)

# yuvarlak kose karesi
$cr = 56
$icp = New-Object System.Drawing.Drawing2D.GraphicsPath
$icp.AddArc(0,        0,        $cr*2, $cr*2, 180, 90)
$icp.AddArc(256-$cr*2,0,        $cr*2, $cr*2, 270, 90)
$icp.AddArc(256-$cr*2,256-$cr*2,$cr*2, $cr*2,   0, 90)
$icp.AddArc(0,        256-$cr*2,$cr*2, $cr*2,  90, 90)
$icp.CloseFigure()

$iconBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    ([System.Drawing.PointF]::new(0,0)),
    ([System.Drawing.PointF]::new(256,256)),
    ([System.Drawing.Color]::FromArgb(255, 0x1E, 0x3A, 0x8A)),
    ([System.Drawing.Color]::FromArgb(255, 0x0E, 0xA5, 0xE9)))
$g.FillPath($iconBrush, $icp)
$iconBrush.Dispose()

# track halka
$trackPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(46,255,255,255), 12)
$g.DrawEllipse($trackPen, 44, 44, 168, 168)
$trackPen.Dispose()

# ilerleme yayi
$arcPen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 12)
$arcPen.StartCap = 'Round'; $arcPen.EndCap = 'Round'
$g.DrawArc($arcPen, 44, 44, 168, 168, -90, 270)
$arcPen.Dispose()

# W
$wPen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 14)
$wPen.StartCap = 'Round'; $wPen.EndCap = 'Round'; $wPen.LineJoin = 'Round'
$pts = @(
    [System.Drawing.PointF]::new( 70,  92),
    [System.Drawing.PointF]::new( 95, 168),
    [System.Drawing.PointF]::new(128, 116),
    [System.Drawing.PointF]::new(161, 168),
    [System.Drawing.PointF]::new(186,  92))
$g.DrawLines($wPen, $pts)
$wPen.Dispose()

$g.Restore($state)

# --- "WinOTP" yazisi ---
$titleFont = New-Object System.Drawing.Font('Segoe UI', 30, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
$winBrush  = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
$otpBrush  = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 0x67, 0xE8, 0xF9))

$winText = 'Win'
$otpText = 'OTP'
$sf = [System.Drawing.StringFormat]::GenericTypographic
$winSize = $g.MeasureString($winText, $titleFont, [int]::MaxValue, $sf)
$otpSize = $g.MeasureString($otpText, $titleFont, [int]::MaxValue, $sf)
$totalW  = $winSize.Width + $otpSize.Width
$titleY  = $iconY + $iconSize + 20
$titleX  = ($W - $totalW) / 2

$g.DrawString($winText, $titleFont, $winBrush, $titleX,                $titleY, $sf)
$g.DrawString($otpText, $titleFont, $otpBrush, $titleX + $winSize.Width, $titleY, $sf)

$winBrush.Dispose(); $otpBrush.Dispose(); $titleFont.Dispose()

# --- alt yazi ---
$subFont = New-Object System.Drawing.Font('Segoe UI', 13, [System.Drawing.FontStyle]::Regular, [System.Drawing.GraphicsUnit]::Pixel)
$subBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(200, 226, 232, 240))
$sub = 'TOTP Authenticator'
$subSize = $g.MeasureString($sub, $subFont, [int]::MaxValue, $sf)
$g.DrawString($sub, $subFont, $subBrush, ($W - $subSize.Width) / 2, $titleY + 50, $sf)
$subFont.Dispose(); $subBrush.Dispose()

$g.Dispose()

$path = Join-Path $PSScriptRoot 'splash.png'
$bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()

Write-Host "[OK] splash.png olusturuldu -> $path" -ForegroundColor Green
