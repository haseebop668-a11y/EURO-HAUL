# EURO HAUL — Game Design and Content Specification

## Product target
EURO HAUL is an offline-first, single-player European heavy-haul trucking career simulator for Android first, with Windows as a later build target. The current Unity repository is an engineering prototype, not a finished game.

## Non-negotiable product principles
- Driving should feel weighty and predictable: truck mass, wheelbase, load, tires, suspension, braking, gearbox, differential and trailer articulation all affect handling.
- Simulation assists must inform the driver; they must not secretly steer or brake except for explicitly modelled emergency systems.
- Career, saves, contracts, depots and core driving must work offline.
- Warm, soft visual direction: restrained contrast, warm daylight, natural greens, muted asphalt, readable dashboard, no excessive bloom, oversharpening or saturated neon.
- Quality settings must scale visual cost without changing vehicle physics or simulation fairness.
- Use original, commissioned, public-domain or explicitly licensed assets only. Keep an asset register with source, author, license, attribution and allowed platforms.

## Content roadmap

### Phase 0 — Engineering prototype (current repository state)
- Truck Lab test road, one player vehicle, waypoint traffic, touch controls, camera, safety sensors and basic powertrain.
- This phase is not a commercial-quality truck model or a complete game world.

### Phase 1 — Playable vertical slice
- One polished fictional European tractor unit with interior/exterior camera.
- One configurable semi-trailer with articulated physics and stable reverse manoeuvres.
- Cargo attach/detach, depot loading/unloading interaction, delivery validation and contract payout.
- One compact fictional region with a motorway, rural road, town approach, service area, toll plaza and logistics depot.
- Save/load, pause, settings, tutorial, damage and repair loop.
- Traffic lanes, junctions, traffic lights, speed limits, road signs and basic police/penalty rules.
- Android HUD and touch layout designed for landscape and multi-touch.

### Phase 2 — Career and logistics
- Company reputation, driver level, bank balance, fuel, tolls, repairs, fines and contract deadlines.
- Cargo classes: general freight, refrigerated, container, timber, machinery and oversize loads; each has mass, dimensions, fragility and handling rules.
- Owned/leased trucks, trailers, upgrades, garages and hiring progression.
- Loading and unloading states with cargo straps/doors/reefer controls where appropriate.
- Fatigue/rest stops only if implemented as a transparent, configurable gameplay mechanic.

### Phase 3 — European world expansion
- Build the world from legally usable road network data and authored 3D art, not scraped satellite imagery.
- Add countries/regions incrementally; each region needs road geometry, signs, junctions, towns, depots, service areas, toll systems and country-specific rules.
- OpenStreetMap-derived data may be used subject to ODbL obligations and attribution. Review whether distributed databases or adapted datasets trigger share-alike obligations before release.
- Avoid claiming centimetre-perfect real-world reproduction. Prioritise believable road topology and driving scale over copying every building.

### Phase 4 — Vehicle and world variety
- Add truck classes progressively: 4x2, 6x2, 6x4 and heavy-haul configurations.
- Real manufacturer names, badges, cab designs, sounds and exact model replicas require permission/licensing. Until licensed, use distinctive fictional makes and original designs rather than unapproved branded replicas.
- Add trailer variants, cargo equipment, weather, day/night, seasonal lighting and dynamic road works.
- Add more traffic vehicle types and behaviours only after profiling their CPU cost.

## Core simulation requirements

### Vehicle
- Separate engine torque curve, RPM governor, gearbox ratios, clutch, differential, retarder/engine brake, service brakes, parking brake, tires and suspension.
- Model gross vehicle mass and centre of gravity from tractor, trailer and cargo.
- Trailer articulation, fifth-wheel constraints, jackknife risk and trailer wheel braking.
- Tire grip changes with wetness and surface type; avoid arcade-like instant grip.
- Gear shifts require appropriate clutch and speed/RPM conditions.
- All forces and state changes must be deterministic enough for repeatable tests.

### Traffic and roads
- Lane graph with explicit lane direction, width, speed limit, curvature, grade, junction connectivity and restrictions.
- AI uses safe following distance, braking distance, speed limits, signals, lane-change checks and right-of-way.
- Toll plaza: advance warning signs, lane selection, barriers, payment/collection interaction, queue spacing, stop line, barrier state, re-entry acceleration and toll record in career ledger.
- Rest/service areas: parking bays, fuel, repair, rest and route re-entry.
- Road rules are configurable per country and must be reviewed against authoritative sources before claiming accuracy.

### Performance and stability
- Keep vehicle physics independent of render frame rate; use fixed-step physics and interpolate visuals.
- Pool traffic, particles, UI notifications and transient props.
- Use LODs, occlusion culling, GPU instancing, baked lighting and limited shadow distance.
- Stream world sectors around the player; never load the entire continent into memory.
- Profile CPU, GPU, memory, thermal throttling, battery use and frame pacing on actual target phones.
- Treat 120 FPS as a device-dependent high-refresh mode, not a universal guarantee.

## Definition of done for a release candidate
- Unity import/compile succeeds with zero errors and reviewed warnings.
- Automated EditMode and PlayMode tests pass.
- Android ARM64 APK installs, launches, saves and resumes on physical devices.
- No reproducible blocker, soft-lock, save corruption or missing critical asset.
- 30-minute thermal/performance run on target hardware with frame-time, memory and temperature evidence.
- Driving, traffic, cargo, tolls, repair, career, offline mode and settings pass documented QA.
- All third-party licenses, attributions, privacy disclosures and store requirements are checked.


## Implemented career prototype (offline)
- Local JSON career save in `Application.persistentDataPath`, with a previous-save backup.
- Driver level, XP, Game Cash, completed-delivery count and driven distance.
- Generated fictional freight offers with cargo, origin/destination labels and reward values.
- Accept contract from the in-game career HUD; delivery progress is measured from player-truck movement in the Truck Lab prototype.
- Delivery reward is credited locally and a new offer is generated.
- The menu exposes Continue Career and New Career.
- This is a first vertical-slice career loop, not the final route/network simulation. The current destination is distance-based on the test track; city-to-city navigation, cargo attachment/loading, fuel, damage, company management, and server/cloud save are not yet implemented.
