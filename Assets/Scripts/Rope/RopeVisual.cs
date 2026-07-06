using UnityEngine;

namespace LastAnchor.Rope
{
    // Drives a 2-point LineRenderer between the climber and the last placed anchor.
    // Purely visual; the fall/swing simulation is independent of this.
    [RequireComponent(typeof(LineRenderer))]
    public class RopeVisual : MonoBehaviour
    {
        [SerializeField] private AnchorController anchors;
        [SerializeField] private Transform ropeStart;

        private LineRenderer line;

        private void Awake()
        {
            line = GetComponent<LineRenderer>();
            line.positionCount = 2;

            if (ropeStart == null)
            {
                ropeStart = transform;
            }
        }

        private void LateUpdate()
        {
            Anchor anchor = anchors != null ? anchors.LastAnchor : null;
            bool show = anchor != null;
            line.enabled = show;

            if (show)
            {
                line.SetPosition(0, ropeStart.position);
                line.SetPosition(1, anchor.Position);
            }
        }
    }
}
