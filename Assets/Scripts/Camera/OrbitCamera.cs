using LastAnchor.Input;
using UnityEngine;

namespace LastAnchor.CameraControl
{
    // M1 debug camera: world-space yaw/pitch orbit around the climber, driven by the
    // existing Player/Look action. Deliberately does not re-orient with the wall normal
    // so the player keeps a stable frame of reference while climbing.
    public class OrbitCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private PlayerInputRouter input;
        [SerializeField] private float distance = 5f;
        [SerializeField] private float heightOffset = 1.2f;
        [SerializeField] private float sensitivity = 0.25f;
        [SerializeField] private float minPitch = -40f;
        [SerializeField] private float maxPitch = 75f;

        private float yaw;
        private float pitch = 15f;

        private void LateUpdate()
        {
            if (target == null || input == null)
            {
                return;
            }

            Vector2 look = input.Look;
            yaw += look.x * sensitivity;
            pitch = Mathf.Clamp(pitch - look.y * sensitivity, minPitch, maxPitch);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focusPoint = target.position + Vector3.up * heightOffset;
            transform.position = focusPoint - rotation * Vector3.forward * distance;
            transform.rotation = rotation;
        }
    }
}
