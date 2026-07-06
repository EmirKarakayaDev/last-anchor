using UnityEngine;

namespace LastAnchor.Character
{
    // Fakes hand-IK: eases toward a grabbed world position over a short duration,
    // then stays pinned there (in world space) until the next grab.
    public class LimbTarget : MonoBehaviour
    {
        [SerializeField] private float reachDuration = 0.2f;
        [SerializeField] private AnimationCurve reachCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private Vector3 reachStart;
        private Vector3 reachTarget;
        private float reachT = 1f;

        public bool HasHold { get; private set; }
        public Vector3 GrabbedPosition { get; private set; }

        private void Awake()
        {
            reachTarget = transform.position;
        }

        public void ReachTo(Vector3 worldPosition)
        {
            reachStart = transform.position;
            reachTarget = worldPosition;
            reachT = 0f;
            HasHold = true;
            GrabbedPosition = worldPosition;
        }

        public void Release()
        {
            HasHold = false;
        }

        private void Update()
        {
            if (reachT >= 1f)
            {
                return;
            }

            reachT = Mathf.Min(1f, reachT + Time.deltaTime / reachDuration);
            float eased = reachCurve.Evaluate(reachT);
            transform.position = Vector3.Lerp(reachStart, reachTarget, eased);
        }
    }
}
