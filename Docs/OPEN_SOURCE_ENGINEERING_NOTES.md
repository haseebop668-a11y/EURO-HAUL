# Open-source engineering research

Research was used to study architecture and workflows, not to copy unlicensed game assets.

## References reviewed

- **Godot Truck Town demo** — https://github.com/godotengine/godot-demo-projects/tree/master/3d/truck_town  
  Official Godot demo for VehicleBody3D, multi-vehicle/trailer setups and mobile touch behavior. Its README identifies GDScript and MIT licensing for the demo project. Use as a physics reference; inspect exact file-level notices before copying assets.
- **OpenCity** — https://github.com/webgl-games/opencity  
  MIT code / CC0 assets according to its README and credits. Useful engineering patterns: procedural greybox world generation, separate car/truck scenes, lane traffic, follow camera, HUD, fixed random seed for repeatable QA, and headless drive/screenshot scenarios. EURO HAUL does not copy its assets.
- **Vehicle Physics** — https://github.com/dicnunz/vehicle-physics  
  Godot driving sandbox demonstrating suspension/tire force modelling, mirrors, deformation and repeatable telemetry test scenarios. A reference for later physics iteration, not a drop-in validated truck simulation.
- **Godot Engine** — https://github.com/godotengine/godot  
  MIT-licensed engine; no Unity license dependency.

## Engineering decisions for EURO HAUL

1. Build a reproducible greybox vertical slice before detailed art.
2. Keep truck controller, traffic, career/save, HUD and world generation in separate scripts.
3. Use deterministic test scenarios and headless CI before claiming a subsystem is stable.
4. Use primitive geometry and original procedural materials while gameplay is being validated.
5. Do not copy a project's GPL code into this proprietary project unless the project owner deliberately accepts GPL distribution obligations. Do not copy non-redistributable models, sounds or textures.
6. Pin the Godot version and matching export templates. The Android APK pipeline must use the same version.
7. Profile on actual Android hardware; frame-rate targets are goals, not guarantees.

## Current prototype limitation

The current Godot branch is a greybox driving/career foundation. It is not a full European world, production truck model, complete traffic simulation, full company management system, or a verified release build.
