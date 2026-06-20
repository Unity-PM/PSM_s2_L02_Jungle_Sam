using UnityEngine;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class CheckpointVolume : MonoBehaviour
{
    [Header("Checkpoint")]
    [SerializeField] private string checkpointId = "Checkpoint";
    [SerializeField] private RespawnPoint respawnPoint;
    [SerializeField] private bool activateOnce = true;

    [Header("Player Filter")]
    [SerializeField] private bool requirePlayerTag = true;
    [SerializeField] private string playerTag = "Player";

    private bool _activated;

    public string CheckpointId => checkpointId;
    public RespawnPoint RespawnPoint => respawnPoint;
    public bool IsActivated => _activated;

    private void Reset()
    {
        Collider triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;

        if (respawnPoint == null)
            respawnPoint = GetComponentInChildren<RespawnPoint>();
    }

    private void Awake()
    {
        Collider triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null && !triggerCollider.isTrigger)
            triggerCollider.isTrigger = true;

        if (respawnPoint == null)
            respawnPoint = GetComponentInChildren<RespawnPoint>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activateOnce && _activated)
            return;

        if (requirePlayerTag && !other.CompareTag(playerTag))
            return;

        ActivateCheckpoint();
    }

    public void ActivateCheckpoint()
    {
        if (activateOnce && _activated)
            return;

        CheckpointManager manager = CheckpointManager.Instance != null
            ? CheckpointManager.Instance
            : FindFirstObjectByType<CheckpointManager>();

        if (manager == null)
        {
            Debug.LogWarning($"[{name}] CheckpointManager not found for checkpoint '{checkpointId}'.");
            return;
        }

        manager.RegisterCheckpoint(this);
        _activated = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _activated ? Color.green : Color.yellow;

        Collider checkpointCollider = GetComponent<Collider>();

        if (checkpointCollider is BoxCollider boxCollider)
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
            Gizmos.matrix = oldMatrix;
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, 1f);
        }

        if (respawnPoint != null)
            Gizmos.DrawLine(transform.position, respawnPoint.transform.position);
    }
}
