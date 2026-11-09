# EURO HAUL
European Trucking Career Simulator for Android and Windows.

## Current status
**Engine migration in progress: Godot 4 is the active target.** Godot is open-source and does not require a Unity license. The original Unity source under `Game/` is preserved as a reference. The Godot branch now has its first procedural greybox Truck Lab: a drivable truck controller, third-person chase camera, road/roadside geometry, ambient traffic vehicles, touch steering/throttle/brake controls, dashboard telemetry and an offline career/contract save foundation. These scripts have been committed but have **not yet been run in the Godot editor or validated on a phone**. No verified APK exists yet.

## Engineering principles
- Offline-first single-player gameplay.
- Android-first, Windows after the Android slice is stable.
- Performance-scalable 3D; profile on target hardware before setting frame-rate guarantees.
- Original or properly licensed assets only.
- Fictional logistics operators and generated freight contracts.
- Driver warnings assist the player; they do not secretly steer or override the player.
- Safety calculations are game simulation approximations, not real-world driving instructions.

## Godot vertical slice foundation
- Custom arcade truck controller with diesel-style RPM, clutch state, manual gears, reverse, steering and braking.
- Third-person chase camera.
- Procedurally generated motorway greybox with road markings, barriers and warehouse silhouettes.
- Ambient traffic vehicle movement.
- Touch steering, throttle and brake buttons.
- Dashboard speed/RPM/gear/damage display.
- Offline driver profile, freight offer, accept contract, delivery progress, XP/levels and Game Cash JSON save.
- Original EURO HAUL icon and navy/blue/gold visual identity.

## Preserved gameplay scope
The migration will retain the original requirements: richer truck physics and wheel response, traffic AI and collision sensing, blind spots and warnings, indicators/hazards/horn, camera modes, full offline career, player-owned fictional logistics companies, fleet/garage/driver management, economy, scalable Android graphics and later Windows support. The current greybox is only the first playable foundation, not the full game.

## Research and engineering references
See [Docs/OPEN_SOURCE_ENGINEERING_NOTES.md](Docs/OPEN_SOURCE_ENGINEERING_NOTES.md) for reviewed open-source Godot vehicle projects, architecture patterns and license boundaries.

## Repository layout
- `Godot/`: active Godot 4 project.
- `Game/`: preserved Unity prototype/source reference; not the active build target.
- `Docs/ENGINE_MIGRATION_GODOT.md`: migration decision, checklist and build gates.
- `Docs/OPEN_SOURCE_ENGINEERING_NOTES.md`: open-source research notes.
- `Docs/GAME_DESIGN_AND_CONTENT.md`: gameplay/content scope.
- `Docs/TRAFFIC_AND_SAFETY.md`: traffic and driving safety design.
- `Docs/VEHICLE_POWERTRAIN.md`: powertrain design.
- `Docs/BRAND_AND_UI_DESIGN.md`: app icon and visual system.
- `Docs/ANDROID_GRAPHICS_PERFORMANCE.md`: mobile performance budgets.
- `Docs/MONETIZATION_AND_AD_INTEGRITY.md`: compliant ad policy.
- `Docs/ECONOMY_AND_PLAYER_BUSINESSES.md`: economy and player companies.

## Build and test status
No Godot Android APK has been exported, installed or device-tested yet. A cloud validation workflow is configured; it must pass before a debug APK pipeline is treated as ready. Android exports require matching Godot export templates and Android tooling. A signed Play Store release additionally requires a securely managed signing key.

## License
All rights reserved by the project owner unless a file explicitly states otherwise. Third-party assets must retain their own licenses and attribution.
