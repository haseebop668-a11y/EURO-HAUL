# EURO HAUL Architecture

## Active runtime boundaries (Godot 4)
- Driving: manual gearbox/clutch, reverse, speed-sensitive steering, engine braking, retarder, cargo-mass-dependent acceleration/braking/speed/fuel, collision damage and touch/keyboard input.
- Trailer: independent AnimatableBody3D with cargo collision body, impact sensor, truck-damage/cargo-damage propagation and capped load-dependent articulation; no physical fifth-wheel joint, axle or suspension dynamics.
- Camera: three selectable chase presets (chase, close and high); cockpit camera remains a later extension.
- World: 900m looping greybox motorway, three T-junction access roads to depot/service yards, toll barrier, weigh station, service/refuelling stop, roadside scenery, accelerated day/night and clear/cloudy/fog states; six traffic actors use wrap-aware proximity, safe-gap lane changes and speed adjustment.
- Logistics: freight offers, cargo pickup/delivery, forward-only progress, mass/fragility, trailer-impact cargo damage, delivery deadlines and damage/late-adjusted reward/XP; fuel, repair, toll and weigh-station loops.
- Career: driver profile, XP, levels and Game Cash.
- Company: founding, balance, truck purchases, one-driver-per-truck hiring, daily simulated revenue/operating costs, player-delivery revenue share, completed jobs and reputation.
- Persistence: validated schema-versioned offline JSON saves under Godot `user://`, temporary writes, backup recovery, saved truck telemetry and freight timer/damage.
- Platform: Android touch plus Android ARM64 and Windows x64 debug export presets; physical device/PC launch acceptance remains required.
- Monetization: no ad SDK or network dependency is currently included; future optional permitted ads must not block offline gameplay.

## Dependency rules
1. Core gameplay must not depend on advertising SDKs or a network connection.
2. Career and company systems must remain playable offline.
3. Platform-specific input/export code must stay isolated from gameplay rules.
4. Economy mutations must validate funds and apply exactly once.
5. Save data must be schema-versioned, validated and recoverable.
6. Third-party content must have recorded source, license and attribution.
7. Never commit credentials, signing keys or private user data.

## Initial Godot vertical slice
The active project is in `Godot/`. It builds a procedural greybox motorway, custom CharacterBody3D truck, independent kinematic trailer, AI traffic, three chase-camera presets, Android touch/desktop keyboard controls, freight/cargo economy, toll/weigh/service stops, day/night/weather and offline career/company systems. This is still a prototype rather than a full commercial truck simulator. A connected European city-to-city road network with GPS routing, production assets, physical fifth-wheel/suspension dynamics, loading animation and device acceptance remain outstanding.

## Future vehicle model
Evolve the current arcade controller in testable increments: calibrated diesel torque curve, differential, tire friction, ABS/traction, suspension, wheel response and physical fifth-wheel/axle dynamics. Profile each change on the target Android device.

## Persistence
Keep offline saves schema-versioned. Validate types/ranges on load and write atomically with a temporary file plus safe replacement. Avoid adding a remote backend to a single-player offline game unless a later feature genuinely requires synchronization or cloud accounts.

## World and content
Use fictional depots and generated cargo contracts initially. Next world milestone is a connected European-style road network with junctions and GPS routing. Review OpenStreetMap-derived data for ODbL compliance and attribution. Commercial map tiles, truck models, logos, sounds and real carrier data require separate license checks.

## Verification and review

- `Docs/FEATURE_AUDIT.md` records what is present in source versus what remains.
- `Docs/QA_TEST_PLAN.md` separates automated logic checks from physical Android/Windows acceptance.
- Do not mark the app release-ready until the GitHub Actions workflow produces both artifacts and the APK is installed and exercised on a real Android device.
