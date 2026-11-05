using UnityEngine;

namespace EuroHaul.Traffic
{
    /// <summary>Ordered lane-centre waypoints for one traffic lane.</summary>
    public sealed class TrafficRoute : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private bool loop = true;

        public int Count => waypoints == null ? 0 : waypoints.Length;
        public bool Loop => loop;
        public Transform GetWaypoint(int index)
        {
            if (waypoints == null || index < 0 || index >= waypoints.Length) return null;
            return waypoints[index];
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Length < 2) return;
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.8f);
            for (int i = 0; i < waypoints.Length - 1; i++)
                if (waypoints[i] != null && waypoints[i + 1] != null)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            if (loop && waypoints[0] != null && waypoints[waypoints.Length - 1] != null)
                Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
        }
#endif
    }
}