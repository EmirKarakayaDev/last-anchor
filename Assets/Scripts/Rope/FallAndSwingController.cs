using LastAnchor.Character;
using LastAnchor.Input;
using UnityEngine;

namespace LastAnchor.Rope
{
    // Takes over from ClimbCharacterController whenever the climber falls: kinematically
    // arcs to a hang point below the last anchor, then runs a damped 2D pendulum swing.
    // No anchor placed -> instant reset to a respawn point (fall-to-death stub).
    public class FallAndSwingController : MonoBehaviour
    {
        public enum Phase
        {
            Idle,
            ArcingToHang,
            Swinging
        }

        [Header("Refs")]
        [SerializeField] private ClimbCharacterController climbController;
        [SerializeField] private AnchorController anchors;
        [SerializeField] private PlayerInputRouter input;
        [SerializeField] private Transform respawnPoint;

        [Header("Rope")]
        [SerializeField] private float ropeLength = 3f;
        [SerializeField] private float minRopeLength = 0.75f;

        [Header("Arc-to-hang")]
        [SerializeField] private float arcDuration = 0.4f;
        [SerializeField] private AnimationCurve arcCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Swing")]
        [SerializeField] private float gravity = 9.81f;
        [SerializeField] private float swingDamping = 0.6f;
        [SerializeField] private float swingInputForce = 3f;

        [Header("Debug Belay (solo-testing stub, not real co-op logic)")]
        [SerializeField] private float belayPullSpeed = 1.5f;

        [Header("Safety")]
        [SerializeField, Tooltip("Seconds after any reset/regrab during which a new fall cannot re-trigger. Prevents a reset-fall-reset-fall spin if the climber is put back down still touching/near the edge.")]
        private float postResetGrace = 0.3f;

        [Header("Regrab")]
        [SerializeField, Tooltip("Same layer as ClimbCharacterController's Wall Mask.")]
        private LayerMask wallMask = ~0;
        [SerializeField, Tooltip("How far to search for a wall when the player presses Grab while swinging. Deliberately more generous than the climber's own on-wall probe distance, so regrab isn't a pixel-perfect timing check.")]
        private float regrabProbeDistance = 2f;
        [SerializeField, Tooltip("Distance to sit off the wall surface once regrabbed, matching the climber's capsule radius.")]
        private float onWallDistance = 0.5f;

        public Phase CurrentPhase { get; private set; } = Phase.Idle;
        public bool IsActive => CurrentPhase != Phase.Idle;

        // M4: lets GripMeter force a release the instant grip hits 0, without waiting for
        // ClimbCharacterController's own wall-probe to (possibly never) report Falling.
        public void ForceFall()
        {
            if (CurrentPhase == Phase.Idle)
            {
                StartFall();
            }
        }

        private Vector3 arcStart;
        private Vector3 arcTarget;
        private float arcT;

        private Anchor swingAnchor;
        private float currentRopeLength;
        private float theta;
        private float omega;
        private float graceUntil;

        private void Update()
        {
            bool inGrace = Time.time < graceUntil;

            // A real edge-fall (ClimbCharacterController lost the wall) or a debug-forced fall
            // both funnel through StartFall() so both test paths behave identically.
            if (!inGrace && CurrentPhase == Phase.Idle && climbController.enabled && climbController.State == ClimbState.Falling)
            {
                StartFall();
            }

            if (!inGrace && input != null && input.DebugForceFallPressed && CurrentPhase == Phase.Idle)
            {
                StartFall();
            }

            if (CurrentPhase == Phase.Swinging && input != null && input.GrabPressed)
            {
                TryRegrab();
            }
        }

        private void FixedUpdate()
        {
            switch (CurrentPhase)
            {
                case Phase.ArcingToHang:
                    TickArc();
                    break;
                case Phase.Swinging:
                    TickSwing();
                    break;
            }
        }

        private void StartFall()
        {
            // Hands off to us: disables both the script and the raw CharacterController so nothing
            // else can fight over transform.position while we're driving the arc/swing.
            climbController.BeginExternalControl();

            Anchor last = anchors != null ? anchors.LastAnchor : null;
            if (last == null)
            {
                ResetToRespawn();
                return;
            }

            swingAnchor = last;
            currentRopeLength = ropeLength;

            arcStart = transform.position;
            Vector3 offset = arcStart - swingAnchor.Position;
            Vector3 horizontalDir = new Vector3(offset.x, 0f, offset.z);
            if (horizontalDir.sqrMagnitude < 0.0001f)
            {
                horizontalDir = Vector3.forward;
            }
            horizontalDir.Normalize();

            arcTarget = swingAnchor.Position + horizontalDir * (currentRopeLength * 0.3f) + Vector3.down * (currentRopeLength * 0.95f);
            arcT = 0f;
            CurrentPhase = Phase.ArcingToHang;
        }

        private void TickArc()
        {
            arcT = Mathf.Min(1f, arcT + Time.fixedDeltaTime / arcDuration);
            float eased = arcCurve.Evaluate(arcT);
            transform.position = Vector3.Lerp(arcStart, arcTarget, eased);

            if (arcT >= 1f)
            {
                BeginSwing();
            }
        }

        private void BeginSwing()
        {
            Vector3 offset = transform.position - swingAnchor.Position;
            theta = Mathf.Atan2(offset.x, -offset.y);
            omega = 0f;
            CurrentPhase = Phase.Swinging;
        }

        private void TickSwing()
        {
            float dt = Time.fixedDeltaTime;

            if (input != null && input.DebugBelayHeld)
            {
                currentRopeLength = Mathf.Max(minRopeLength, currentRopeLength - belayPullSpeed * dt);
            }

            float angularAccel = -(gravity / currentRopeLength) * Mathf.Sin(theta);
            omega += angularAccel * dt;

            if (input != null)
            {
                omega += input.Move.x * swingInputForce * dt;
            }

            omega *= Mathf.Clamp01(1f - swingDamping * dt);
            theta += omega * dt;

            Vector3 swingOffset = new Vector3(Mathf.Sin(theta), -Mathf.Cos(theta), 0f) * currentRopeLength;
            transform.position = swingAnchor.Position + swingOffset;
        }

        private void TryRegrab()
        {
            // -transform.up is the established "toward the wall" direction (matches
            // ClimbCharacterController's own probe convention). Searching both directions was the
            // bug: with a wide enough probe distance the ray can punch through the (thin) wall from
            // either side, and the wrong-side hit has a backwards normal, producing a broken snap.
            if (TryFindWall(-transform.up, out RaycastHit hit))
            {
                Vector3 snappedPosition = hit.point + hit.normal * onWallDistance;
                Quaternion snappedRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
                CurrentPhase = Phase.Idle;
                climbController.EndExternalControl(snappedPosition, snappedRotation);
                graceUntil = Time.time + postResetGrace; 
            }
            // else: not close enough to the wall yet, stay swinging and let the player try again.
        }

        private bool TryFindWall(Vector3 towardWallDirection, out RaycastHit hit)
        {
            Vector3 probeOrigin = transform.position - towardWallDirection * regrabProbeDistance;
            return Physics.Raycast(probeOrigin, towardWallDirection, out hit, regrabProbeDistance * 2f, wallMask);
        }

        private void ResetToRespawn()
        {
            CurrentPhase = Phase.Idle;
            Vector3 targetPos = respawnPoint != null ? respawnPoint.position : transform.position;
            Quaternion targetRot = respawnPoint != null ? respawnPoint.rotation : transform.rotation;
            climbController.EndExternalControl(targetPos, targetRot);
            graceUntil = Time.time + postResetGrace;
        }
    }
}
