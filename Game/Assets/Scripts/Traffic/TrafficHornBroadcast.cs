using UnityEngine;

namespace EuroHaul.Traffic
{
    /// <summary>Finds nearby traffic receivers when the player sounds the horn.</summary>
    public sealed class TrafficHornBroadcast : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float broadcastRadius = 40f;
        [SerializeField] private LayerMask trafficMask = ~0;
        private readonly Collider[] overlapBuffer = new Collider[32];

        public void BroadcastHorn()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, broadcastRadius,
                overlapBuffer, trafficMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                TrafficHornReceiver receiver = overlapBuffer[i].GetComponentInParent<TrafficHornReceiver>();
                if (receiver != null && receiver.transform.root != transform.root)
                    receiver.ReceiveHorn(transform);
            }
        }
    }
}