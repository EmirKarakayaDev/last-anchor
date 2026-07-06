using System.Collections.Generic;
using LastAnchor.Character;
using LastAnchor.Input;
using UnityEngine;

namespace LastAnchor.Holds
{
    // Finds the nearest reachable HoldPoints, ranks them by alignment with the camera's
    // aim direction, highlights the top few, and snaps the hand LimbTarget to the best one on Grab.
    public class HoldTargetingController : MonoBehaviour
    {
        [SerializeField] private PlayerInputRouter input;
        [SerializeField] private LimbTarget hand;
        [SerializeField] private Transform reachOrigin;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private float reachDistance = 2f;
        [SerializeField] private float minAlignmentDot = 0.5f;
        [SerializeField] private int maxCandidates = 3;
        [SerializeField] private HoldHighlight highlightPrefab;

        private readonly List<HoldPoint> candidates = new();
        private readonly List<(HoldPoint hold, float dot)> scored = new();
        private readonly List<HoldHighlight> highlightPool = new();

        public HoldPoint BestCandidate { get; private set; }

        private void Awake()
        {
            if (reachOrigin == null)
            {
                reachOrigin = transform;
            }

            if (aimCamera == null)
            {
                aimCamera = Camera.main;
            }
        }

        private void Update()
        {
            FindCandidates();
            UpdateHighlights();

            if (input != null && input.GrabPressed)
            {
                TryGrab();
            }
        }

        private void FindCandidates()
        {
            candidates.Clear();
            scored.Clear();
            BestCandidate = null;

            if (aimCamera == null)
            {
                return;
            }

            IReadOnlyList<HoldPoint> allHolds = HoldPoint.Active;
            for (int i = 0; i < allHolds.Count; i++)
            {
                HoldPoint hold = allHolds[i];
                if (Vector3.Distance(hold.Position, reachOrigin.position) > reachDistance)
                {
                    continue;
                }

                Vector3 dir = (hold.Position - aimCamera.transform.position).normalized;
                float dot = Vector3.Dot(dir, aimCamera.transform.forward);
                if (dot < minAlignmentDot)
                {
                    continue;
                }

                scored.Add((hold, dot));
            }

            scored.Sort((a, b) => b.dot.CompareTo(a.dot));

            for (int i = 0; i < scored.Count && i < maxCandidates; i++)
            {
                candidates.Add(scored[i].hold);
            }

            if (candidates.Count > 0)
            {
                BestCandidate = candidates[0];
            }
        }

        private void UpdateHighlights()
        {
            if (highlightPrefab == null)
            {
                return;
            }

            while (highlightPool.Count < candidates.Count)
            {
                highlightPool.Add(Instantiate(highlightPrefab, transform));
            }

            for (int i = 0; i < highlightPool.Count; i++)
            {
                if (i < candidates.Count)
                {
                    highlightPool[i].SetPosition(candidates[i].Position);
                    highlightPool[i].SetColor(candidates[i] == BestCandidate ? Color.green : Color.white);
                    highlightPool[i].SetActive(true);
                }
                else
                {
                    highlightPool[i].SetActive(false);
                }
            }
        }

        private void TryGrab()
        {
            if (BestCandidate == null || hand == null)
            {
                return;
            }

            hand.ReachTo(BestCandidate.Position);
        }
    }
}
