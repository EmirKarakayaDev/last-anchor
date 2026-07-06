using System.Collections.Generic;
using LastAnchor.Input;
using UnityEngine;

namespace LastAnchor.Rope
{
    // Places limited-count anchors at the climber's current position and tracks the stack.
    public class AnchorController : MonoBehaviour
    {
        [SerializeField] private PlayerInputRouter input;
        [SerializeField] private Transform anchorOrigin;
        [SerializeField] private Anchor anchorPrefab;
        [SerializeField] private int maxAnchors = 5;

        private readonly List<Anchor> placed = new();

        public int AnchorsRemaining => maxAnchors - placed.Count;
        public Anchor LastAnchor => placed.Count > 0 ? placed[^1] : null;

        private void Awake()
        {
            if (anchorOrigin == null)
            {
                anchorOrigin = transform;
            }
        }

        private void Update()
        {
            if (input != null && input.PlaceAnchorPressed)
            {
                TryPlaceAnchor();
            }
        }

        private void TryPlaceAnchor()
        {
            if (AnchorsRemaining <= 0 || anchorPrefab == null)
            {
                return;
            }

            Anchor anchor = Instantiate(anchorPrefab, anchorOrigin.position, Quaternion.identity);
            placed.Add(anchor);
        }
    }
}
