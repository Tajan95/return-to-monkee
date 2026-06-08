# Return To Monkee - Entwickler-README

Return To Monkee ist eine .NET-MAUI-App fuer digitale Gesundheit mit Fokus auf bewussteren Medienkonsum, Zeitlimits, Schlafenszeit-Erinnerungen, Bewegungspausen und einfache Fortschrittsstatistiken.

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
- [Aktivitätsdiagramm-Bewegungspausen-Workflow](../diagramme/Aktivitätsdiagramme/Aktivitätsdiagramm-Bewegungspausen-Workflow.svg)

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
- Relevante Dokumentation ist aktualisiert.
- Aenderung ist in einer reviewbaren PR mit klarer Issue-Referenz beschrieben.
