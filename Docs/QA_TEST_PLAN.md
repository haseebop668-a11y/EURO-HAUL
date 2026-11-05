# QA Test Plan — Truck Lab

## Editor smoke tests
- [ ] Project imports with no C# compiler errors.
- [ ] All serialized wheel references are assigned.
- [ ] Truck Rigidbody is non-kinematic.
- [ ] Each WheelCollider contacts the ground.
- [ ] Wheel meshes follow collider poses.
- [ ] Camera follows the truck without jitter.
- [ ] Keyboard steering works both directions.
- [ ] Forward throttle accelerates the vehicle.
- [ ] Reverse throttle moves the vehicle backward.
- [ ] Service brake slows and stops the vehicle.
- [ ] Vehicle respects configured forward/reverse speed limits.
- [ ] Disabling the input component clears throttle.

## Regression tests
- [ ] No missing scripts or references.
- [ ] No console exceptions during a 10-minute drive.
- [ ] Repeated stop/start cycles do not retain unintended torque.
- [ ] Vehicle remains controllable after collision and reset.

## Platform tests
- [ ] Windows x64 development build launches.
- [ ] Android ARM64 development build launches on a physical device.
- [ ] Android touch controls are added and tested before mobile release.
- [ ] Frame-time and thermal behaviour measured on target Android devices.

## Release rule
Only mark a checkbox complete after a real test run and recorded result. Repository presence or compilation alone does not prove playable behaviour.

## Manual powertrain test cases (not yet executed)
1. Start with first gear selected and clutch released; verify idle RPM is shown.
2. Apply throttle in first gear; verify rear wheels receive positive torque and speed rises.
3. Press clutch, shift up; verify gear changes only while clutch is sufficiently pressed.
4. Attempt a shift without clutch; verify the shift is rejected.
5. Select neutral with clutch pressed; verify drive torque becomes zero while engine RPM can rise with throttle.
6. Select reverse with clutch pressed; apply throttle and verify reverse wheel torque and speed limiting.
7. Apply service brake; verify all four wheel brakes receive brake torque.
8. Verify HUD speed, RPM, gear and clutch values follow telemetry.
9. On Android, wire touch press/release events and verify controls return to neutral on component disable.
10. Verify steering input clamps to -1..1 and throttle/brake/clutch inputs remain within their documented ranges.

These are planned checks only. They have not been run in Unity Editor, on a device, or in an automated test runner. A Unity project/scene and Android build pipeline are still required before APK validation.


## Traffic and road-safety tests (not yet executed)
1. Confirm forward and rear sensors ignore the player's own colliders.
2. Place a slower vehicle ahead and verify following distance increases with player speed.
3. Place a fast vehicle behind and verify rear-approach warning appears before it reaches the truck.
4. Place a vehicle in each blind spot and verify lane-change clearance is denied.
5. Place an oncoming vehicle in the passing lane and verify overtaking clearance is denied.
6. Confirm right-lane changes are denied on the generated test road because the right side is a shoulder, not a traffic lane.
7. Confirm indicator lamps blink and hazard mode flashes both sides.
8. Hold the horn and verify nearby traffic receivers hear and acknowledge it; release the horn and verify audio stops.
9. Verify a rear AI vehicle sounds its courtesy horn only when approaching a slow obstacle within the configured gap.
10. Verify stopping-distance warnings respond to speed and front gap.
11. Verify curve speed advice changes with authored road curvature markers.
12. Verify a moderate impact activates hazards and emergency braking; a critical impact locks the truck until service repair.
13. Verify mobile controls support simultaneous clutch and reverse press, and release safely when a pointer exits a button.
14. Verify keyboard and touch adapters are mutually exclusive on the selected platform.

## Build pipeline tests (not yet executed)
- [ ] Unity EditMode test job succeeds on Unity 6000.3.0f1.
- [ ] Android ARM64 APK build succeeds and artifact uploads.
- [ ] Windows x64 build succeeds and artifact uploads.
- [ ] APK installs and launches on a physical Android device.
- [ ] Android touch buttons are tested on the target device.
- [ ] Ten-minute traffic drive completes without console exceptions.
