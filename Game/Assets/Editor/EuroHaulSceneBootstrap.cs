using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using EuroHaul.Driving;
using EuroHaul.CameraSystem;
using EuroHaul.Traffic;

namespace EuroHaul.Editor
{
    [InitializeOnLoad]
    public static class EuroHaulSceneBootstrap
    {
        private const string ScenePath = "Assets/Scenes/TruckLab.unity";

        static EuroHaulSceneBootstrap()
        {
            EditorApplication.delayCall += EnsureScene;
        }

        [MenuItem("EURO HAUL/Generate Truck Lab Scene")]
        public static void EnsureScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!File.Exists(ScenePath)) CreateScene();
            ConfigureBuildScenes();
        }

        private static void CreateScene()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material roadMat = CreateMaterial("Road", new Color(0.12f, 0.13f, 0.14f));
            Material bodyMat = CreateMaterial("TruckBody", new Color(0.12f, 0.28f, 0.42f));
            Material trafficMat = CreateMaterial("Traffic", new Color(0.72f, 0.22f, 0.12f));
            Material lampMat = CreateMaterial("IndicatorAmber", new Color(1f, 0.35f, 0.02f));

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "WorldGround";
            ground.transform.position = new Vector3(0f, -0.22f, 0f);
            ground.transform.localScale = new Vector3(80f, 1f, 80f);

            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "TwoLaneTestRoad";
            road.transform.position = new Vector3(0f, -0.1f, 0f);
            road.transform.localScale = new Vector3(14f, 0.2f, 600f);
            road.GetComponent<Renderer>().sharedMaterial = roadMat;

            for (int z = -280; z <= 280; z += 18)
            {
                GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                line.name = "CentreLine";
                line.transform.position = new Vector3(0f, 0.012f, z);
                line.transform.localScale = new Vector3(0.16f, 0.025f, 8f);
                Object.DestroyImmediate(line.GetComponent<Collider>());
            }

            GameObject player = CreateTruck("PlayerTruck", new Vector3(1.75f, 1.35f, 0f), Quaternion.identity, bodyMat, true);
            TruckController controller = player.GetComponent<TruckController>();
            TruckSignals signals = player.GetComponent<TruckSignals>();
            TruckTrafficSensors sensors = player.GetComponent<TruckTrafficSensors>();
            TruckSafetyAdvisor advisor = player.GetComponent<TruckSafetyAdvisor>();

            GameObject routeForwardObject = new GameObject("TrafficRouteForward");
            TrafficRoute routeForward = routeForwardObject.AddComponent<TrafficRoute>();
            Transform[] forwardPoints = CreateRoutePoints(routeForwardObject.transform, 1.75f, new[] { -280f, -100f, 100f, 280f });
            SetArrayReferences(routeForward, "waypoints", forwardPoints);
            SetBool(routeForward, "loop", false);

            GameObject routeReverseObject = new GameObject("TrafficRouteOncoming");
            TrafficRoute routeReverse = routeReverseObject.AddComponent<TrafficRoute>();
            Transform[] reversePoints = CreateRoutePoints(routeReverseObject.transform, -1.75f, new[] { 100f, -100f, -280f });
            SetArrayReferences(routeReverse, "waypoints", reversePoints);
            SetBool(routeReverse, "loop", false);

            CreateTruck("TrafficTruckAhead", new Vector3(1.75f, 1.35f, 95f), Quaternion.identity, trafficMat, false, routeForward);
            CreateTruck("TrafficTruckBehind", new Vector3(1.75f, 1.35f, -95f), Quaternion.identity, trafficMat, false, routeForward);
            CreateTruck("OncomingTruck", new Vector3(-1.75f, 1.35f, 190f), Quaternion.Euler(0f, 180f, 0f), trafficMat, false, routeReverse);

            CreateIndicatorLamp(player.transform, new Vector3(-1.05f, 0.35f, 3.08f), lampMat, out Renderer leftLamp);
            CreateIndicatorLamp(player.transform, new Vector3(1.05f, 0.35f, 3.08f), lampMat, out Renderer rightLamp);
            SetObjectReference(signals, "leftIndicator", leftLamp);
            SetObjectReference(signals, "rightIndicator", rightLamp);

            SetObjectReference(advisor, "truck", controller);
            SetObjectReference(advisor, "sensors", sensors);

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            FollowCamera follow = cameraObject.AddComponent<FollowCamera>();
            SetObjectReference(follow, "target", player.transform);
            cameraObject.transform.position = player.transform.TransformPoint(new Vector3(0f, 5f, -10f));
            cameraObject.transform.rotation = Quaternion.Euler(15f, 0f, 0f);
            camera.fieldOfView = 65f;

            GameObject sun = new GameObject("Directional Light");
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientLight = new Color(0.45f, 0.45f, 0.48f);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static GameObject CreateTruck(string name, Vector3 position, Quaternion rotation,
            Material material, bool player, TrafficRoute route = null)
        {
            GameObject root = new GameObject(name);
            root.transform.SetPositionAndRotation(position, rotation);
            BoxCollider bodyCollider = root.AddComponent<BoxCollider>();
            bodyCollider.center = new Vector3(0f, 0.3f, 0f);
            bodyCollider.size = new Vector3(2.5f, 2.2f, 6f);
            Rigidbody rigidbody = root.AddComponent<Rigidbody>();
            rigidbody.mass = player ? 16000f : 12000f;
            rigidbody.drag = 0.02f;
            rigidbody.angularDrag = 0.5f;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            GameObject bodyVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bodyVisual.name = "CabAndChassisVisual";
            bodyVisual.transform.SetParent(root.transform, false);
            bodyVisual.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            bodyVisual.transform.localScale = new Vector3(2.5f, 2.2f, 6f);
            bodyVisual.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(bodyVisual.GetComponent<Collider>());

            WheelCollider[] wheels = new WheelCollider[4];
            Transform[] visuals = new Transform[4];
            Vector3[] positions =
            {
                new Vector3(-1.08f, -0.55f, 2.1f),
                new Vector3(1.08f, -0.55f, 2.1f),
                new Vector3(-1.08f, -0.55f, -2.1f),
                new Vector3(1.08f, -0.55f, -2.1f)
            };
            for (int i = 0; i < 4; i++)
            {
                GameObject wheelObject = new GameObject("WheelCollider_" + i);
                wheelObject.transform.SetParent(root.transform, false);
                wheelObject.transform.localPosition = positions[i];
                WheelCollider wheel = wheelObject.AddComponent<WheelCollider>();
                wheel.radius = 0.55f;
                wheel.suspensionDistance = 0.25f;
                wheel.mass = 45f;
                wheels[i] = wheel;

                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                visual.name = "WheelVisual_" + i;
                visual.transform.SetParent(root.transform, false);
                visual.transform.localPosition = positions[i];
                visual.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                visual.transform.localScale = new Vector3(0.38f, 0.55f, 0.55f);
                Object.DestroyImmediate(visual.GetComponent<Collider>());
                visuals[i] = visual.transform;
            }

            if (player)
            {
                TruckPowertrain powertrain = root.AddComponent<TruckPowertrain>();
                TruckController controller = root.AddComponent<TruckController>();
                SetObjectReference(controller, "frontLeft", wheels[0]);
                SetObjectReference(controller, "frontRight", wheels[1]);
                SetObjectReference(controller, "rearLeft", wheels[2]);
                SetObjectReference(controller, "rearRight", wheels[3]);
                SetObjectReference(controller, "powertrain", powertrain);

                WheelVisualSync sync = root.AddComponent<WheelVisualSync>();
                SetWheelVisuals(sync, wheels, visuals);
                TruckSignals signals = root.AddComponent<TruckSignals>();
                AudioSource horn = root.AddComponent<AudioSource>();
                horn.playOnAwake = false;
                horn.spatialBlend = 1f;
                SetObjectReference(signals, "hornAudio", horn);
                root.AddComponent<TrafficHornBroadcast>();
                root.AddComponent<TruckIndicatorInput>();
                SetObjectReference(root.GetComponent<TruckIndicatorInput>(), "signals", signals);
                TruckTrafficSensors sensors = root.AddComponent<TruckTrafficSensors>();
                TruckSafetyAdvisor advisor = root.AddComponent<TruckSafetyAdvisor>();
                TruckDashboardHUD hud = root.AddComponent<TruckDashboardHUD>();
                SetObjectReference(hud, "truck", controller);
                SetObjectReference(hud, "sensors", sensors);
                SetObjectReference(hud, "safety", advisor);
                SetObjectReference(hud, "signals", signals);
                KeyboardTruckInput keyboard = root.AddComponent<KeyboardTruckInput>();
                SetObjectReference(keyboard, "truck", controller);
                SetObjectReference(keyboard, "signals", signals);
                ConfigureSensors(root, sensors);
            }
            else
            {
                TrafficVehicleAI ai = root.AddComponent<TrafficVehicleAI>();
                SetObjectReference(ai, "route", route);
                SetObjectReference(ai, "frontLeft", wheels[0]);
                SetObjectReference(ai, "frontRight", wheels[1]);
                SetObjectReference(ai, "rearLeft", wheels[2]);
                SetObjectReference(ai, "rearRight", wheels[3]);
                GameObject sensor = new GameObject("FrontSensor");
                sensor.transform.SetParent(root.transform, false);
                sensor.transform.localPosition = new Vector3(0f, 0.8f, 4.5f);
                SetObjectReference(ai, "frontSensorOrigin", sensor.transform);
                SetLayerMask(ai, "vehicleMask", 1 << 8);
                SetLayerRecursively(root, 8);
                root.AddComponent<TrafficHornReceiver>();
            }
            return root;
        }

        private static void ConfigureSensors(GameObject player, TruckTrafficSensors sensors)
        {
            Transform front = CreateSensorOrigin(player.transform, "FrontSensor", new Vector3(0f, 0.8f, 4.2f), 0f);
            Transform rear = CreateSensorOrigin(player.transform, "RearSensor", new Vector3(0f, 0.8f, -4.2f), 0f);
            Transform left = CreateSensorOrigin(player.transform, "LeftBlindSpot", new Vector3(-1.4f, 0.8f, 0f), -90f);
            Transform right = CreateSensorOrigin(player.transform, "RightBlindSpot", new Vector3(1.4f, 0.8f, 0f), 90f);
            Transform leftRear = CreateSensorOrigin(player.transform, "LeftRearApproach", new Vector3(-1.4f, 0.8f, -3f), -135f);
            Transform rightRear = CreateSensorOrigin(player.transform, "RightRearApproach", new Vector3(1.4f, 0.8f, -3f), 135f);
            SetObjectReference(sensors, "forwardOrigin", front);
            SetObjectReference(sensors, "rearOrigin", rear);
            SetObjectReference(sensors, "leftBlindSpotOrigin", left);
            SetObjectReference(sensors, "rightBlindSpotOrigin", right);
            SetObjectReference(sensors, "leftRearApproachOrigin", leftRear);
            SetObjectReference(sensors, "rightRearApproachOrigin", rightRear);
            SetLayerMask(sensors, "trafficMask", 1 << 8);
        }

        private static Transform CreateSensorOrigin(Transform parent, string name, Vector3 position, float yaw)
        {
            GameObject origin = new GameObject(name);
            origin.transform.SetParent(parent, false);
            origin.transform.localPosition = position;
            origin.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            return origin.transform;
        }

        private static Transform[] CreateRoutePoints(Transform parent, float x, float[] zValues)
        {
            Transform[] points = new Transform[zValues.Length];
            for (int i = 0; i < zValues.Length; i++)
            {
                GameObject point = new GameObject("LanePoint_" + i);
                point.transform.SetParent(parent, false);
                point.transform.position = new Vector3(x, 0f, zValues[i]);
                points[i] = point.transform;
            }
            return points;
        }

        private static void CreateIndicatorLamp(Transform parent, Vector3 localPosition, Material material, out Renderer lamp)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = "IndicatorLamp";
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPosition;
            obj.transform.localScale = new Vector3(0.22f, 0.16f, 0.08f);
            obj.GetComponent<Renderer>().sharedMaterial = material;
            lamp = obj.GetComponent<Renderer>();
            Object.DestroyImmediate(obj.GetComponent<Collider>());
        }

        private static void SetWheelVisuals(WheelVisualSync sync, WheelCollider[] wheels, Transform[] visuals)
        {
            SerializedObject so = new SerializedObject(sync);
            SerializedProperty array = so.FindProperty("wheels");
            array.arraySize = wheels.Length;
            for (int i = 0; i < wheels.Length; i++)
            {
                array.GetArrayElementAtIndex(i).FindPropertyRelative("collider").objectReferenceValue = wheels[i];
                array.GetArrayElementAtIndex(i).FindPropertyRelative("visual").objectReferenceValue = visuals[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectReference(Object target, string propertyName, Object value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(propertyName);
            if (property != null) { property.objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
        }

        private static void SetBool(Object target, string propertyName, bool value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(propertyName);
            if (property != null) { property.boolValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
        }

        private static void SetArrayReferences(Object target, string propertyName, Object[] values)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(propertyName);
            if (property == null) return;
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetLayerMask(Object target, string propertyName, int value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(propertyName);
            if (property != null) { property.intValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            root.layer = layer;
            foreach (Transform child in root.transform) SetLayerRecursively(child.gameObject, layer);
        }

        private static Material CreateMaterial(string name, Color color)
        {
            string folder = "Assets/Materials";
            Directory.CreateDirectory(folder);
            string path = folder + "/" + name + ".mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (name == "IndicatorAmber")
            {
                material.EnableKeyword("_EMISSION");
                if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", color * 2f);
            }
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void ConfigureBuildScenes()
        {
            string guid = AssetDatabase.AssetPathToGUID(ScenePath);
            if (string.IsNullOrEmpty(guid)) return;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }
    }
}