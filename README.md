# EURO HAUL
European Trucking Career Simulator for Android and Windows.

## Current status
Early Unity project foundation. Vehicle, driver-signal, safety-advisory and traffic-AI source modules are committed. The Unity scene and builds still require editor validation; this repository does not yet contain a verified playable release APK.

## Engineering principles
- Offline-first single-player gameplay.
- Shared Unity project for Android and Windows.
- Performance-scalable 3D; visual quality must be profiled on target devices.
- Original or properly licensed assets only.
- Fictional logistics operators and generated freight contracts.
- Driver warnings assist the player; they do not secretly steer or override the player.
- Safety calculations are game simulation approximations, not real-world driving instructions.

## Implemented source modules
- Four-wheel WheelCollider truck controller and prototype powertrain.
- Engine RPM, clutch, manual gears and dashboard telemetry.
- Keyboard and mobile touch input adapters.
- Turn indicators, hazard lights and horn controls.
- Forward/rear traffic sensing and left/right blind-spot sensing.
- Stopping-distance, time-to-collision and curve-speed advisory calculations.
- Waypoint-based traffic vehicle controller with conservative following and courtesy-horn event.
- Unity EditMode tests for deterministic safety math.
- Manual Unity CI workflow for Android APK and Windows x64 artifacts.

## Controls (PC prototype)
| Key | Action |
|---|---|
| W / Up | Throttle |
| A / D or Left / Right | Steering |
| Space | Service brake |
| C (hold) | Clutch |
| E / Q | Gear up / down (clutch required) |
| N / R | Neutral / reverse (clutch required) |
| Z / X | Left / right indicator |
| H | Hazard lights |
| B (hold) | Horn |

## Project and build
- Unity editor baseline: 6000.3.0f1 (Unity 6.3 LTS).
- Unity project root: `Game/`. The Editor bootstrap creates a Truck Lab road, player truck, mobile control canvas, traffic routes and build-scene entry on first Editor open.
- Setup: [Docs/TRUCK_LAB_SETUP.md](Docs/TRUCK_LAB_SETUP.md).
- Powertrain: [Docs/VEHICLE_POWERTRAIN.md](Docs/VEHICLE_POWERTRAIN.md).
- Traffic and safety: [Docs/TRAFFIC_AND_SAFETY.md](Docs/TRAFFIC_AND_SAFETY.md).\n- Full gameplay/content roadmap: [Docs/GAME_DESIGN_AND_CONTENT.md](Docs/GAME_DESIGN_AND_CONTENT.md).\n- Android graphics and performance budgets: [Docs/ANDROID_GRAPHICS_PERFORMANCE.md](Docs/ANDROID_GRAPHICS_PERFORMANCE.md).
- CI/APK: [Docs/BUILD_AND_RELEASE.md](Docs/BUILD_AND_RELEASE.md).
- Monetization and rewarded ads: [Docs/MONETIZATION_AND_AD_INTEGRITY.md](Docs/MONETIZATION_AND_AD_INTEGRITY.md).

## Important limitations
No full European map, finished truck art, trailer/cargo physics, career economy, final touch HUD, sound library, or verified Android/Windows build is available yet. AI traffic requires authored lane routes and correctly configured vehicle layers/sensors. Sensor ranges, braking assumptions and cornering limits must be tuned and play-tested; they are not certified vehicle dynamics.

## License
All rights reserved by the project owner unless a file explicitly states otherwise. Third-party assets must retain their own licenses and attribution.
