# EURO HAUL Architecture

## Runtime boundaries
- Driving: vehicle input, vehicle dynamics, wheel visuals.
- Camera: follow and future cockpit cameras.
- Logistics: cargo, pickup, loading, delivery and unloading.
- Career: contracts, progression and company ownership.
- World: roads, depots, navigation and traffic.
- Persistence: offline profile and career saves.
- Platform: Android touch, PC keyboard/controller, store and ad adapters.

## Dependency rules
1. Core gameplay must not depend on advertising SDKs.
2. Core career must work without network access.
3. Platform-specific code must be behind interfaces.
4. Content definitions must be data-driven and validated.
5. Third-party content must have recorded source, license and attribution.
6. Never commit credentials or private keys.

## Initial prototype
Truck Lab uses Unity WheelColliders and Rigidbody as a first playable foundation. It is a tuning prototype, not a validated commercial-vehicle simulation.

## Future vehicle model
Introduce separate components for engine torque curve, gearbox, clutch, service brake, retarder, differential, tire friction, suspension and trailer articulation. Keep subsystems independently testable.

## Persistence
Use versioned local saves. Validate input and write saves atomically where possible. Introduce SQLite when career data requires relational queries; avoid premature dependencies.

## World and map data
Use fictional depots and generated cargo contracts initially. Review OpenStreetMap-derived data for ODbL compliance and attribution. Commercial map tiles, truck models, logos, sounds and real carrier data require separate license checks.
