# EURO HAUL
European Trucking Career Simulator for Android and Windows.

## Status
Prototype 001 source foundation. These files are starter scripts and documentation, not a complete Unity project, playable build, or tested APK.

## Principles
- Offline-first single-player career.
- Shared Unity project for Android and Windows.
- Optimized realistic 3D.
- Original or properly licensed assets only.
- Fictional logistics operators and generated freight contracts.
- No mandatory network connection for core gameplay.

## First milestone: Truck Lab
- Four WheelColliders.
- Front axle steering and rear axle drive.
- Acceleration, reverse, and service braking.
- PC keyboard input.
- Follow camera and wheel mesh synchronization.
- Flat test scene.

## Unity setup
1. Install a supported Unity 6 LTS editor using Unity Hub on a PC.
2. Create a Universal 3D (URP) project named EURO-HAUL.
3. Copy Game/Assets/ from this repository into the Unity project's Assets/ directory.
4. Set Project Settings > Player > Active Input Handling to Both while the prototype uses the legacy UnityEngine.Input keyboard adapter.
5. Follow Docs/TRUCK_LAB_SETUP.md.
6. Assign all serialized references in the Inspector.

## Controls
| Input | Action |
|---|---|
| W / Up | Accelerate |
| S / Down | Reverse |
| A / Left | Steer left |
| D / Right | Steer right |
| Space | Service brake |

## Layout
- Game/Assets/Scripts/Driving: vehicle control and input.
- Game/Assets/Scripts/Camera: camera behaviour.
- Docs: architecture and setup instructions.

## Limitations
Unity scene assets, truck meshes, wheel colliders, materials, lighting, platform build support, and editor-side testing still need to be completed in Unity Editor. No build or runtime test is claimed.

## License
All rights reserved by the project owner unless a file explicitly states otherwise. Third-party assets must retain their own licenses and attribution.
