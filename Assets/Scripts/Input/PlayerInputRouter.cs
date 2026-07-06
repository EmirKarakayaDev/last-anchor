using UnityEngine;
using UnityEngine.InputSystem;

namespace LastAnchor.Input
{
    // Wraps the raw InputActionAsset (no generated wrapper class is configured on
    // InputSystem_Actions.inputactions) and exposes clean read-only properties to gameplay scripts.
    public class PlayerInputRouter : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;

        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction grabAction;
        private InputAction placeAnchorAction;
        private InputAction debugBelayAction;
        private InputAction debugForceFallAction;
        private InputAction restAction;

        public Vector2 Move => moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 Look => lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;

        // M2 stopgap: reuses the template's "Attack" action (mouse left button / gamepad West)
        // as the single-hand Grab trigger. Revisit once left/right hand actions are needed.
        public bool GrabPressed => grabAction != null && grabAction.WasPressedThisFrame();

        // M3 stopgap: reuses template actions instead of adding a new Climbing action map.
        public bool PlaceAnchorPressed => placeAnchorAction != null && placeAnchorAction.WasPressedThisFrame();
        public bool DebugBelayHeld => debugBelayAction != null && debugBelayAction.IsPressed();
        public bool DebugForceFallPressed => debugForceFallAction != null && debugForceFallAction.WasPressedThisFrame();

        // M4: reuses the template's "Interact" action (E key, Hold interaction) as Rest.
        public bool RestHeld => restAction != null && restAction.IsPressed();

        private void OnEnable()
        {
            if (inputActions == null)
            {
                Debug.LogError($"{nameof(PlayerInputRouter)} on {name} has no InputActionAsset assigned.", this);
                return;
            }

            var playerMap = inputActions.FindActionMap("Player", throwIfNotFound: true);
            moveAction = playerMap.FindAction("Move", throwIfNotFound: true);
            lookAction = playerMap.FindAction("Look", throwIfNotFound: true);
            grabAction = playerMap.FindAction("Attack", throwIfNotFound: true);
            placeAnchorAction = playerMap.FindAction("Jump", throwIfNotFound: true);
            debugBelayAction = playerMap.FindAction("Crouch", throwIfNotFound: true);
            debugForceFallAction = playerMap.FindAction("Next", throwIfNotFound: true);
            restAction = playerMap.FindAction("Interact", throwIfNotFound: true);
            playerMap.Enable();
        }

        private void OnDisable()
        {
            inputActions?.FindActionMap("Player")?.Disable();
        }
    }
}
