# EURO HAUL Feature Inventory and Gap Audit

Audit date: 2026-11-10
Active project: `Godot/` (Godot 4.4.1). The Unity project under `Game/` is preserved as reference material.

This inventory distinguishes code that exists from code that has passed runtime/device QA. A feature marked **Implemented (not device-verified)** exists in the current source but is not yet certified on a physical Android phone.

## Existing systems

| System | Current implementation | Verification status |
|---|---|---|
| Android project/export preset | Android ARM64 preset, package ID `com.eurohaul.game` | CI export not yet passing |
| App identity | EURO HAUL name and SVG icon | Source present; launcher appearance not device-verified |
| 3D world | Procedural looping motorway, pickup/delivery depot triggers and signs, toll barrier, fuel/service station, lane markings, roadside barriers and warehouse greyboxes | Implemented as a greybox prototype; visual/performance QA pending |
| Player truck | Custom `CharacterBody3D`, engine RPM/torque approximation, gearbox, clutch, reverse, brake, speed limits, engine braking, retarder, fuel use, cargo-mass-dependent acceleration/braking/speed, service refuelling and repair | Logic smoke tests authored; CI not yet green |
| Steering safety | Speed-sensitive steering angle using a lateral-acceleration limit | Source implemented; handling not tuned on device |
| Camera | Three SpringArm chase presets (chase, close and high) | Source implemented; device QA pending |
| Trailer | Independent kinematic trailer body with cargo collision shape, cargo-mass articulation, impact sensor that damages truck and fragile cargo, wheels and capped forward/reverse articulation | Prototype implemented; physical hitch and load dynamics not yet implemented |
| Inputs and feedback | Keyboard/touch controls, gear controls, blinking rear indicators/hazards and a generated dual-tone horn audio effect | Source implemented; device/audio QA pending |
| Traffic | Six kinematic traffic bodies with cruise speed, lane assignment, wrap-aware proximity sensing and neighbor-aware lane changes/speed adjustment | Prototype and logic tests authored; CI/device QA pending |
| Traffic response | Physics-tick player-aware proximity detection; checks adjacent traffic gaps before lane changes; slows/adjusts speed when blocked; returns after the gap clears; traffic actors have collision bodies | Newly implemented; automated test authored; CI not yet green |
| Lane-change feedback | Indicator-triggered adjacent-lane proximity warning / clear prompt; nearest forward/rear traffic HUD warning | Newly implemented; automated tests authored; CI not yet green |
| Collision damage | Side-impact filtering/cooldown plus trailer impact sensor; damage capped at 100% | Source implemented; physical interaction/device QA pending |
| Career and freight | Driver profile, cash, 2–5 km regional offers, cargo mass/fragility, pickup loading, forward-only route progress, delivery-depot unloading, damage/late-adjusted rewards/XP and timed delivery deadlines; road segment recycles to support continuous driving | Source implemented; logic tests authored; CI not yet green |
| Company | Company founding with seed capital, truck purchase, one-driver-per-truck hiring, simulated daily driver jobs/revenue, operating costs, reputation and delivery revenue | Basic passive driver simulation implemented; full management/dispatch UI absent |
| Persistence | Local schema-versioned career/company JSON saves, temp-write/backup recovery, freight state, truck fuel and damage persistence | Source implemented; restart/corruption QA pending |
| Offline mode | Core career/company saves and gameplay logic are local; no account/backend dependency | Implemented by architecture |

## Missing or incomplete systems

These are not present as complete gameplay features yet and must not be described as finished:

1. **Articulated trailer simulation** — an independent kinematic trailer body with wheels, collision shape and capped steering articulation now exists; a physical fifth-wheel constraint, axle/suspension dynamics, load-dependent handling and realistic reverse jack-knifing remain incomplete.
2. **Full vehicle dynamics** — wheel/tire grip, suspension, axle loads, differential, calibrated torque curves, ABS/traction behaviour and load-dependent handling. Engine braking, retarder, fuel, service and repair basics exist.
3. **Traffic simulation** — basic traffic bodies, multi-vehicle safe-gap checks and player-aware lane changes exist; overtaking rules, junction behaviour, traffic lights, roundabouts, speed-limit adherence and emergency braking remain.
4. **Forward/rear awareness** — basic front/rear proximity and closing-speed warnings exist; dedicated blind-spot coverage, lane-change risk scoring and more detailed dashboard alerts remain.
5. **European world and navigation** — basic toll and service-stop interactions exist on a single straight greybox motorway loop; connected road network, city-specific routes, junctions, roundabouts, rest areas, borders and GPS route guidance remain.
6. **Freight operations** — pickup/delivery depots, cargo load/unload, toll payment, fuel consumption, refuelling and repairs exist as basic loops; real depot yards, loading animation, trailer selection and route-specific delivery confirmation remain.
7. **Company progression** — detailed AI driver schedules/routes, truck assignment UI, fleet maintenance, driver wages/skills, contracts for hired drivers, reputation-driven contract access and a full management screen.
8. **Presentation** — basic horn audio, rear indicator lights, day/night lighting and clear/cloudy/fog environment states now exist; production truck/trailer models, interiors, detailed road assets, animations, rain effects, engine/air-brake ambience and UI polish remain.
9. **Platform release** — Android and Windows x64 debug exports are now configured in CI; physical Android install/performance/thermal testing, Windows PC launch testing and long-session acceptance remain.
10. **Release quality** — complete automated tests for persistence, traffic interactions, contract edge cases, economy exploits and long-session stability.

## Current priority order

- P0: Get CI green: import, headless scene, logic tests and ARM64 APK export.
- P1: Replace the single looping greybox road with a connected European-style route network, junctions and GPS navigation.
- P2: Add cargo loading animation, trailer selection, weigh stations and route-specific delivery confirmation.
- P3: Build the full company management/dispatch UI and driver scheduling.
- P4: Improve fifth-wheel physics, suspension/tyres, production truck/trailer assets, interiors, rain and engine/air-brake audio.
- P5: Android device acceptance, performance/thermal tuning and Windows PC launch testing.

## Review rule

A source file or a passing headless test is not proof of full feature quality. Mark a feature complete only after its acceptance criteria pass. The user review should happen after the planned implementation and CI gates have stabilized; defects found in that review should be logged, fixed and retested in a separate review pass.
