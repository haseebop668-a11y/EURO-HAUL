# EURO HAUL Feature Inventory and Gap Audit

Audit date: 2026-11-10
Active project: `Godot/` (Godot 4.4.1). The Unity project under `Game/` is preserved as reference material.

This inventory distinguishes code that exists from code that has passed runtime/device QA. A feature marked **Implemented (not device-verified)** exists in the current source but is not yet certified on a physical Android phone.

## Existing systems

| System | Current implementation | Verification status |
|---|---|---|
| Android project/export preset | Android ARM64 preset, package ID `com.eurohaul.game` | CI export not yet passing |
| App identity | EURO HAUL name and SVG icon | Source present; launcher appearance not device-verified |
| 3D world | Procedural straight motorway, lane markings, roadside barriers and warehouse greyboxes | Implemented; visual/performance QA pending |
| Player truck | Custom `CharacterBody3D`, engine RPM/torque approximation, gearbox, clutch, reverse, brake and speed limits | Logic smoke tests authored; CI not yet green |
| Steering safety | Speed-sensitive steering angle using a lateral-acceleration limit | Source implemented; handling not tuned on device |
| Camera | Third-person SpringArm chase camera | Source implemented; device QA pending |
| Inputs and feedback | Keyboard/touch controls, gear controls, blinking rear indicators/hazards and a generated dual-tone horn audio effect | Source implemented; device/audio QA pending |
| Traffic | Six scripted vehicles with cruise speed and lane assignment | Basic traffic exists |
| Traffic response | Physics-tick player-aware proximity detection; checks adjacent traffic gaps before lane changes; slows/adjusts speed when blocked; returns after the gap clears; traffic actors have collision bodies | Newly implemented; automated test authored; CI not yet green |
| Lane-change feedback | Indicator-triggered adjacent-lane proximity warning / clear prompt; nearest forward/rear traffic HUD warning | Newly implemented; automated tests authored; CI not yet green |
| Collision damage | Side-impact filtering and cooldown; damage capped at 100% | Source implemented; traffic collision integration and device QA pending |
| Career and freight | Driver profile, cash, 2–5 km regional offers, accept, pickup loading, route progress, delivery-depot unloading, rewards, XP and levels; road segment recycles to support continuous driving | Source implemented; logic tests authored; CI not yet green |
| Company | Company founding with seed capital, truck purchase, one-driver-per-truck hiring, simulated daily driver jobs/revenue, operating costs, reputation and delivery revenue | Basic passive driver simulation implemented; full management/dispatch UI absent |
| Persistence | Local schema-versioned JSON career/company saves with temporary write and backup recovery | Source implemented; restart/corruption QA pending |
| Offline mode | Core career/company saves and gameplay logic are local; no account/backend dependency | Implemented by architecture |

## Missing or incomplete systems

These are not present as complete gameplay features yet and must not be described as finished:

1. **Articulated trailer simulation** — a visual/kinematic trailer prototype with wheels and capped steering articulation now exists; physical fifth-wheel constraint, trailer collision, load-dependent handling and realistic reverse jack-knifing remain incomplete.
2. **Full vehicle dynamics** — wheel/tire grip, suspension, axle loads, differential, torque curve calibration, retarder/engine braking, ABS/traction behaviour and service/repair.
3. **Traffic simulation** — physical traffic collision bodies, multi-vehicle awareness, safe gap selection, overtaking rules, junction behaviour, traffic lights, roundabouts, speed-limit adherence and emergency braking.
4. **Forward/rear awareness** — dedicated front/rear sensing with closing-speed calculation, blind-spot checks, lane-change risk scoring and clear dashboard alerts.
5. **European world and navigation** — connected road network, city-specific distances, junctions, roundabouts, tolls, service stations, rest areas, borders and GPS route guidance. The current road is a single straight greybox motorway loop.
6. **Freight operations** — pickup and delivery depot triggers with load/unload interaction now exist; remaining work includes real depot yards, cargo mass/fragility, cargo loading animation, trailer selection, damage/late-delivery penalties, weigh stations and route-specific delivery confirmation.
7. **Company progression** — detailed AI driver schedules/routes, truck assignment UI, fleet maintenance, driver wages/skills, contracts for hired drivers, reputation-driven contract access and a full management screen.
8. **Presentation** — basic horn audio, rear indicator lights, day/night lighting and clear/cloudy/fog environment states now exist; production truck/trailer models, interiors, detailed road assets, animations, rain effects, engine/air-brake ambience and UI polish remain.
9. **Platform release** — physical Android install/performance/thermal testing, save/resume testing, and Windows x64 build.
10. **Release quality** — complete automated tests for persistence, traffic interactions, contract edge cases, economy exploits and long-session stability.

## Current priority order

- P0: Get CI green: import, headless scene, logic tests and ARM64 APK export.
- P1: Add and test forward/rear traffic sensing, safe lane-change decision logic and actual traffic collision bodies.
- P2: Add articulated trailer prototype and test forward/reverse handling.
- P3: Build route/depot/loading gameplay loop before expanding the map.
- P4: Expand company simulation, vehicle dynamics, audio, weather and production assets.
- P5: Android device acceptance, performance tuning and Windows export.

## Review rule

A source file or a passing headless test is not proof of full feature quality. Mark a feature complete only after its acceptance criteria pass. The user review should happen after the planned implementation and CI gates have stabilized; defects found in that review should be logged, fixed and retested in a separate review pass.
