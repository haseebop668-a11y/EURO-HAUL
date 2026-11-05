# Truck Lab — Unity Scene Setup

## Prerequisites
- Unity 6.3 LTS editor 6000.3.0f1.
- Open the Unity project directly from the repository's `Game/` folder.
- Android Build Support with SDK/NDK and OpenJDK modules for Android builds.
- Active Input Handling set to Both or Input Manager (Old) for the temporary keyboard adapter.

## Player truck assembly
1. Create a road test scene and a ground/road collider.
2. Create a truck root with a BoxCollider and Rigidbody. Set a realistic prototype mass and keep the centre of mass low.
3. Add four WheelCollider children at the axle positions; assign front-left, front-right, rear-left and rear-right to TruckController.
4. Add TruckPowertrain, TruckController, TruckSignals, TruckTrafficSensors, TruckSafetyAdvisor and TruckDashboardHUD to the truck root.
5. Add WheelVisualSync and assign each WheelCollider to its visible wheel mesh.
6. Add KeyboardTruckInput for desktop, or TouchTruckInput and TruckIndicatorInput for mobile.
7. Add FollowCamera to the camera and assign the truck root as its target.
8. Add front/rear sensor origins, left/right blind-spot origins, and left/right rear-quarter origins. Orient the side origins toward the lane they must scan.
9. Configure the traffic layer mask so it detects vehicles and road hazards but excludes the player's own truck and irrelevant scenery.
10. Add a RoadCurveSpeedAdvisor and assign three road centre-line markers (near, apex, exit) for each upcoming curve.

## Traffic vehicle assembly
1. Create a separate traffic vehicle root with Rigidbody and body collider.
2. Add four WheelColliders and TrafficVehicleAI; assign its route and wheel references.
3. Create a TrafficRoute object with ordered lane-centre waypoint transforms.
4. Put all traffic vehicles on a dedicated layer included in the player's traffic sensor mask.
5. Configure a front sensor origin at the front bumper and ensure it does not begin inside the vehicle collider.
6. Do not enable automatic overtaking in this prototype. The AI follows its authored lane and maintains a speed-dependent following gap.

## PC controls
| Key | Action |
|---|---|
| W / Up | Throttle |
| A / D or Left / Right | Steering |
| Space | Service brake |
| C (hold) | Clutch |
| E / Q | Gear up / down (clutch required) |
| N / R | Neutral / reverse (clutch required) |
| Z / X | Left / right indicator |
| H | Hazard lights |
| B (hold) | Horn |

## Mobile controls
Use Unity UI Buttons and connect PointerDown/PointerUp events:
- AccelerateDown / AccelerateUp
- ReverseDown / ReverseUp
- BrakeDown / BrakeUp
- ClutchDown / ClutchUp
- GearUp / GearDown / Neutral
- TruckIndicatorInput.LeftIndicator / RightIndicator / HazardLights / SignalOff
- HornDown / HornUp

Connect a normalized steering joystick to TouchTruckInput.SetSteering(float), and release it with ClearSteering(). Reverse selection requires the clutch to be held; the reverse pedal will not drive unless reverse gear was actually selected.

## Tuning and validation
Wheel friction, suspension, steering geometry, brake torque, mass distribution and drivetrain ratios must be tuned together. Collision sensing requires correct transforms, collider sizes and layer masks. The default deceleration, reaction time, sensor ranges and curve limits are prototype assumptions and must be calibrated against the intended gameplay feel.

## Scope boundary
This scene is a physics and traffic testbed, not a finished European trucking career game. Trailer articulation, cargo loading, road signs, speed-limit enforcement, navigation, economy, career progression, save/load and production UI are separate milestones.
