using LastAnchor.Input;
using UnityEngine;

namespace LastAnchor.Character
{
    // M1: sticks the character to whatever surface is under its "up" side and lets the
    // player free-move across that surface's tangent plane. No holds/anchors/grip yet.
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputRouter))]
    public class ClimbCharacterController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSlerpSpeed = 8f;

        [Header("Wall Sticking")]
        [SerializeField] private float wallProbeDistance = 0.6f;
        [SerializeField] private float stickForce = 2f;
        [SerializeField] private LayerMask wallMask = ~0;
        [SerializeField, Tooltip("Consecutive missed probes tolerated before declaring Falling. Absorbs single-frame raycast noise (e.g. right after a reset/regrab) so State doesn't flicker.")]
        private int missedProbeTolerance = 5;

        private CharacterController controller;
        private PlayerInputRouter input;
        private int missedProbeCount;

        public ClimbState State { get; private set; } = ClimbState.OnWall;
        public int MissedProbeCount => missedProbeCount;

        // M4: GripMeter sets this while the player is resting so the character holds still
        // (but still presses into the wall) instead of continuing to climb.
        public bool MovementLocked { get; set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            input = GetComponent<PlayerInputRouter>();
        }

        private void FixedUpdate()
        {
            AlignToSurface();
            ApplyMovement();
        }

        // Lets external callers (e.g. FallAndSwingController after a reset/regrab) force an
        // immediate re-check instead of waiting for the next FixedUpdate, where State would
        // otherwise stay stale for a frame and can re-trigger a fall the instant this re-enables.
        public void RecomputeAlignment()
        {
            missedProbeCount = 0;
            AlignToSurface();
        }

        // Hands control to an external mover (e.g. FallAndSwingController). Disables the raw
        // CharacterController component too, not just this script: CharacterController caches its
        // own internal position and does not reliably notice direct transform.position writes made
        // while it stays enabled, which was snapping the climber back after every reset/regrab.
        public void BeginExternalControl()
        {
            enabled = false;
            controller.enabled = false;
        }

        // Takes control back: applies the final position/rotation while the CharacterController is
        // still disabled (so it re-syncs cleanly), then re-enables both and forces a fresh alignment check.
        public void EndExternalControl(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            controller.enabled = true;
            enabled = true;
            missedProbeCount = 0;
            AlignToSurface();
        }

        private void AlignToSurface()
        {
            Vector3 probeOrigin = transform.position + transform.up * wallProbeDistance;
            if (Physics.Raycast(probeOrigin, -transform.up, out RaycastHit hit, wallProbeDistance * 2f, wallMask))
            {
                missedProbeCount = 0;
                Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSlerpSpeed * Time.fixedDeltaTime);
                State = ClimbState.OnWall;
            }
            else
            {
                // Don't drop to Falling on a single missed probe (skin-width/collision-resolution
                // jitter, e.g. right after a reset, can cause one-frame false misses). Only commit
                // to Falling once several consecutive probes miss.
                missedProbeCount++;
                if (missedProbeCount >= missedProbeTolerance)
                {
                    State = ClimbState.Falling;
                }
            }
        }

        private void ApplyMovement()
        {
            Vector2 moveInput = MovementLocked ? Vector2.zero : input.Move;
            Vector3 tangentMove = (transform.right * moveInput.x + transform.forward * moveInput.y) * moveSpeed;
            Vector3 intoWall = -transform.up * stickForce;
            controller.Move((tangentMove + intoWall) * Time.fixedDeltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 probeOrigin = transform.position + transform.up * wallProbeDistance;
            Gizmos.color = State == ClimbState.OnWall ? Color.green : Color.red;
            Gizmos.DrawLine(probeOrigin, probeOrigin - transform.up * (wallProbeDistance * 2f));
        }
    }
}
