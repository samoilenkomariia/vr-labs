using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Lab4AssemblyFeedback : MonoBehaviour
{
    [SerializeField] private XRSocketInteractor socket;
    [SerializeField] private XRGrabInteractable module;

    [SerializeField] private GameObject emptyIndicator;
    [SerializeField] private GameObject installedIndicator;
    [SerializeField] private TMP_Text statusText;

    private bool wasInstalled;

    private bool IsInstalled =>
        socket != null &&
        module != null &&
        socket.hasSelection &&
        socket.firstInteractableSelected == module;

    private void Start()
    {
        wasInstalled = IsInstalled;
        Refresh(wasInstalled);
    }

    private void Update()
    {
        bool installed = IsInstalled;

        if (installed == wasInstalled)
            return;

        wasInstalled = installed;
        Refresh(installed);
    }

    private void Refresh(bool installed)
    {
        if (emptyIndicator != null)
            emptyIndicator.SetActive(!installed);

        if (installedIndicator != null)
            installedIndicator.SetActive(installed);

        if (statusText != null)
        {
            statusText.text = installed
                ? "Module installed"
                : "Insert the server module";
        }
    }

    public void CheckAssembly()
    {
        if (statusText == null)
            return;

        statusText.text = IsInstalled
            ? "Check passed: module is installed"
            : "Check failed: module is missing";
    }
}