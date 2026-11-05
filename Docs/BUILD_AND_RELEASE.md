# Build and Release Pipeline

## Engine baseline
- Unity 6.3 LTS, editor version 6000.3.0f1.
- Unity project root: `Game/`.
- Android output: APK (not Android App Bundle).
- Android architecture: ARM64.
- Windows output: Windows x64 player.
- Automated validation: Unity EditMode tests before platform builds.

## Scene generation
The Editor bootstrap in `Game/Assets/Editor/EuroHaulSceneBootstrap.cs` creates a basic Truck Lab test scene on first Unity Editor startup and registers it in Build Settings. The scene contains a player truck, two same-direction traffic trucks, one oncoming truck, indicators, horn, sensor origins, a camera, road markings and mobile touch buttons. It is a prototype test track, not the full European map.

## GitHub Actions
Workflow: `.github/workflows/unity-ci.yml`

It is manual-only (`workflow_dispatch`) so an unconfigured Unity license does not break ordinary source commits. It:
1. Checks out the repository.
2. Runs EditMode tests.
3. Builds Android APK and Windows x64 after tests pass.
4. Uploads test reports and build artifacts.

## Required repository secrets
A Unity Personal license activation is required by the GameCI runner. In GitHub, open repository Settings → Secrets and variables → Actions → New repository secret.

- `UNITY_LICENSE`: Unity license file contents.
- `UNITY_EMAIL`: Unity account email if required by the activation flow.
- `UNITY_PASSWORD`: Unity account password if required by the activation flow.

Never put license values in source files, workflow YAML, issues, or chat. Do not share these values with anyone.

## Run a build
1. Configure the Unity secrets above.
2. Open the repository Actions tab.
3. Select “Unity validation and builds”.
4. Choose “Run workflow”.
5. Wait for EditMode tests and both platform builds.
6. Download the Android and Windows artifacts from the completed workflow run.

## Important status
The workflow definition and automatic test scene generator are committed, but no CI run has been executed in this repository yet. No APK or Windows executable is currently verified. A valid Unity license secret and successful CI results are required before claiming a build.

## Release gates
- All EditMode tests pass.
- Android ARM64 APK installs and launches on a physical device.
- Windows x64 build launches.
- No missing scripts or serialized references.
- Driving, traffic sensing, signals, collision warnings, and mobile controls pass manual play tests.
- Performance and thermal tests are recorded on a physical Android device.
