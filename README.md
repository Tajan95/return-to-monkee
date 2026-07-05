# Return To Monkee - Entwickler-README

Return To Monkee ist eine .NET-MAUI-App fuer digitale Gesundheit mit Fokus auf bewussteren Medienkonsum, Zeitlimits, Schlafenszeit-Erinnerungen, Bewegungspausen und einfache Fortschrittsstatistiken.

## Voraussetzungen

Die App ist eine **.NET 10 MAUI**-App (Single Project). Zum Bauen und Ausfuehren wird benoetigt:

- **.NET SDK 10.0** oder neuer — Pruefen mit `dotnet --version`.
- **.NET MAUI Workload** — einmalig installieren:
  ```bash
  dotnet workload install maui
  ```
- **Pro Zielplattform:**
  - **Windows** (empfohlen fuer die Demo): Windows 10 Version 1809 (Build 17763) oder neuer. Der WinUI-Build ist unpackaged (`WindowsPackageType=None`), es wird kein Store-Paket benoetigt.
  - **Android**: Android SDK + ein Emulator **oder** ein per USB verbundenes Geraet (min. **Android 5.0 / API 21**). Android-SDK und JDK kommen ueber die MAUI-Workload bzw. Visual Studio mit.
- **Optional, aber am einfachsten:** Visual Studio 2022 (17.x) mit der Workload **".NET Multi-platform App UI development"**. Dann laesst sich alles per Auswahl des Zielgeraets und `F5` starten.

> Hinweis: iOS/macOS-Targets werden nur auf macOS gebaut und sind fuer die Abgabe nicht erforderlich.

## Projekt starten (Build & Run)

Alle Befehle werden aus dem Ordner `ReturnToMonkee/` (dieser Ordner, in dem die `ReturnToMonkee.csproj` liegt) ausgefuehrt.

1. **Abhaengigkeiten wiederherstellen:**
   ```bash
   dotnet restore ReturnToMonkee.csproj
   ```

2. **Auf Windows starten (WinUI):**
   ```bash
   dotnet build ReturnToMonkee.csproj -t:Run -f net10.0-windows10.0.19041.0
   ```

3. **Auf Android starten (Emulator muss laufen bzw. Geraet verbunden):**
   ```bash
   dotnet build ReturnToMonkee.csproj -t:Run -f net10.0-android
   ```

**Mit Visual Studio 2022:** Projekt `ReturnToMonkee.csproj` oeffnen, oben das Zielgeraet waehlen (**Windows Machine** oder ein **Android Emulator**) und mit `F5` starten.

Das Projekt ist mehrfach-getargetet (`net10.0` fuer die Test-Bibliothek, `net10.0-android`, unter Windows zusaetzlich `net10.0-windows10.0.19041.0`). Beim Bauen/Starten daher immer das gewuenschte Framework mit `-f` angeben. Eine `.sln` gibt es nicht — es wird direkt gegen die `.csproj` gebaut.

## Tests ausfuehren

```bash
dotnet test ReturnToMonkee.Tests/ReturnToMonkee.Tests.csproj
```

Die Tests laufen gegen das `net10.0`-Target (reine .NET-Bibliothek, kein Geraet/Emulator noetig).

## Fehlerbehebung

- **`MSB3021` / `MSB3027` ("Datei wird von einem anderen Prozess verwendet", `ReturnToMonkee.exe`)** beim Windows-Build: die App laeuft noch. Vor dem erneuten Bauen die laufende App schliessen.
- **`maui`-Workload fehlt / Build findet MAUI nicht:** `dotnet workload install maui` ausfuehren (ggf. `dotnet workload update`).
- **Android-Build startet nicht auf dem Geraet:** pruefen, ob ein Emulator laeuft (`adb devices`) bzw. USB-Debugging am Geraet aktiv ist.
- **Erster Android-Build ist langsam:** das ist normal (Ressourcen-/Package-Aufbau); Folgebuilds sind deutlich schneller.

## Projektstatus

- MVP-Backlog ist als kleine, umsetzbare Issues geschnitten.
- Reihenfolge: erst Phase 0 (Basis), danach Phase 1 (Feature-Slices).
- Aktuelles Ziel: Team-Onboarding beschleunigen und Implementierung sauber starten.

## Neu im Team?

Starte mit dem Quickstart:

- [MAUI Quickstart fuer das Team](docs/quickstart/MAUI_Quickstart_Team.md)

## Projektdokumente

- [PRD MVP Digital Health](docs/prd/PRD_MVP_Digital_Health_v1.md)
- [Lastenheft Return to Monkee v1](docs/lastenheft/Lastenheft_Return_to_Monkee_v1.md)
- [Diagrammuebersicht](docs/diagramme/README.md)
- [Systemkontextdiagramm v2](docs/diagramme/systemkontext/Systemkontextdiagramm_v2.drawio)
- [Use-Case-Diagramm MVP v2](docs/diagramme/use-cases/UseCase_Diagramm_MVP_v2.drawio)
- [Moduluebersicht MVP v2](docs/diagramme/moduluebersicht/Moduluebersicht_MVP_v2.drawio)
- [Aktivitätsdiagramm Reminder-Statistik-Workflow](docs/diagramme/Aktivitätsdiagramme/Aktivitätsdiagramm-Reminder-Statistik-Workflow.svg)
- [Aktivitätsdiagramm-Zeitlimit-Workflow](docs/diagramme/Aktivitätsdiagramme/Aktivitätsdiagramm-Zeitlimit-Workflow.drawio.svg)

## Projektkonventionen

### Branching und Commits

- Pro Issue ein eigener Branch.
- Kleine, reviewbare Commits bevorzugen.
- Commit-Messages und PR-Text enthalten immer die zugehoerige Issue-Referenz (z. B. `#11`).
- PRs so schneiden, dass sie einen klaren fachlichen Zweck abbilden.

### Coding-Basics fuer C#/.NET MAUI

- Nullable ist aktiv und bleibt aktiv.
- Benennungen klar und sprechend halten.
- Keine toten TODOs: Jeder TODO-Vermerk braucht eine Issue-Referenz oder wird entfernt.
- Vor dem Merge muss ein lokaler Build mindestens fuer Android Debug erfolgreich sein.

### Ticket-Flow im MVP

- Phase 0 (Basis) zuerst abschliessen, dann Phase 1 (Feature-Slices).
- Neue Arbeit startet nur, wenn vorgelagerte Abhaengigkeiten laut Backlog erfuellt sind.
- Tickets sind absichtlich klein (ca. 0.5 bis 1 Tag) und sollen auch so umgesetzt bleiben.

### Dokumentationspflege

- PRD aktualisieren, wenn sich Produktziel, Scope oder Akzeptanzkriterien aendern.
- Lastenheft aktualisieren, wenn Anforderungen oder nicht-funktionale Rahmenbedingungen angepasst werden.
- Diagrammdateien aktualisieren, wenn Datenfluss, Architektur oder Modulgrenzen veraendert werden.
- Bei jeder fachlichen oder technischen Richtungsentscheidung pruefen, ob Doku angepasst werden muss.

## Definition of Done (MVP-Issues)

- Akzeptanzkriterien des Issues sind vollstaendig erfuellt.
- Lokaler Build ist erfolgreich (mindestens Android Debug).
- Tests laufen durch: `dotnet test ReturnToMonkee.Tests/ReturnToMonkee.Tests.csproj`
- Relevante Dokumentation ist aktualisiert.
- Aenderung ist in einer reviewbaren PR mit klarer Issue-Referenz beschrieben.
