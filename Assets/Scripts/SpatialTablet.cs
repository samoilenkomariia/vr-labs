using UnityEngine;
using UnityEngine.InputSystem;

public class SpatialTablet : MonoBehaviour
{
    [SerializeField] private Transform head;
    [SerializeField] private GameObject tablet;

    [SerializeField] private float distance = 0.8f;
    [SerializeField] private float verticalOffset = -0.15f;

    private InputAction toggleAction;

    private void Awake()
    {
        if (head == null || tablet == null)
        {
            Debug.LogError("[SpatialTablet] Assign Head and Tablet in the Inspector.", this);
            enabled = false;
            return;
        }

        toggleAction = new InputAction(
            "ToggleTablet",
            InputActionType.Button);

        toggleAction.AddBinding("<Keyboard>/f2");

        toggleAction.AddBinding(
            "<XRController>{LeftHand}/menuButton");

        tablet.SetActive(false);
    }

    private void OnEnable()
    {
        if (toggleAction == null)
            return;

        toggleAction.performed += OnToggle;
        toggleAction.Enable();
        Debug.Log("[SpatialTablet] Ready. Press F2 with Unity Game view focused, or use Open Tablet (Play Mode) in this component's context menu.", this);
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

    private void OnToggle(InputAction.CallbackContext context)
    {
        Debug.Log($"[SpatialTablet] Input received: {context.control.path}", this);
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
        Debug.Log($"[SpatialTablet] Opened at {tablet.transform.position}. Active in hierarchy: {tablet.activeInHierarchy}", this);
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
