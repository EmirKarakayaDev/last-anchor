using LastAnchor.Input;
using LastAnchor.Rope;
using UnityEngine;

namespace LastAnchor.Character
{
    // Drains while the climber is on the wall and not resting; regenerates during an explicit
    // Rest hold, or passively while dangling on the rope (arcing/swinging) since no grip strength
    // is being spent there. Hitting 0 forces a release via FallAndSwingController.
    public class GripMeter : MonoBehaviour
    {
        [SerializeField] private ClimbCharacterController climber;
        [SerializeField] private PlayerInputRouter input;
        [SerializeField] private FallAndSwingController fallAndSwing;

        [SerializeField] private float drainPerSecond = 0.1f;
        [SerializeField] private float regenPerSecond = 0.25f;
        [SerializeField, Tooltip("Passive regen rate while arcing/swinging on the rope (not gripping anything).")]
        private float swingRegenPerSecond = 0.2f;
        [SerializeField, Tooltip("Minimum grip granted the instant the climber gets back on the wall (regrab/reset), so a 0-grip landing gives a moment to react instead of insta-falling again next frame.")]
        private float minGripOnRegrab = 0.15f;

        public float Grip01 { get; private set; } = 1f;
        public bool IsResting { get; private set; }

        private bool wasOnWall = true;

        private void Update()
        {
            if (fallAndSwing != null && fallAndSwing.IsActive)
            {
                IsResting = false;
                wasOnWall = false;
                Grip01 = Mathf.Min(1f, Grip01 + swingRegenPerSecond * Time.deltaTime);
                return;
            }

            if (climber.State != ClimbState.OnWall)
            {
                IsResting = false;
                wasOnWall = false;
                return;
            }

            if (!wasOnWall)
            {
                Grip01 = Mathf.Max(Grip01, minGripOnRegrab);
            }
            wasOnWall = true;

            bool restHeld = input != null && input.RestHeld;
            IsResting = restHeld;
            climber.MovementLocked = restHeld;

            if (restHeld)
            {
                Grip01 = Mathf.Min(1f, Grip01 + regenPerSecond * Time.deltaTime);
            }
            else
            {
                Grip01 = Mathf.Max(0f, Grip01 - drainPerSecond * Time.deltaTime);
                if (Grip01 <= 0f && fallAndSwing != null)
                {
                    fallAndSwing.ForceFall();
                }
            }
        }
    }
}
