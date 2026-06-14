# PRD: Return to Monkee MVP (Digital Health)

## Problem Statement

Junge Nutzerinnen und Nutzer (ca. 16 bis 30 Jahre) erleben im Alltag haeufig zu hohe Bildschirmzeit, unbewusste Mediennutzung, spaete Schlafenszeiten und zu wenig Bewegungspausen. Bestehende Systeme und Apps sind oft auf Aufmerksamkeit statt auf langfristig gesundes Nutzungsverhalten optimiert. Es fehlt ein niedrigschwelliges, nicht-medizinisches Selbstmanagement-Tool, das alltagstauglich hilft, digitale Gewohnheiten sichtbar zu machen, Regeln zu setzen und Fortschritte motivierend rueckzumelden.

## Solution

Return to Monkee liefert einen plattformunabhaengigen MVP (Produktziel Android + iOS, technische Umsetzung Android-first) mit lokal gespeicherten Regeln, Erinnerungen und Statistiken. Nutzer koennen Zeitlimits, Schlafenszeit und Bewegungspausen konfigurieren, werden durch Soft-Interventionen unterstuetzt und sehen ihren Tages- und Wochenfortschritt im Dashboard und Statistikbereich. Die Loesung ist bewusst nicht-strafend, sondern reflexions- und routinesorientiert.

## User Stories

1. As a young smartphone user, I want to set a daily time limit for a digital category, so that I can reduce overuse intentionally.
2. As a user, I want to activate and deactivate rules, so that I can adapt my goals to daily context.
3. As a user, I want to edit an existing usage rule, so that my limits stay realistic over time.
4. As a user, I want to define one of the MVP categories (Social Media, Video/Streaming, Gaming, Sonstiges), so that my rules map to my real habits.
5. As a user, I want to see my active rules at a glance, so that I immediately understand what applies today.
6. As a user, I want to mark when I exceeded a simulated limit, so that progress tracking reflects reality even before API integration.
7. As a user, I want a clear soft warning when a limit is exceeded, so that I pause and reconsider my current behavior.
8. As a user, I want to receive a reflection prompt after overuse, so that I can make a conscious next decision.
9. As a user, I want to select a suggested alternative activity, so that I can redirect from passive scrolling.
10. As a user, I want to set my preferred bedtime, so that I can prepare for sleep earlier.
11. As a user, I want a reminder before bedtime, so that I can close my digital day intentionally.
12. As a user, I want to confirm bedtime reminders, so that adherence is visible in statistics.
13. As a user, I want movement reminders every 60 minutes by default, so that I interrupt long passive sessions.
14. As a user, I want to configure movement interval to 30, 60, or 90 minutes, so that reminders fit my routine.
15. As a user, I want to confirm or ignore movement reminders, so that the app can track my actual behavior.
16. As a user, I want to complete onboarding in under 2 minutes, so that I can start quickly without friction.
17. As a new user, I want onboarding to set my goal focus, bedtime, break interval, and first rule, so that the app is immediately useful.
18. As a user, I want a dashboard with today's status, so that I can orient myself without navigating multiple screens.
19. As a user, I want to see kept and exceeded limits per day, so that I can evaluate discipline and drift.
20. As a user, I want to see confirmed movement breaks and ignored reminders, so that I can identify behavior patterns.
21. As a user, I want to see bedtime reminder adherence per day, so that I can improve sleep consistency.
22. As a user, I want a simple 7-day trend view, so that I can measure progress across a short horizon.
23. As a privacy-conscious user, I want all data to remain local on device, so that I trust the app with sensitive usage habits.
24. As a privacy-conscious user, I want one action to delete all stored data, so that I stay in control.
25. As a stakeholder, I want the MVP demo flow to run 15 minutes without critical crashes, so that we can validate feasibility credibly.
26. As a product owner, I want Android-first implementation with iOS follow-up, so that we de-risk delivery while preserving platform independence.

## Implementation Decisions

- Product scope is MVP-first and intentionally narrow: rules, reminders, dashboard, statistics, and lightweight interventions.
- Platform strategy is cross-platform by product definition, delivered in phases: Android first, iOS follow-up.
- Architecture stays modular monolith to optimize speed, coherence, and maintainability for a small team.
- Preferred stack is .NET MAUI + SQLite with local-only persistence for MVP.
- Usage tracking in MVP is simulated/manual; native usage APIs are deferred to post-MVP enhancement.
- Intervention behavior in MVP is soft only (warn, reflect, suggest), no hard blocking or punitive flows.
- Categories in MVP are fixed start set: Social Media, Video/Streaming, Gaming, Sonstiges.
- Movement reminder default is 60 minutes with 30/60/90 user configuration.
- Onboarding is a strict 3-step setup flow optimized for under-2-minute completion.
- Dashboard prioritizes "today context": active rules, current goal state, next reminders, and progress indicators.
- Statistics model includes daily counters and simple 7-day trend aggregates.
- Privacy baseline: no account, no cloud sync, no personal identity requirement, explicit local-data messaging.
- Data control requirement: "delete all data" action in settings is mandatory in MVP.
- Deep-module orientation for implementation:
- `Rule Engine` encapsulates rule validation, activation logic, and limit-state evaluation via stable service interface.
- `Reminder Scheduler` encapsulates timing policies and dispatch triggers independent from UI.
- `Intervention Engine` encapsulates suggestion selection and soft-intervention policy.
- `Statistics Aggregator` encapsulates event-to-metric transformations for dashboard and trend outputs.
- `Notification Adapter` isolates platform notification mechanics behind one app-facing contract.
- `Persistence Facade` abstracts SQLite schema/repository details from product modules.

## Testing Decisions

- Good tests must assert external behavior and outcomes (state transitions, emitted events, user-visible counters), not internal implementation details.
- Priority module tests:
- `Rule Engine`: limit evaluation, active/inactive toggling, boundary validation.
- `Reminder Scheduler`: interval computation, bedtime lead-time triggering, recurring behavior.
- `Intervention Engine`: correct intervention choice for scenario inputs.
- `Statistics Aggregator`: deterministic daily and 7-day metric calculations from event streams.
- `Persistence Facade`: read/write integrity for rules, reminders, events, and daily aggregates.
- UI-level smoke tests should cover the 3 core end-to-end workflows from onboarding through dashboard/statistics updates.
- Non-functional verification for MVP includes crash-free demo run (15 minutes), startup responsiveness, and local data delete completeness.
- Prior art in current codebase is minimal (starter MAUI app), so tests should establish baseline patterns for service-level unit tests plus a small set of end-to-end flows.

## Out of Scope

- Medical diagnostics, treatment guidance, or therapeutic claims.
- AI-driven health coaching or adaptive recommendation intelligence.
- Native hard app blocking across all platforms in MVP.
- Full native app-usage API integration in MVP.
- User accounts, backend identity, or cloud synchronization.
- Social features, friend systems, or competitive gamification loops.
- Advanced background-processing guarantees beyond MVP-friendly reminder behavior.
- Compliance certifications or production-grade legal/security attestations at MVP stage.

## Further Notes

- MVP acceptance baseline:
- Onboarding completion under 2 minutes.
- All three core workflows run end-to-end.
- Dashboard reflects current day status correctly.
- Statistics show correct daily counters and 7-day trend.
- No critical crash during a 15-minute Android demo flow.
- Issue slicing after this PRD should follow vertical slices around the three core workflows before deeper platform enhancements.
