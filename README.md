# EURO HAUL
European Trucking Career Simulator for Android and Windows.

## Current status
**Godot 4 is the active build target.** Godot is open-source and does not require a Unity license. The original Unity source under `Game/` is preserved as a reference. The active project now contains a greybox simulation prototype with manual truck controls, AI traffic, freight contracts, cargo mass/damage, animated trailer loading/unloading, deadlines, company economy, tolls, weigh station, fuel/service, three chase-camera modes, day/night and weather systems; runtime validation is still pending. The project is still a prototype: the connected European road network, production vehicle assets, full fifth-wheel physics and device acceptance remain incomplete. No successful CI build or verified APK/Windows executable has been confirmed yet.

## Engineering principles
- Offline-first single-player gameplay.
- Android and Windows x64 debug exports are configured in CI; real-device/PC acceptance is still required.
- Performance-scalable 3D; profile on target hardware before setting frame-rate guarantees.
- Original or properly licensed assets only.
- Fictional logistics operators and generated freight contracts.
- Driver warnings assist the player; they do not secretly steer or override the player.
- Safety calculations are game simulation approximations, not real-world driving instructions.

## Godot vertical slice foundation
- Truck controller with diesel-style RPM, clutch, manual gears, reverse, speed-sensitive steering, engine braking, retarder, fuel, cargo-mass handling and collision damage.
- Three selectable third-person chase camera presets.
- Procedural 900m looping motorway greybox with road markings, barriers, three T-junction access roads to pickup/delivery and service yards, a paid toll gate and a weigh station.
- Six AI traffic vehicles with wrap-aware front/rear proximity, safe-gap lane changes, speed adjustment and collision bodies.
- Android touch controls plus desktop keyboard controls, including retarder, clutch/gear actions, indicators, hazards and horn.
- Dashboard telemetry for speed, RPM, gear, fuel, truck damage, cargo mass/fragility/damage and delivery time remaining.
- Offline freight offers, pickup/delivery, forward-only route progress, cargo mass/fragility, trailer impact damage, timed deadlines, reward/XP penalties and recoverable local saves.
- Player logistics company with fleet/driver purchases, daily costs/revenue, reputation/jobs and a share of player delivery revenue.
- Accelerated day/night and clear/cloudy/fog weather cycle; fuel, refuelling, repairs and roadside recovery.
- Original EURO HAUL icon and navy/blue/gold visual identity.

## Preserved gameplay scope
The current implementation covers a functional gameplay loop, not the complete game. Remaining major scope includes a connected European road network with junctions/GPS, full suspension and fifth-wheel dynamics, trailer selection and loading animation, real depot yards/weigh stations, deeper company dispatch management, production truck/interior assets, rain and richer engine/air-brake audio, and real Android/Windows acceptance testing.

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
- `Docs/FEATURE_AUDIT.md`: existing vs implemented vs remaining feature inventory.
- `Docs/QA_TEST_PLAN.md`: automated and physical acceptance checklist.
- `Docs/REVIEW_GUIDE.md`: recommended source-review order and final acceptance checklist.
- `Docs/BUILD_AND_RELEASE.md`: Android and Windows CI build instructions.

## Build and test status
The GitHub Actions workflow is configured to import/test the project and export Android ARM64 and Windows x64 debug artifacts. Until a successful workflow run and artifacts are verified, no APK or Windows executable should be treated as built. Physical Android install/performance testing and a Windows PC launch test are separate release gates. A Play Store release additionally requires a securely managed signing key.

## License
All rights reserved by the project owner unless a file explicitly states otherwise. Third-party assets must retain their own licenses and attribution.
