using System.Collections.Generic;
using UnityEngine;

namespace LastAnchor.Holds
{
    // Hand-placed marker for a grabbable point on the facade. transform.forward
    // represents the surface normal the hold sticks out of (used later for anchor validity).
    // Self-registers in a static list so targeting doesn't depend on colliders/physics queries.
    public class HoldPoint : MonoBehaviour
    {
        private static readonly List<HoldPoint> active = new();
        public static IReadOnlyList<HoldPoint> Active => active;

        public Vector3 Position => transform.position;

        private void OnEnable()
        {
            active.Add(this);
        }

        private void OnDisable()
        {
            active.Remove(this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.3f);
        }
    }
}
