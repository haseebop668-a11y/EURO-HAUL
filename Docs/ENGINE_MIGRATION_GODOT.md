# EURO HAUL — Unity-free Godot migration

## Decision

The active target engine is **Godot 4**, an open-source engine under the MIT license. The project no longer depends on a Unity Editor license, Unity account, or Unity build service.

The existing Unity implementation under `/Game` is deliberately preserved as a reference until each subsystem has a verified Godot replacement. Nothing is deleted merely to make the migration look complete.

## Why Godot

- No Unity license or paid engine subscription is required.
- Native Android export is supported.
- GDScript and the engine are open source.
- Headless editor and export workflows can run on GitHub Actions.
- The GL Compatibility renderer is the initial Android target for broad device support.

## Preserved feature scope

The following are product requirements, not optional cuts:

1. Truck handling: acceleration, torque curve, manual gears, clutch, reverse, braking, steering, wheel response and camera.
2. Road safety: safe steering limits, braking distance, time-to-collision, curve-speed advice, collision damage and emergency stop.
3. Traffic: waypoint vehicles, lane discipline, following gaps, front/rear sensing, blind spots, indicators, horns and traffic reactions.
4. Driver experience: touch controls, dashboard telemetry, camera modes, pause/settings and premium navy/blue/gold visual identity.
5. Offline career: driver profile, XP/levels, contracts, delivery progress, Game Cash and resilient local saves.
6. Business/economy: player-owned fictional logistics companies, fleets, garages, drivers, operating costs and simulated profits.
7. Monetization policy: optional, player-initiated and verified rewarded ads only; no hidden impressions or forced ads during driving. Premium currency must use compliant store billing and cannot be cashed out.
8. Platforms: Android first, Windows after the Android gameplay slice is stable.

## Migration rules

- Keep the original Unity tree intact as a reference; do not delete or silently discard it.
- Port one vertical slice at a time and keep the feature checklist updated.
- Prefer original fictional truck designs until real manufacturer licensing is secured.
- Use original or properly licensed audio, models, textures and road data.
- Do not scrape Google Maps tiles.
- Keep local saves versioned and recoverable.
- Do not claim an APK, successful build, test pass or device compatibility until CI and a real-device installation test confirm it.

## Build strategy

The Godot project lives in `/Godot`. The Android pipeline will use a pinned Godot 4 release, matching export templates and Android SDK/JDK tooling in GitHub Actions. Debug APKs are for internal testing; a Play Store release requires a properly managed signing key and release configuration.

GitHub Actions minutes and artifact retention are subject to the repository/account limits. A cloud build avoids the need for a local PC, but it does not remove the need for a successful export and real-device testing.

## Current status

- [x] Select an engine that does not require Unity licensing.
- [x] Add Godot project entry point and preserve the existing icon.
- [x] Preserve the original Unity source tree as a migration reference.
- [ ] Port vehicle physics and touch controls.
- [ ] Port traffic AI and safety systems.
- [ ] Port career, economy and company simulation.
- [ ] Rebuild the final menu, dashboard and settings in Godot.
- [ ] Add automated Godot validation and Android export workflow.
- [ ] Produce a debug APK and install/test it on the target Android device.
- [ ] Profile and tune performance on actual hardware.

A project scaffold is not a completed game. The first meaningful completion milestone is a playable, exported Android vertical slice with driving, traffic, controls, HUD and save/load verified on a physical device.
