# EURO HAUL Repository Review Guide

## Active project

- Active game project: `Godot/`
- Engine target: Godot 4.4.1
- Legacy Unity source: `Game/` (reference only; not the active Android build target)
- Main scene: `Godot/Scenes/Main.tscn`
- Project config: `Godot/project.godot`
- Export presets: `Godot/export_presets.cfg`

## Recommended code-review order

1. `Godot/Scripts/main.gd` — scene composition, input routing, world interactions and HUD integration.
2. `Godot/Scripts/vehicle_controller.gd` — gearbox, clutch, steering, braking, retarder, fuel, cargo load and collision damage.
3. `Godot/Scripts/trailer_controller.gd` — independent trailer body, articulation, cargo impact sensor, rear doors and cargo animation.
4. `Godot/Scripts/traffic_vehicle.gd` — AI movement, safe-gap lane changes, proximity reports and loop-aware distances.
5. `Godot/Scripts/career_manager.gd` — freight offers, cargo state, deadlines, damage/late penalties and recoverable saves.
6. `Godot/Scripts/company_manager.gd` — company founding, fleet/driver economy, daily simulation and company persistence.
7. `Godot/Scripts/world_environment_controller.gd` — accelerated day/night and weather.
8. `Godot/Tests/logic_smoke_test.gd` — automated logic checks.
9. `.github/workflows/godot-check.yml` — import, test, Android ARM64 debug export and Windows x64 debug export.

## Current implemented gameplay slice

- Manual truck gears, clutch, reverse, speed-sensitive steering, service brake, engine braking and retarder.
- Cargo-mass effects on acceleration, speed, braking and fuel consumption.
- Independent kinematic trailer with capped articulation, collision shape, impact damage, animated rear doors and cargo slide.
- Six AI traffic vehicles with loop-aware front/rear sensing and safe-gap lane changes.
- Indicators, hazards, horn and three chase-camera presets.
- Freight pickup/delivery, forward-only route progress, cargo fragility, damage penalties and timed delivery deadlines.
- Player company, truck/driver purchases, daily operating costs/revenue, reputation/jobs and player-delivery revenue share.
- Toll gate, weigh station, fuel/service stop, roadside recovery, accelerated day/night and clear/cloudy/fog weather.
- Offline career/company saves with validation, temporary writes and backup recovery.

## Known scope gaps (do not mark these as complete)

- Connected European road network, city junctions, roundabouts, borders and GPS route guidance.
- Physical fifth-wheel joint, axle/suspension/tire dynamics and load-calibrated diesel torque model.
- Trailer selection, realistic depot yards and weigh-station enforcement beyond the current single-stop prototype.
- Cockpit camera, production truck/interior assets, rain effects and richer engine/air-brake audio.
- Deeper company dispatch/driver scheduling.
- Successful CI artifact verification and real Android/Windows acceptance testing.

## Validation

Run the logic smoke test from the Godot project directory:

```sh
godot --headless --path Godot --script res://Tests/logic_smoke_test.gd
```

The GitHub Actions workflow also imports the project, boots the main scene headlessly, runs the smoke test and exports debug builds. A green workflow and uploaded artifacts must be confirmed before treating either platform build as produced. The Android APK must still be installed and exercised on a real device.

## Review checklist

- [ ] GDScript parses with no errors or warnings that indicate broken behavior.
- [ ] Forward/reverse input and manual shifting behave correctly.
- [ ] Trailer follows the truck, doors animate and cargo state survives restart.
- [ ] Traffic sensing handles the road-loop boundary and avoids unsafe lane changes.
- [ ] Freight progress only increases while driving forward.
- [ ] Cargo damage and late delivery correctly reduce payout/XP.
- [ ] Weigh/toll/service interactions cannot soft-lock the player.
- [ ] Career/company saves recover from a corrupt primary file.
- [ ] Android touch UI is usable in landscape on a physical device.
- [ ] Windows x64 debug build launches and accepts keyboard input.
- [ ] Performance, thermals, memory and frame pacing are acceptable on target hardware.
