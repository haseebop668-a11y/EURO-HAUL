# EURO HAUL QA Test Plan — Godot 4 Android Vertical Slice

## Automated CI gates
- [ ] Godot 4.4.1 imports the project with no GDScript parse/load errors.
- [ ] Main scene starts headlessly and prints the expected boot marker.
- [ ] Android export templates match the engine version.
- [ ] Android ARM64 debug APK exports successfully.
- [ ] APK passes ZIP integrity and Android package metadata checks.
- [ ] APK artifact uploads and can be downloaded from the workflow run.

## Runtime and scene
- [ ] Main scene creates world, truck, camera, traffic, career manager, company manager and HUD without runtime errors.
- [ ] Camera remains behind/above the truck and does not lose its target.
- [ ] Truck remains on the motorway and does not fall through the road.
- [ ] Roadside barriers and scenery do not create unintended blocking collisions.
- [ ] App pause/resume does not leave the simulation in a broken state.
- [ ] Android Back and the pause button pause/resume the simulation while keeping HUD controls responsive.
- [ ] App focus loss clears held touch inputs and saves career/company progress.

## Driving and input
- [ ] Keyboard steering, throttle and brake work in the desktop/headless-compatible test environment.
- [ ] Touch steering, throttle and brake respond to press and release.
- [ ] Simultaneous throttle/brake and left/right inputs resolve safely.
- [ ] Input returns to neutral after button release, focus loss and app pause.
- [ ] Gear up/down only works with clutch engaged.
- [ ] Neutral and reverse selection obey speed/clutch constraints on keyboard and touch controls.
- [ ] Forward and reverse speed limits are respected.
- [ ] Braking stops the truck without unintended acceleration.
- [ ] Steering response is reduced at high speed to respect the lateral-acceleration limit.
- [ ] Collision damage increases only on actual blocking collisions and cannot exceed 100%.

## Traffic and road safety
- [ ] Traffic vehicles stay within their assigned lanes and road bounds.
- [ ] Traffic does not spawn directly inside the player truck.
- [ ] Front/rear sensing ignores the player's own collision shapes.
- [ ] Following distance and rear-approach warnings respond to speed and gap.
- [ ] Blind-spot and lane-change warnings are correct before lane changes are implemented.
- [ ] Indicator and hazard states update the HUD correctly.
- [ ] Rear indicator lights blink at a stable cadence and hazards blink both sides.
- [ ] Horn button/key plays the generated horn effect without overlapping audio glitches.
- [ ] Horn control produces feedback and does not remain stuck after release.
- [ ] Traffic collision reactions do not cause uncontrolled physics or repeated damage loops.
- [ ] Player-aware traffic moves to a safe adjacent lane when the player closes from behind.
- [ ] Traffic does not change into an occupied adjacent lane and adjusts speed when blocked.
- [ ] Traffic returns to its base lane only after a clear-gap delay.
- [ ] Forward and rear proximity reports identify the correct relative zone and closing direction.
- [ ] Trailer articulation responds to steering in forward/reverse and stays within the configured angle limit.
- [ ] Traffic collision bodies interact with the player truck without jitter or tunnelling.

## Offline career and persistence
- [ ] New profile creates a valid freight offer.
- [ ] Accepting a contract moves it from offered to active exactly once.
- [ ] Cargo can only be loaded at the pickup depot and cannot be loaded twice.
- [ ] Contract distance advances only while cargo is loaded.
- [ ] Cargo cannot be unloaded before required route distance is complete.
- [ ] Delivery depot unloads cargo, pays reward once and generates the next offer.
- [ ] Distance progress is monotonic and does not jump when a frame has a large position delta.
- [ ] Road-segment recycling keeps the truck on the motorway and does not add teleport distance to a freight contract.
- [ ] Delivery completion pays the reward once, awards XP once and generates the next offer.
- [ ] Level progression handles multiple level-ups from one XP award.
- [ ] Career save survives app restart.
- [ ] Missing, malformed and unsupported-version save files do not crash the game.
- [ ] Malformed contract fields are rejected and route progress is clamped to the contract distance.
- [ ] Save writes use a recoverable/atomic strategy.

## Company economy and persistence
- [ ] Company can only be founded once and deducts the founding fee once.
- [ ] Fleet purchase and driver hiring require sufficient company balance.
- [ ] Company balance does not drain every rendered frame.
- [ ] Operating costs are charged once per simulated day (300 seconds), with elapsed time handled correctly.
- [ ] Company delivery revenue and reputation are credited once.
- [ ] New company receives the configured seed balance and can buy its first truck.
- [ ] Hired-driver count cannot exceed owned fleet trucks.
- [ ] Each active hired driver generates one simulated delivery per game day, with costs/revenue applied once.
- [ ] Company save survives app restart and malformed data does not crash the game.

## Physical Android device acceptance
- [ ] Install and launch on an ARM64 Android phone.
- [ ] Verify portrait/landscape behaviour and HUD safe-area layout.
- [ ] Verify touch controls at native resolution and during simultaneous touches.
- [ ] Verify app background/foreground and screen lock/resume.
- [ ] Complete a ten-minute drive without crash, stuck controls or save corruption.
- [ ] Record average frame rate, frame-time spikes, memory use and device temperature.

## Release rule
A checked-in script is not proof that a feature works. A headless smoke test is not a physical-device test. Record the workflow run, artifact, device model, Android version and observed result before marking each release gate complete.
