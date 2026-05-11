# MAUI Quickstart fuer das Team (Android-first)

Dieser Quickstart ist fuer Teammitglieder ohne oder mit wenig MAUI/C# Erfahrung. Ziel ist ein reproduzierbarer lokaler Start bis zum erfolgreichen Android-Debug-Run.

## 1) Voraussetzungen

- Windows 10/11 oder macOS mit aktuellen Updates.
- .NET SDK installiert (Version passend zu `global.json`, falls vorhanden).
- MAUI-Workloads installiert.
- IDE:
  - Visual Studio 2022/2026 mit MAUI-Workload
  - oder VS Code + .NET CLI (fuer Build/Run per CLI)
- Android-Setup:
  - Android SDK + Emulator
  - oder ein physisches Android-Geraet mit USB-Debugging

## 2) Initiales Setup

Im Projektordner `ReturnToMonkee/`:

```bash
dotnet --info
dotnet workload install maui
dotnet restore
```

Wenn `dotnet workload install maui` fehlschlaegt, zuerst SDK-Version pruefen und ggf. Visual-Studio-Installer/SDK aktualisieren.

## 3) Build (Android Debug)

```bash
dotnet build -f net10.0-android
```

Erwartung: Build laeuft ohne Fehler durch.

## 4) App starten (Android)

Option A (Visual Studio):

- Loesung `ReturnToMonkee.slnx` oeffnen.
- Startprofil auf Android-Emulator oder verbundenes Geraet setzen.
- Debug-Start ausfuehren.

Option B (CLI):

```bash
dotnet build -t:Run -f net10.0-android
```

Hinweis: Fuer CLI-Run muss ein lauffaehiges Android-Ziel verfuegbar sein.

## 5) Troubleshooting (Kurz)

### Problem: MAUI-Workload fehlt

Symptome:

- Fehlermeldungen zu fehlenden MAUI-Targets/SDK-Komponenten.

Loesung:

```bash
dotnet workload install maui
dotnet workload restore
```

Danach erneut `dotnet restore` und `dotnet build -f net10.0-android`.

### Problem: Emulator/Geraet wird nicht gefunden

Symptome:

- Kein Android-Target in IDE sichtbar.
- CLI-Run bricht ohne Device ab.

Loesung:

- Emulator manuell starten und warten, bis er voll gebootet ist.
- Bei physischem Geraet USB-Debugging aktivieren.
- ADB-Verbindung pruefen:

```bash
adb devices
```

### Problem: SDK-/Workload-Version passt nicht

Symptome:

- Build-Fehler nach Update oder neuer Umgebung.

Loesung:

- `dotnet --info` pruefen.
- Visual Studio + .NET SDK + MAUI-Workloads auf kompatiblen Stand bringen.
- Danach Workloads erneut installieren/restoren.

## 6) Naechster Schritt fuer iOS (Folgepfad)

iOS ist im MVP nicht der erste Onboarding-Pfad. Nach erfolgreichem Android-Setup:

- macOS Build-Host und Apple-Toolchain sicherstellen.
- Ziel-Framework `net10.0-ios` lokal bauen:

```bash
dotnet build -f net10.0-ios
```

- iOS-Run erst starten, wenn Signing/Provisioning sauber konfiguriert ist.
