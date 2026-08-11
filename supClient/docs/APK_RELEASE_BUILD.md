# Android APK Release Build

Use a signed Release APK for installing or sharing the app. Do not use APK files from a Debug build or Visual Studio Fast Deployment.

## 1. Create a signing key

Run from the repository root:

```powershell
New-Item -ItemType Directory -Force -Path .\supClient\Signing
keytool -genkeypair -v `
  -keystore .\supClient\Signing\sup-release.keystore `
  -alias sup-release `
  -keyalg RSA `
  -keysize 2048 `
  -validity 10000
```

Remember the passwords you enter. The keystore is ignored by git.

## 2. Set signing variables

Use the same PowerShell window:

```powershell
$env:SUP_ANDROID_KEYSTORE = "E:\source\repos\supClient\supClient\Signing\sup-release.keystore"
$env:SUP_ANDROID_KEY_ALIAS = "sup-release"
$env:SUP_ANDROID_STORE_PASS = "your-store-password"
$env:SUP_ANDROID_KEY_PASS = "your-key-password"
```

## 3. Publish the APK

```powershell
dotnet publish .\supClient\supClient.csproj -f net9.0-android -c Release
```

The APK will be in:

```text
supClient\bin\Release\net9.0-android\publish\
```

The build keeps the technical Android package file and also creates a friendly copy:

```text
SportUrbanPoint-SuP-v1.0.apk
```

The `v1.0` part comes from `ApplicationDisplayVersion` in the project publish settings.

Install/share the friendly APK from that `publish` folder.

## Notes

- Stop the app in Visual Studio before publishing.
- If the app was installed previously from Debug, uninstall it from the phone first.
- Keep the same keystore for future updates. Android will not update an app signed with a different key.
