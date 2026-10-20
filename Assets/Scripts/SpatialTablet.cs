using UnityEngine;
using UnityEngine.InputSystem;

public class SpatialTablet : MonoBehaviour
{
    [SerializeField] private Transform head;
    [SerializeField] private GameObject tablet;

    [SerializeField] private float distance = 0.8f;
    [SerializeField] private float verticalOffset = -0.15f;

    private InputAction toggleAction;
    private bool controllerButtonWasPressed;

    private void Awake()
    {
        if (head == null || tablet == null)
        {
            enabled = false;
            return;
        }

        toggleAction = new InputAction(
            "ToggleTablet",
            InputActionType.Button);

        toggleAction.AddBinding("<Keyboard>/f2");

        tablet.SetActive(false);
    }

    private void OnEnable()
    {
        controllerButtonWasPressed = false;
        if (toggleAction == null)
            return;

        toggleAction.performed += OnToggle;
        toggleAction.Enable();
    }

    private void OnDisable()
    {
        if (toggleAction == null)
            return;

        toggleAction.performed -= OnToggle;
        toggleAction.Disable();
    }

    private void OnDestroy()
    {
        toggleAction?.Dispose();
    }

    private void Update()
    {
        var right = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(
            UnityEngine.XR.XRNode.RightHand);
        var left = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(
            UnityEngine.XR.XRNode.LeftHand);

        bool rightPressed = right.TryGetFeatureValue(
            UnityEngine.XR.CommonUsages.secondaryButton, out bool secondary) && secondary;
        bool leftPressed = left.TryGetFeatureValue(
            UnityEngine.XR.CommonUsages.menuButton, out bool menu) && menu;
        bool pressed = rightPressed || leftPressed;

        if (pressed && !controllerButtonWasPressed)
            Toggle();

        controllerButtonWasPressed = pressed;
    }

    private void OnToggle(InputAction.CallbackContext context)
    {
        Toggle();
    }

    public void Toggle()
    {
        if (!Application.isPlaying || head == null || tablet == null)
            return;

        if (tablet.activeSelf)
        {
            Close();
            return;
        }

        Open();
    }

    [ContextMenu("Open Tablet (Play Mode)")]
    public void Open()
    {
        if (!Application.isPlaying || head == null || tablet == null)
            return;

        Vector3 forward =
            Vector3.ProjectOnPlane(head.forward, Vector3.up);

        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.ProjectOnPlane(
                head.up, Vector3.up);
        }

        forward.Normalize();

        tablet.transform.position =
            head.position +
            forward * distance +
            Vector3.up * verticalOffset;

        tablet.transform.rotation =
            Quaternion.LookRotation(forward, Vector3.up);

        tablet.SetActive(true);
    }

    [ContextMenu("Close Tablet (Play Mode)")]
    public void Close()
    {
        if (!Application.isPlaying || tablet == null)
            return;

        tablet.SetActive(false);
        Debug.Log("[SpatialTablet] Closed.", this);
    }
}
