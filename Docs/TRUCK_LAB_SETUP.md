# Truck Lab — Unity Scene Setup

## Prerequisites
- Unity 6 LTS installed through Unity Hub on a PC.
- Universal 3D (URP) project.
- Repository scripts copied into the Unity project's Assets directory.
- Active Input Handling set to Both or Input Manager (Old) for the temporary keyboard adapter.

## Scene assembly
1. Create and save a scene named TruckLab.
2. Create a Plane named TestGround; scale it to (20, 1, 20).
3. Create a Cube named TruckBody; approximate scale (2.5, 2.5, 6), positioned above the ground.
4. Add a Rigidbody to TruckBody; start with mass 8500, drag 0.05, angular drag 0.5, and Interpolate interpolation.
5. Create four wheel child objects at local positions: front left (-1.1, -0.7, 1.8), front right (1.1, -0.7, 1.8), rear left (-1.1, -0.7, -1.8), rear right (1.1, -0.7, -1.8).
6. Add a WheelCollider to each wheel object. Start with radius 0.5 and suspension distance 0.25. Tune spring, damper, and friction in Play Mode.
7. Create visible wheel meshes with pivots centered on each axle.
8. Add TruckController to TruckBody and assign all four WheelCollider references.
9. Add WheelVisualSync and assign each collider to its matching visible wheel.
10. Add KeyboardTruckInput and assign the TruckController.
11. Add a Camera with FollowCamera and assign TruckBody as target.
12. Add TruckLab to the build scene list.

## Controls
- W / Up: throttle forward.
- S / Down: reverse.
- A / Left and D / Right: steering.
- Space: service brake.

## Tuning notes
Torque, brake, center-of-mass, mass, friction, suspension and wheelbase values must be tuned together. This is a first prototype, not a validated heavy-truck physics model.

## Known limitations
No gearbox, clutch, engine RPM, air-brake pressure, retarder, trailer, ABS, traction control, mobile touch input, production truck mesh or road environment yet. The scene must be assembled and tested in Unity Editor.
