# EURO HAUL — Brand and UI Design System

## Direction
A premium, calm European logistics identity: midnight navy surfaces, steel blue controls, warm brass-gold highlights and clean white typography. The interface should feel like a modern vehicle instrument cluster, not a neon arcade game.

## Identity assets
- Source icon: `Game/Assets/Branding/euro-haul-icon.svg`.
- Working title: EURO HAUL.
- Tagline: Drive. Deliver. Build your fleet.
- Vehicle branding: original fictional designs only until real manufacturer marks and cab designs are licensed.
- The SVG is an editable vector source. Export platform-specific PNG/adaptive icon sizes from this master during the Unity Android Player Settings stage.

## Tokens
| Token | Hex | Use |
|---|---|---|
| Midnight | `#06111F` | Main background |
| Deep Navy | `#102B45` | Secondary background |
| Panel | `#0E1B2B` | Cards and modal panels |
| Steel Blue | `#1461C7` | Primary actions and focus |
| Brass Gold | `#E8B85B` | Brand accent, key values |
| White | `#EEF4FA` | Main text |
| Muted | `#9EB5C9` | Secondary text |
| Success | `#3D9B78` | Delivery success only |
| Warning | `#D79B3D` | Caution and alerts |
| Danger | `#C65A55` | Critical warnings |

## Layout rules
- Landscape-first 16:9 safe-area layout; scale with screen size.
- Keep critical driving controls reachable without covering mirrors, road centre or warning area.
- Primary action is visually dominant; secondary actions use low-contrast panels.
- Use 8-point spacing increments, 12–20 px corner radius, restrained shadows.
- Dashboard typography must remain legible at phone distance. Avoid dense text during driving.
- Settings and career screens may use more information density than the driving HUD.

## Core screens
1. Main menu: logo/title, Start Driving, Settings, Exit. Future Continue/New Career entries appear only when save/career flows exist.
2. Driving HUD: speed, gear, RPM, route cue, warning stack, indicators and minimal touch controls.
3. Pause: Resume, Settings, Return to Main Menu.
4. Settings: quality preset and FPS cap; future control layout, audio, accessibility and language tabs.
5. Career/garage/company/store: deferred until the career ledger and save model are implemented. Do not show fake balances, purchase buttons or withdrawal promises before those systems exist.

## Interaction and accessibility
- Large touch targets (minimum 56 dp; 64–88 dp preferred for driving).
- High-contrast text and non-colour-only warning states (icon + label).
- UI buttons must have pressed/selected feedback and must not trigger while the game is paused except menu actions.
- Persist only local preferences until a real save system is introduced.
- FPS choices are caps, not performance guarantees. Device thermal limits, GPU/CPU load and display refresh rate determine actual frame rate.

## Prototype implementation
`EuroHaulMenuUI.cs` creates the main menu, pause panel and quality/FPS settings with built-in uGUI. It deliberately avoids fake Continue, multiplayer, store, cashout and world-map actions. It is a functional prototype overlay, not the finished career UI.
