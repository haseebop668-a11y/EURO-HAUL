using UnityEngine;

namespace EuroHaul.Driving
{
    /// <summary>Estimates upcoming road curvature from three authored centre-line markers.</summary>
    public sealed class RoadCurveSpeedAdvisor : MonoBehaviour
    {
        [SerializeField] private TruckSafetyAdvisor safetyAdvisor;
        [SerializeField] private Transform nearMarker;
        [SerializeField] private Transform apexMarker;
        [SerializeField] private Transform exitMarker;
        [SerializeField, Range(0.5f, 4f)] private float lateralAccelerationLimit = 1.35f;

        private void Awake()
        {
            if (safetyAdvisor == null) safetyAdvisor = GetComponentInParent<TruckSafetyAdvisor>();
        }

        private void Update()
        {
            if (safetyAdvisor == null || nearMarker == null || apexMarker == null || exitMarker == null) return;
            Vector3 a = (apexMarker.position - nearMarker.position).normalized;
            Vector3 b = (exitMarker.position - apexMarker.position).normalized;
            float angleRadians = Vector3.Angle(a, b) * Mathf.Deg2Rad;
            float chord = Vector3.Distance(nearMarker.position, exitMarker.position);
            float radius = angleRadians < 0.02f ? 10000f : chord / (2f * Mathf.Sin(angleRadians * 0.5f));
            safetyAdvisor.SetCurveRadius(radius, lateralAccelerationLimit);
        }

        private void OnDisable()
        {
            if (safetyAdvisor != null) safetyAdvisor.SetCurveRadius(0f);
        }
    }
}