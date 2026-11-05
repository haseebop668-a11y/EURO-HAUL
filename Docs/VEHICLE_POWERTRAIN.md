# Manual Powertrain and Controls

## Components
- `TruckPowertrain`: engine RPM response, diesel-style prototype torque curve, clutch engagement, reverse/neutral/forward ratios and final drive.
- `TruckController`: applies powertrain torque to the rear WheelColliders and exposes read-only dashboard telemetry.
- `TruckDashboardHUD`: temporary built-in IMGUI speed/RPM/gear/clutch display.
- `KeyboardTruckInput`: PC keyboard adapter.
- `TouchTruckInput`: UI-event adapter for mobile controls.

## Truck prefab wiring
1. Add `Rigidbody`, four `WheelCollider` objects and `TruckController` to the truck root.
2. Add `TruckPowertrain` and `TruckDashboardHUD` to the same root.
3. Assign the four WheelColliders in `TruckController`. Assign the powertrain reference or leave it empty for automatic component lookup.
4. Add either `KeyboardTruckInput` or `TouchTruckInput` and assign the truck controller.
5. The initial gear is first gear and the clutch starts released.

## Keyboard
| Key | Action |
|---|---|
| W / Up | Throttle |
| A / D or Left / Right | Steering |
| Space | Service brake |
| C (hold) | Clutch pedal |
| E | Shift up (clutch must be held) |
| Q | Shift down (clutch must be held) |
| N | Select neutral (clutch must be held) |
| R | Select reverse (clutch must be held) |

Use the clutch before shifting. Select reverse with R, then use W to reverse. Select first gear with E after reverse/neutral as appropriate.

## Mobile UI event wiring
Use Unity UI Buttons with PointerDown/PointerUp events (via EventTrigger or a small pointer relay) for:
- AccelerateDown / AccelerateUp
- ReverseDown / ReverseUp
- BrakeDown / BrakeUp
- ClutchDown / ClutchUp
- GearUp / GearDown / Neutral

Connect a steering joystick's normalized horizontal value to `SetSteering(float)`, and call `ClearSteering()` when released. ReverseDown requests reverse gear; it will only select when the clutch pedal is pressed. A proper pointer relay and responsive Canvas layout are still required.

## Prototype limitations
This is a lightweight arcade-simulation foundation, not a validated heavy-truck drivetrain. It currently omits engine stall, starter/ignition, torque converter, synchronizer wear, gear grinding, gearbox damage, retarder, differential simulation, realistic tire curves, and drivetrain thermal behaviour. The HUD is a temporary IMGUI overlay, not the final designed dashboard. Values require in-editor tuning and vehicle testing.
