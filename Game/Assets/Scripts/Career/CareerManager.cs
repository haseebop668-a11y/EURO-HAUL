using System;
using System.IO;
using UnityEngine;

namespace EuroHaul.Career
{
    [Serializable]
    public sealed class FreightContract
    {
        public string id;
        public string origin;
        public string destination;
        public string cargo;
        public float requiredDistanceMeters;
        public float totalDistanceMeters;
        public int rewardCash;
        public int rewardXp;
        public bool accepted;
    }

    [Serializable]
    public sealed class CareerSaveData
    {
        public int schemaVersion = 1;
        public string driverName = "New Driver";
        public int driverLevel = 1;
        public int experience;
        public long gameCash = 25000;
        public int completedDeliveries;
        public float totalDistanceMeters;
        public FreightContract offeredContract;
        public FreightContract activeContract;
        public string lastSavedUtc;
    }

    public static class CareerProgression
    {
        public static int ExperienceRequiredForNextLevel(int level)
        {
            return Mathf.Max(100, 100 + (Mathf.Max(1, level) - 1) * 75);
        }

        public static int ApplyExperience(CareerSaveData data, int amount)
        {
            if (data == null || amount <= 0) return 0;
            data.experience += amount;
            int levelsGained = 0;
            while (data.experience >= ExperienceRequiredForNextLevel(data.driverLevel))
            {
                data.experience -= ExperienceRequiredForNextLevel(data.driverLevel);
                data.driverLevel++;
                levelsGained++;
            }
            return levelsGained;
        }

        public static long ApplyDeliveryReward(CareerSaveData data, int cash, int xp)
        {
            if (data == null) return 0;
            long safeCash = Math.Max(0, cash);
            data.gameCash = Math.Max(0L, data.gameCash + safeCash);
            ApplyExperience(data, xp);
            data.completedDeliveries++;
            return safeCash;
        }
    }

    /// <summary>Local, offline-first career save. Wallet is fictional Game Cash only.</summary>
    public sealed class CareerManager : MonoBehaviour
    {
        private const int CurrentSchemaVersion = 1;
        private const string SaveFileName = "career-save.json";
        private const string BackupFileName = "career-save.backup.json";

        [SerializeField] private Transform trackedTruck;
        [SerializeField, Min(0.5f)] private float completionRadiusMeters = 2f;

        public static CareerManager Instance { get; private set; }
        public CareerSaveData Data { get; private set; }
        public float ActiveContractProgress01 { get; private set; }
        public string LastNotice { get; private set; } = "Welcome to EURO HAUL";
        public event Action OnCareerChanged;

        private Vector3 lastTruckPosition;
        private bool hasLastTruckPosition;
        private bool deliveryCompletedThisFrame;

        private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);
        private string BackupPath => Path.Combine(Application.persistentDataPath, BackupFileName);

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
            if (trackedTruck != null) SetTrackedTruck(trackedTruck);
        }

        private void Update()
        {
            if (trackedTruck == null || Data == null) return;
            Vector3 current = trackedTruck.position;
            if (!hasLastTruckPosition)
            {
                lastTruckPosition = current;
                hasLastTruckPosition = true;
                return;
            }

            float delta = Vector3.Distance(current, lastTruckPosition);
            lastTruckPosition = current;
            if (delta <= 0f || delta > 20f || Time.timeScale <= 0f) return;

            Data.totalDistanceMeters += delta;
            if (Data.activeContract != null && Data.activeContract.accepted)
            {
                Data.activeContract.requiredDistanceMeters =
                    Mathf.Max(1f, Data.activeContract.requiredDistanceMeters - delta);
                ActiveContractProgress01 = Mathf.Clamp01(1f -
                    Data.activeContract.requiredDistanceMeters / Mathf.Max(1f, initialContractDistance));
                if (Data.activeContract.requiredDistanceMeters <= completionRadiusMeters)
                    CompleteActiveContract();
            }
        }

        private float initialContractDistance = 1f;

        public void SetTrackedTruck(Transform truck)
        {
            trackedTruck = truck;
            lastTruckPosition = truck != null ? truck.position : Vector3.zero;
            hasLastTruckPosition = truck != null;
        }

        public void StartNewCareer(string driverName)
        {
            Data = new CareerSaveData();
            Data.driverName = string.IsNullOrWhiteSpace(driverName) ? "New Driver" : driverName.Trim();
            Data.offeredContract = GenerateContract();
            ActiveContractProgress01 = 0f;
            LastNotice = "Career started. Choose your first freight contract.";
            Save();
            NotifyChanged();
        }

        public bool AcceptOfferedContract()
        {
            if (Data == null || Data.activeContract != null || Data.offeredContract == null) return false;
            Data.activeContract = Data.offeredContract;
            Data.activeContract.accepted = true;
            initialContractDistance = Mathf.Max(1f, Data.activeContract.totalDistanceMeters > 0f
                        ? Data.activeContract.totalDistanceMeters : Data.activeContract.requiredDistanceMeters);
            ActiveContractProgress01 = 0f;
            LastNotice = "Contract accepted: " + Data.activeContract.destination;
            Save();
            NotifyChanged();
            return true;
        }

        public bool AbandonActiveContract()
        {
            if (Data == null || Data.activeContract == null) return false;
            Data.activeContract = null;
            ActiveContractProgress01 = 0f;
            Data.offeredContract = GenerateContract();
            LastNotice = "Contract abandoned. A new offer is available.";
            Save();
            NotifyChanged();
            return true;
        }

        private void CompleteActiveContract()
        {
            if (deliveryCompletedThisFrame || Data == null || Data.activeContract == null) return;
            deliveryCompletedThisFrame = true;
            FreightContract completed = Data.activeContract;
            CareerProgression.ApplyDeliveryReward(Data, completed.rewardCash, completed.rewardXp);
            Data.activeContract = null;
            Data.offeredContract = GenerateContract();
            ActiveContractProgress01 = 0f;
            LastNotice = "Delivery complete! +" + completed.rewardCash + " Game Cash";
            Save();
            NotifyChanged();
            deliveryCompletedThisFrame = false;
        }

        private FreightContract GenerateContract()
        {
            string[] cities = { "Berlin", "Hamburg", "Munich", "Prague", "Vienna", "Amsterdam", "Brussels" };
            string[] cargoes = { "Packaged Food", "Building Materials", "Auto Parts", "Medical Supplies", "Furniture" };
            int seed = UnityEngine.Random.Range(100000, 999999);
            int origin = UnityEngine.Random.Range(0, cities.Length);
            int destination = (origin + UnityEngine.Random.Range(1, cities.Length)) % cities.Length;
            int distance = UnityEngine.Random.Range(420, 561);
            return new FreightContract
            {
                id = "EH-" + seed,
                origin = cities[origin],
                destination = cities[destination],
                cargo = cargoes[UnityEngine.Random.Range(0, cargoes.Length)],
                requiredDistanceMeters = distance,
                totalDistanceMeters = distance,
                rewardCash = Mathf.RoundToInt(distance * UnityEngine.Random.Range(1.2f, 1.8f)),
                rewardXp = Mathf.RoundToInt(distance * 0.12f),
                accepted = false
            };
        }

        private void Load()
        {
            Data = ReadSave(SavePath);
            if (Data == null) Data = ReadSave(BackupPath);
            if (Data == null || Data.schemaVersion > CurrentSchemaVersion)
            {
                Data = new CareerSaveData();
                Data.offeredContract = GenerateContract();
            }
            else
            {
                Data.schemaVersion = CurrentSchemaVersion;
                if (Data.offeredContract == null && Data.activeContract == null)
                    Data.offeredContract = GenerateContract();
                if (Data.activeContract != null)
                {
                    Data.activeContract.accepted = true;
                    initialContractDistance = Mathf.Max(1f, Data.activeContract.requiredDistanceMeters);
                }
            }
            NotifyChanged();
        }

        private CareerSaveData ReadSave(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                CareerSaveData loaded = JsonUtility.FromJson<CareerSaveData>(File.ReadAllText(path));
                return loaded != null && loaded.schemaVersion > 0 ? loaded : null;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("EURO HAUL save read failed: " + exception.Message);
                return null;
            }
        }

        public void Save()
        {
            if (Data == null) return;
            try
            {
                Data.schemaVersion = CurrentSchemaVersion;
                Data.lastSavedUtc = DateTime.UtcNow.ToString("o");
                string directory = Application.persistentDataPath;
                Directory.CreateDirectory(directory);
                string tempPath = SavePath + ".tmp";
                File.WriteAllText(tempPath, JsonUtility.ToJson(Data, true));
                if (File.Exists(SavePath))
                {
                    File.Copy(SavePath, BackupPath, true);
                    File.Delete(SavePath);
                }
                File.Move(tempPath, SavePath);
            }
            catch (Exception exception)
            {
                Debug.LogError("EURO HAUL save failed: " + exception.Message);
                LastNotice = "Save failed. Check device storage.";
            }
        }

        private void OnApplicationPause(bool paused) { if (paused) Save(); }
        private void OnApplicationQuit() => Save();

        private void NotifyChanged() => OnCareerChanged?.Invoke();

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}