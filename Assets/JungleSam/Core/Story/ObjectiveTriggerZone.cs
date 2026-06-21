using UnityEngine;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class ObjectiveTriggerZone : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private string triggerId = "Trigger_Objective";
    [SerializeField] private string requiredPlayerTag = "Player";
    [SerializeField] private bool activateOnce = true;

    [Header("Objective")]
    [SerializeField] private string objectiveText = "Przeszukaj teren kościoła";
    [SerializeField] private string secondaryObjectiveText = "Znajdź ślady oddziału Grom Division";
    [SerializeField] private bool showNotification = true;
    [SerializeField] private string notificationText = "CEL ZAKTUALIZOWANY";

    private bool _activated;
    private GameplayHUDController _hud;

    private void Reset()
    {
        Collider triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }

    private void Awake()
    {
        Collider triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null && !triggerCollider.isTrigger)
            triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activateOnce && _activated)
            return;

        if (!string.IsNullOrWhiteSpace(requiredPlayerTag) && !other.CompareTag(requiredPlayerTag))
            return;

        TriggerObjective();
    }

    [ContextMenu("Debug Trigger Objective")]
    public void TriggerObjective()
    {
        if (activateOnce && _activated)
            return;

        _activated = true;
        ResolveHUD();

        if (_hud == null)
        {
            Debug.Log($"[{name}] Objective trigger '{triggerId}': {objectiveText} / {secondaryObjectiveText}");
            return;
        }

        _hud.SetObjective(objectiveText, secondaryObjectiveText);

        if (showNotification)
            _hud.ShowNotification(notificationText);
    }

    private void ResolveHUD()
    {
        if (_hud == null)
            _hud = GameplayHUDController.Instance;

        if (_hud == null)
            _hud = FindFirstObjectByType<GameplayHUDController>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _activated ? Color.green : Color.cyan;

        Collider zoneCollider = GetComponent<Collider>();

        if (zoneCollider is BoxCollider boxCollider)
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
            Gizmos.matrix = oldMatrix;
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, 1.5f);
        }
    }
}
