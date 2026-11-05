using UnityEngine;

namespace EuroHaul.Driving
{
    /// <summary>UI-friendly mobile control adapter. Connect press/release events from a touch UI.</summary>
    public sealed class TouchTruckInput : MonoBehaviour
    {
        [SerializeField] private TruckController truck;
        private float steering;
        private bool accelerate;
        private bool reverse;
        private bool brake;
        private bool clutch;

        public void SetSteering(float value) => steering = Mathf.Clamp(value, -1f, 1f);
        public void ClearSteering() => steering = 0f;
        public void AccelerateDown() { accelerate = true; reverse = false; }
        public void AccelerateUp() => accelerate = false;
        public void ReverseDown()
        {
            accelerate = false;
            reverse = truck != null && truck.SelectReverse();
        }
        public void ReverseUp() => reverse = false;
        public void BrakeDown() => brake = true;
        public void BrakeUp() => brake = false;
        public void ClutchDown() => clutch = true;
        public void ClutchUp() => clutch = false;
        public void GearUp() { if (truck != null) truck.ShiftGearUp(); }
        public void GearDown() { if (truck != null) truck.ShiftGearDown(); }
        public void Neutral() { if (truck != null) truck.SelectNeutral(); }

        private void Update()
        {
            if (truck == null) return;
            float drive = accelerate || reverse ? 1f : 0f;
            truck.SetInput(steering, drive, brake ? 1f : 0f);
            truck.SetClutchPedal(clutch ? 1f : 0f);
        }

        private void OnDisable()
        {
            accelerate = reverse = brake = clutch = false;
            steering = 0f;
            if (truck != null)
            {
                truck.SetInput(0f, 0f, 1f);
                truck.SetClutchPedal(1f);
            }
        }
    }
}