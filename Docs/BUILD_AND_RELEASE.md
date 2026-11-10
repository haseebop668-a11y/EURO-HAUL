# Build and Release Pipeline

## Active engine
- Godot 4.4.1; active project root: `Godot/`.
- Unity source under `Game/` is retained as migration/reference material only.
- Android debug output: ARM64 APK, package ID `com.eurohaul.game`.
- Windows x64 debug export is built by the same CI workflow using the Windows Desktop preset.
- No Unity license or Unity account is required for the Godot pipeline.

## Automated GitHub Actions workflow
Workflow: `.github/workflows/godot-check.yml`

The workflow:
1. Checks out the repository and configures Java 17 plus Android SDK packages.
2. Downloads the exact Godot 4.4.1 export templates.
3. Creates a temporary debug keystore for internal testing only.
4. Imports the Godot project and fails if script parse/load errors appear in the log.
5. Runs the main scene headlessly and checks for the expected boot marker.
6. Exports an Android ARM64 debug APK.
7. Checks that the APK is non-empty, passes ZIP integrity validation, and contains the expected Android package metadata.
8. Exports a Windows x64 debug executable and verifies its PE `MZ` header.
9. Uploads Android and Windows artifacts as separate GitHub Actions artifacts for 14 days.

## Download a debug APK
1. Open the repository's Actions tab.
2. Open the latest successful **Godot validation and Android APK** run.
3. Under Artifacts, download `EURO-HAUL-Android-debug`.
4. Extract the ZIP and install `EURO-HAUL-debug.apk` on an ARM64 Android device.

A debug APK is not a Play Store release. It is signed with an ephemeral CI debug key and should be used for testing only. The Windows executable is also a debug build and still requires a real Windows launch test.

## Download the Windows debug build
1. Open the repository's Actions tab.
2. Open the latest successful **Godot validation and Android APK** run.
3. Under Artifacts, download `EURO-HAUL-Windows-x64-debug`.
4. Extract and run `EURO-HAUL.exe` on a Windows x64 PC.

## Build troubleshooting
- Missing export templates: confirm the template archive version exactly matches Godot 4.4.1 and is installed under `~/.local/share/godot/export_templates/4.4.1.stable/`.
- Missing Java/SDK: inspect the Java 17 and Android SDK setup steps and verify `JAVA_HOME`, `ANDROID_HOME`, and `ANDROID_SDK_ROOT`.
- Script parse errors: inspect the Import and runtime smoke-test logs; CI fails on GDScript parse/load errors.
- APK metadata/integrity failure: inspect the export log and the aapt/unzip validation output.

## Release gates
- [ ] Godot import completes with no GDScript parse/load errors.
- [ ] Headless main-scene smoke test passes.
- [ ] Android ARM64 APK exports and passes ZIP/package validation.
- [ ] APK installs and launches on the target physical Android device.
- [ ] Touch controls, steering, throttle, brake, shifting, signals and horn pass device testing.
- [ ] Career and company saves survive app restart and malformed-save recovery.
- [ ] Traffic and collision behaviour pass the QA plan.
- [ ] Ten-minute drive completes without crashes, stuck input or excessive thermal load.
- [ ] Windows x64 debug executable exports and passes PE-header validation.
- [ ] Windows x64 build launches on a real Windows PC.

Only mark a gate complete after the corresponding test actually ran. A successful headless run or APK export does not prove physical-device compatibility.
