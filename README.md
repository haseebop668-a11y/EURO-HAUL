# EURO HAUL
European Trucking Career Simulator for Android and Windows.

## Current status
**Engine migration in progress: Godot 4 is the active target.** Godot is open-source and does not require a Unity license. The original Unity source under `Game/` is intentionally preserved as a reference while each gameplay system is ported. The Godot project currently contains its project configuration, boot scene and original app icon; it is not yet a playable port and no verified APK exists yet.

## Engineering principles
- Offline-first single-player gameplay.
- Android-first, with Windows planned after the Android slice is stable.
- Performance-scalable 3D; visual quality must be profiled on target devices.
- Original or properly licensed assets only.
- Fictional logistics operators and generated freight contracts.
- Driver warnings assist the player; they do not secretly steer or override the player.
- Safety calculations are game simulation approximations, not real-world driving instructions.

## Preserved gameplay scope
- Truck handling: torque, RPM, clutch, manual gearbox, reverse, steering, braking, wheel response and camera.
- Mobile touch controls and PC keyboard controls.
- Indicators, hazards, horn and dashboard telemetry.
- Front/rear traffic sensing, blind spots, following gaps and conservative AI traffic.
- Braking-distance, time-to-collision and curve-speed advice, collision damage and emergency stop.
- Offline career: local save/backup, driver XP/levels, Game Cash, freight contracts and delivery rewards.
- Player-owned fictional logistics companies, fleets, garages, drivers, costs and simulated profits.
- Premium navy/blue/gold menu and icon identity.
- Monetization policy: genuine optional rewarded ads only; no hidden impressions or forced ads while driving. Premium digital goods must use compliant store billing.

## Repository layout
- `Godot/`: active Godot 4 project and future Android/Windows exports.
- `Game/`: preserved Unity prototype/source reference; not the active build target.
- `Docs/ENGINE_MIGRATION_GODOT.md`: migration decision, feature-preservation checklist and build gates.
- `Docs/GAME_DESIGN_AND_CONTENT.md`: gameplay and content scope.
- `Docs/TRAFFIC_AND_SAFETY.md`: traffic and driving safety design.
- `Docs/VEHICLE_POWERTRAIN.md`: vehicle and gearbox design.
- `Docs/BRAND_AND_UI_DESIGN.md`: app icon and visual system.
- `Docs/ANDROID_GRAPHICS_PERFORMANCE.md`: mobile performance targets.
- `Docs/MONETIZATION_AND_AD_INTEGRITY.md`: compliant advertising policy.
- `Docs/ECONOMY_AND_PLAYER_BUSINESSES.md`: in-game economy and company simulation.

## Build and test status
No Godot Android APK has been exported, installed or device-tested yet. The migration checklist must be completed subsystem by subsystem; a project scaffold is not a completed game. Android debug builds will be produced through cloud CI once the Godot port and export configuration are ready. A signed Play Store release additionally requires a securely managed signing key.

## License
All rights reserved by the project owner unless a file explicitly states otherwise. Third-party assets must retain their own licenses and attribution.
