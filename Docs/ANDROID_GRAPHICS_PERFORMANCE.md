# EURO HAUL — Android Graphics and Performance Budget

## Target hardware
Initial tuning target: RedMagic 5S Pro class hardware (Snapdragon 865 / Adreno 650, 8 GB RAM, 144 Hz display). This is a target profile, not a verified benchmark. Other Android devices must be supported through scalable settings.

## Frame-rate policy
- Default: 60 FPS cap for stable thermals and battery behaviour.
- Optional high-refresh modes: 90 FPS and 120 FPS only when the device reports support and a runtime performance check permits them.
- 120 FPS frame budget: 8.33 ms total per frame. A single spike can still cause visible stutter.
- Physics runs on a fixed timestep and must not speed up or change when render FPS changes.
- If thermal throttling or sustained frame-time misses occur, dynamically step down render scale, shadows and effects before reducing simulation correctness.

## Proposed graphics presets

| Setting | Low | Medium | High | Ultra |
|---|---|---|---|---|
| Render scale | 0.70–0.80 | 0.85–0.90 | 0.95–1.00 | 1.00 |
| Target FPS | 30/60 | 60 | 60/90 | 60/90/120 |
| Anti-aliasing | FXAA/off | 2x MSAA | 2x/4x MSAA | 4x MSAA |
| Shadow distance | 25 m | 45 m | 70 m | 90 m |
| Shadow resolution | Low | Low | Medium | Medium |
| Ambient occlusion | Off | Low | Low/Medium | Medium |
| Reflections | Probe only | Probe | Probe + limited planar | Probe + selective planar |
| Traffic density | Low | Medium | High | High (CPU budget permitting) |
| Foliage distance | Short | Medium | Long | Long |
| Texture quality | Half | Full | Full | Full |

These are starting ranges for profiling, not final tuned values. Avoid expensive real-time global illumination and full-resolution screen-space effects on the initial mobile target.

## Warm and soft art direction
- Use a restrained warm-neutral grade: natural daylight, slightly warm highlights, neutral shadows, muted asphalt and realistic vegetation.
- Preserve legibility and colour contrast for signs, indicators, brake lights and HUD.
- Use subtle bloom only on bright sources; avoid heavy vignette, chromatic aberration and sharpening.
- Use baked or mixed lighting for static scenery; reserve realtime lights for headlights, indicators and nearby dynamic sources.
- Provide brightness and colour-grading intensity controls where practical. Do not use colour grading to hide low-quality textures.

## CPU/GPU and memory rules
- Use LOD groups for trucks, trailers, buildings, vegetation and roadside props.
- Use occlusion culling in dense depot/town scenes and GPU instancing for repeated static props.
- Pool traffic vehicles and effects; avoid per-frame allocations and LINQ in hot loops.
- Limit active AI simulation by distance; distant traffic can use simplified movement while preserving lane occupancy.
- Stream scene sectors and release unused assets; avoid loading all map data at startup.
- Use compressed platform-appropriate textures and mipmaps; set maximum texture sizes by asset class.
- Avoid excessive transparent particles and overlapping UI canvases.
- Keep physics collider counts modest and use simple collision meshes for distant scenery.

## Measurement gates
Record, on each target device:
- Average, 1% low and 0.1% low FPS; frame-time distribution.
- CPU main-thread, render-thread and GPU frame time.
- Peak and steady-state memory.
- Device temperature, thermal state, battery drain and throttling.
- Load time, sector-streaming hitches and save/load latency.
- 30-minute continuous drive with traffic, lights, weather and a populated depot.

A build is not certified for 120 FPS until measured on the physical device under sustained load. A menu showing “120 FPS” is not proof of 120 FPS gameplay.
