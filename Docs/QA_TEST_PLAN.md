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
