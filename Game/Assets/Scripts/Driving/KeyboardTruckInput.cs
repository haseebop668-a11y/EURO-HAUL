using UnityEngine;

namespace EuroHaul.Driving
{
    /// <summary>PC keyboard adapter. Requires Active Input Handling = Both or Input Manager (Old).</summary>
    public sealed class KeyboardTruckInput : MonoBehaviour
    {
        [SerializeField] private TruckController truck;
        [SerializeField] private TruckSignals signals;

        private void Update()
        {
            if (truck == null) return;
            float steering = Input.GetAxisRaw("Horizontal");
            float throttle = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : 0f;
            float brake = Input.GetKey(KeyCode.Space) ? 1f : 0f;
            truck.SetInput(steering, throttle, brake);
            truck.SetClutchPedal(Input.GetKey(KeyCode.C) ? 1f : 0f);
            if (Input.GetKeyDown(KeyCode.E)) truck.ShiftGearUp();
            if (Input.GetKeyDown(KeyCode.Q)) truck.ShiftGearDown();
            if (Input.GetKeyDown(KeyCode.N)) truck.SelectNeutral();
            if (Input.GetKeyDown(KeyCode.R)) truck.SelectReverse();

            if (signals != null)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow)) signals.ToggleLeft();
                if (Input.GetKeyDown(KeyCode.RightArrow)) signals.ToggleRight();
                if (Input.GetKeyDown(KeyCode.H)) signals.ToggleHazard();
                if (Input.GetKeyDown(KeyCode.B)) signals.HornDown();
                if (Input.GetKeyUp(KeyCode.B)) signals.HornUp();
            }
        }

        private void OnDisable()
        {
            if (truck != null)
            {
                truck.SetInput(0f, 0f, 1f);
                truck.SetClutchPedal(1f);
            }
            if (signals != null) signals.HornUp();
        }
    }
}