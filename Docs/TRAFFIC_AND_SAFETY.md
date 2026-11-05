# Road Traffic, Signals and Driver Safety

## Driver signals
`TruckSignals` provides left/right indicators, hazard lights and horn. Indicator state is exposed for the dashboard. Assign amber lamp renderers using a material that supports the configured emission property. Assign a properly licensed horn audio clip to an AudioSource. The horn broadcast can notify nearby `TrafficHornReceiver` components; each receiver can play a response sound or trigger an authored reaction.

Mobile UI can call `TruckIndicatorInput.LeftIndicator`, `RightIndicator`, `HazardLights`, `SignalOff`, `HornDown` and `HornUp`. PC prototype keys: Z/X indicators, H hazards, B horn (hold).

## Traffic awareness
`TruckTrafficSensors` uses non-allocating sphere casts to estimate:
- Forward gap and closing speed.
- Rear gap and closing speed.
- Left/right blind-spot occupancy.
- Left/right rear-quarter gap and closing speed.

Configure all sensor origins outside the player's own collider. Orient blind-spot origins sideways and rear-quarter origins diagonally backward into the adjacent lane. Put vehicles on a dedicated layer and set the sensor mask to include that layer. Exclude the player's own vehicle layer where possible.

`CanSafelyChangeLeft/Right` is a conservative game gate based on blind-spot occupancy, rear-quarter gap and time-to-collision. It is not a guarantee that a real lane change is safe. The UI should show “check mirrors / wait” when false, and should never force the steering input.

## Stopping distance and collision warnings
The deterministic model uses:
- Reaction distance: speed × assumed reaction time.
- Braking distance: speed² / (2 × assumed deceleration).
- Additional configurable safety margin.
- Time-to-collision: gap / positive closing speed.

Default reaction time, deceleration and margins are game-design assumptions, not legal or certified vehicle data. Real braking depends on load, grade, tires, brake temperature, road friction, weather and air-brake condition. Do not present these estimates as real driving advice.

## Curves and high-speed turns
`RoadCurveSpeedAdvisor` estimates an upcoming curve radius from three authored road-centre markers. The advisory uses a configurable lateral-acceleration limit to calculate a recommended speed. This is a simplified planar model; it does not yet model superelevation, tire load transfer, trailer off-tracking, road camber, suspension roll or friction changes. The game should warn early and visibly, but it must not claim a guaranteed safe speed.

## Traffic vehicle AI
`TrafficRoute` stores ordered lane-centre waypoints. `TrafficVehicleAI` follows its assigned route, limits speed through waypoint curvature estimates, senses the vehicle ahead, maintains a speed-dependent following gap and can sound a courtesy horn when approaching a slow obstacle.

Automatic overtaking is intentionally disabled. Passing requires a separate lane-occupancy model, road-marking/legal-overtake rules, opposing-traffic prediction, sufficient sight distance and an explicit return-to-lane check. Do not enable AI overtakes until those systems exist and are tested.

## Planned next engineering layers
- Mirror cameras and side/rear radar visualization.
- Road sign and speed-limit recognition from authored world data.
- Overtake legality and opposing-lane prediction.
- Brake/retarder/air-pressure simulation and load-sensitive braking.
- Trailer articulation, off-tracking and rollover envelope.
- Weather, wet/icy friction, road grade and tire temperature.
- Perception/audio accessibility settings and event logs.


## High-speed steering envelope
The player truck and traffic AI limit wheel steering angle using a speed-dependent lateral-acceleration envelope. At higher speed, the same steering input produces a smaller wheel angle; the dashboard reports when the requested steering angle is being limited. This prevents abrupt high-speed steering cuts in the prototype. The configured lateral acceleration is a gameplay tuning value and must be validated with loaded/unloaded vehicle and trailer tests.

## Collision and recovery
`TruckCollisionMonitor` records impact speed and a gameplay damage estimate. Moderate impacts activate hazards; stronger impacts apply emergency braking; critical impacts lock the truck until `RepairAtService()` is called by a future service/repair interaction. This is not a physically accurate crash or injury model.
