using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class EquipmentInfoCard : MonoBehaviour
{
    [SerializeField] private XRBaseInteractable target;
    [SerializeField] private GameObject card;
    [SerializeField] private Transform head;
    [SerializeField] private Transform anchor;

    private void Awake()
    {
        card.SetActive(false);
    }

    private void LateUpdate()
    {
        bool visible = target.isHovered || target.isSelected;

        card.SetActive(visible);

        if (!visible)
            return;

        card.transform.position = anchor.position;

        Vector3 direction =
            card.transform.position - head.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            card.transform.rotation =
                Quaternion.LookRotation(direction, Vector3.up);
        }
    }

    private void OnDisable()
    {
        if (card != null)
            card.SetActive(false);
    }
}