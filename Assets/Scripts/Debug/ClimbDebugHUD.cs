using LastAnchor.Character;
using LastAnchor.Rope;
using UnityEngine;

namespace LastAnchor.DebugTools
{
    // Turns invisible climb state into a readable on-screen readout during Play Mode testing.
    // Fields are added milestone by milestone (M1: state, M2: hand hold status, M3: fall/swing/anchors).
    public class ClimbDebugHUD : MonoBehaviour
    {
        [SerializeField] private ClimbCharacterController climber;
        [SerializeField] private LimbTarget hand;
        [SerializeField] private FallAndSwingController fallAndSwing;
        [SerializeField] private AnchorController anchors;
        [SerializeField] private GripMeter grip;

        private void OnGUI()
        {
            if (climber == null)
            {
                return;
            }

            string stateText = fallAndSwing != null && fallAndSwing.IsActive
                ? fallAndSwing.CurrentPhase.ToString()
                : climber.State.ToString();
            GUI.Label(new Rect(10, 10, 400, 24), $"State: {stateText} (climbCtrl.State={climber.State}, missed={climber.MissedProbeCount})");
            GUI.Label(new Rect(10, 34, 400, 24), $"ClimbCtrl enabled: {climber.enabled} | Phase: {(fallAndSwing != null ? fallAndSwing.CurrentPhase.ToString() : "n/a")}");
            GUI.Label(new Rect(10, 58, 400, 24), $"Position: {climber.transform.position}");

            if (hand != null)
            {
                string handStatus = hand.HasHold ? $"Hand: holding {hand.GrabbedPosition}" : "Hand: free";
                GUI.Label(new Rect(10, 82, 400, 24), handStatus);
            }

            if (anchors != null)
            {
                GUI.Label(new Rect(10, 106, 300, 24), $"Anchors remaining: {anchors.AnchorsRemaining}");
            }

            if (grip != null)
            {
                string restingText = grip.IsResting ? " (resting)" : "";
                GUI.Label(new Rect(10, 130, 300, 24), $"Grip: {grip.Grip01 * 100f:F0}%{restingText}");
            }
        }
    }
}
