using UnityEngine;
using UnityEngine.Events;

namespace EuroHaul.Traffic
{
    /// <summary>Optional receiver for nearby player horn warnings.</summary>
    public sealed class TrafficHornReceiver : MonoBehaviour
    {
        [SerializeField] private AudioSource responseHorn;
        [SerializeField] private UnityEvent onHornHeard;
        [SerializeField, Min(1f)] private float responseCooldown = 3f;
        private float nextResponseTime;

        public void ReceiveHorn(Transform source)
        {
            if (source == null || Time.time < nextResponseTime) return;
            if (Vector3.Distance(transform.position, source.position) > 40f) return;
            nextResponseTime = Time.time + responseCooldown;
            if (responseHorn != null && !responseHorn.isPlaying) responseHorn.Play();
            onHornHeard?.Invoke();
        }
    }
}