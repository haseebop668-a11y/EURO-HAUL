using UnityEngine;

namespace EuroHaul.CameraSystem
{
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 localOffset = new Vector3(0f, 4.5f, -9f);
        [SerializeField, Min(0.1f)] private float positionSharpness = 5f;
        [SerializeField, Min(0.1f)] private float rotationSharpness = 5f;
        [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 1.5f, 0f);

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.TransformPoint(localOffset);
            float positionBlend = 1f - Mathf.Exp(-positionSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, positionBlend);

            Vector3 lookDirection = target.position + lookAtOffset - transform.position;
            if (lookDirection.sqrMagnitude < 0.001f) return;

            Quaternion desiredRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            float rotationBlend = 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationBlend);
        }

        public void SetTarget(Transform newTarget) => target = newTarget;
    }
}