# TotpViewerWpf

Google Authenticator benzeri Windows WPF TOTP Viewer.

## Özellikler

- Google Authenticator benzeri ana ekran
- Sağ altta `+` ile ekleme
- Manual secret key ile hesap ekleme
- QR görselinden `otpauth://totp/...` içe aktarma
- Google Authenticator dışa aktarma QR'ı: `otpauth-migration://offline?...` desteği
- Kodun üstüne tıklayınca panoya kopyalama
- Secret'ları Windows DPAPI ile CurrentUser bazlı şifreli saklama

## Çalıştırma

```powershell
cd TotpViewerWpf
dotnet restore
dotnet run
```

## Publish

```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

Çıktı:

```text
bin\Release\net8.0-windows\win-x64\publish
```

## Veri dosyası

Secret dosyası şu dizinde şifreli saklanır:

```text
%APPDATA%\TotpViewerWpf\accounts.secure
```

Bu dosya aynı Windows kullanıcı hesabı dışında çözülemez.
