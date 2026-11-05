using UnityEngine;

namespace EuroHaul.Driving
{
    /// <summary>Temporary PC keyboard adapter. Requires Active Input Handling = Both or Input Manager (Old).</summary>
    public sealed class KeyboardTruckInput : MonoBehaviour
    {
        [SerializeField] private TruckController truck;

        private void Update()
        {
            if (truck == null) return;
            float steering = Input.GetAxisRaw("Horizontal");
            float throttle = Input.GetAxisRaw("Vertical");
            float brake = Input.GetKey(KeyCode.Space) ? 1f : 0f;
            truck.SetInput(steering, throttle, brake);
        }

        private void OnDisable()
        {
            if (truck != null) truck.SetInput(0f, 0f, 1f);
        }
    }
}