using UnityEngine;
using UnityEngine.Events;
using EuroHaul.Audio;

namespace EuroHaul.Traffic
{
    /// <summary>Nearby traffic hears a player horn and can acknowledge it with a short response.</summary>
    public sealed class TrafficHornReceiver : MonoBehaviour
    {
        [SerializeField] private AudioSource responseHorn;
        [SerializeField] private UnityEvent onHornHeard;
        [SerializeField, Min(1f)] private float responseCooldown = 3f;
        private float nextResponseTime;

        private void Awake()
        {
            if (responseHorn == null) responseHorn = GetComponent<AudioSource>();
            if (responseHorn == null) responseHorn = gameObject.AddComponent<AudioSource>();
            if (responseHorn.clip == null) responseHorn.clip = PrototypeHornAudio.CreateClip("Traffic Horn Acknowledgement");
            responseHorn.spatialBlend = 1f;
        }

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