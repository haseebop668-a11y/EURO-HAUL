using UnityEngine;

namespace EuroHaul.Driving
{
    /// <summary>Keeps visible wheel meshes aligned with their WheelColliders.</summary>
    public sealed class WheelVisualSync : MonoBehaviour
    {
        [System.Serializable]
        private struct Wheel
        {
            public WheelCollider collider;
            public Transform visual;
        }

        [SerializeField] private Wheel[] wheels;

        private void LateUpdate()
        {
            if (wheels == null) return;
            foreach (Wheel wheel in wheels)
            {
                if (wheel.collider == null || wheel.visual == null) continue;
                wheel.collider.GetWorldPose(out Vector3 position, out Quaternion rotation);
                wheel.visual.SetPositionAndRotation(position, rotation);
            }
        }
    }
}