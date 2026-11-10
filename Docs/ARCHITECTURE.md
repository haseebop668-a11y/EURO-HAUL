# EURO HAUL Architecture

## Active runtime boundaries (Godot 4)
- Driving: truck controller, powertrain state, steering/braking, collision damage and touch/keyboard input.
- Trailer: visual kinematic trailer prototype with capped steering articulation; no physical fifth-wheel joint or trailer collision model yet.
- Camera: third-person follow camera; cockpit and alternate cameras are later extensions.
- World: procedural motorway loop, roadside scenery and physics-collidable traffic actors with player-aware lane avoidance and forward/rear proximity reporting.
- Logistics: regional freight offers, contract acceptance, route progress, delivery and reward; the current road recycles as a short regional-drive prototype.
- Career: driver profile, XP, levels and Game Cash.
- Company: company founding, company balance, fleet purchases, one-driver-per-truck hiring, simulated hired-driver deliveries, operating costs and reputation.
- Persistence: versioned offline JSON saves under Godot `user://`.
- Platform: Android touch plus Android ARM64 and Windows x64 debug export presets; physical device/PC launch acceptance remains required.
- Monetization: isolated optional rewarded-ad adapter; core gameplay must remain functional without ads or network access.

## Dependency rules
1. Core gameplay must not depend on advertising SDKs or a network connection.
2. Career and company systems must remain playable offline.
3. Platform-specific input/export code must stay isolated from gameplay rules.
4. Economy mutations must validate funds and apply exactly once.
5. Save data must be schema-versioned, validated and recoverable.
6. Third-party content must have recorded source, license and attribution.
7. Never commit credentials, signing keys or private user data.

## Initial Godot vertical slice
The active project is in `Godot/`. It currently builds a procedural greybox road, a simple custom CharacterBody3D truck, chase camera, ambient traffic, touch/keyboard input, dashboard and offline career/company foundations. It is not yet a full truck simulation: trailer articulation, realistic suspension/tire forces, traffic sensing and a complete European world remain planned.

## Future vehicle model
Evolve the current arcade controller in testable increments: torque curve, gearbox/clutch, service brake/retarder, differential, tire friction, suspension, wheel response and trailer articulation. Profile each change on the target Android device.

## Persistence
Keep offline saves schema-versioned. Validate types/ranges on load and write atomically with a temporary file plus safe replacement. Avoid adding a remote backend to a single-player offline game unless a later feature genuinely requires synchronization or cloud accounts.

## World and content
Use fictional depots and generated cargo contracts initially. Review OpenStreetMap-derived data for ODbL compliance and attribution. Commercial map tiles, truck models, logos, sounds and real carrier data require separate license checks.
