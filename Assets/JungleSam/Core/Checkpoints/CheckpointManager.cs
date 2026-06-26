using UnityEngine;

[DisallowMultipleComponent]
public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("Fallback")]
    [SerializeField] private RespawnPoint fallbackRespawnPoint;

    [Header("Player Restore")]
    [SerializeField] private bool restoreArmorOnRespawn = true;
    [SerializeField] private float respawnArmor = 100f;

    private CheckpointVolume _currentCheckpoint;

    public CheckpointVolume CurrentCheckpoint => _currentCheckpoint;

    private void Reset()
    {
        if (fallbackRespawnPoint == null)
            fallbackRespawnPoint = GetComponentInChildren<RespawnPoint>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Multiple CheckpointManager instances found. '{name}' will stay scene-local but Instance remains '{Instance.name}'.");
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void RegisterCheckpoint(CheckpointVolume checkpoint)
    {
        if (checkpoint == null)
            return;

        _currentCheckpoint = checkpoint;
        Debug.Log($"Checkpoint activated: {checkpoint.CheckpointId}");
        GameplaySaveSystem.SaveCheckpoint(checkpoint);
    }

    public void RespawnPlayer(GameObject player)
    {
        if (player == null)
            return;

        RespawnPoint respawnPoint = ResolveRespawnPoint();

        if (respawnPoint != null)
            TeleportPlayer(player, respawnPoint.Position, respawnPoint.Rotation);
        else
            Debug.LogWarning($"[{name}] No active checkpoint or fallback RespawnPoint assigned.");

        PlayerHealth playerHealth = player.GetComponentInChildren<PlayerHealth>();

        if (playerHealth != null)
            playerHealth.RestoreFullHealth();

        PlayerStats playerStats = player.GetComponentInChildren<PlayerStats>();

        if (restoreArmorOnRespawn && playerStats != null)
            playerStats.SetArmor(respawnArmor);

        // TODO: Invoke minimum ammo refill hook here when the ammo safety floor is implemented.
    }

    private RespawnPoint ResolveRespawnPoint()
    {
        if (_currentCheckpoint != null && _currentCheckpoint.RespawnPoint != null)
            return _currentCheckpoint.RespawnPoint;

        return fallbackRespawnPoint;
    }

    private static void TeleportPlayer(GameObject player, Vector3 position, Quaternion rotation)
    {
        CharacterController characterController = player.GetComponent<CharacterController>();
        Rigidbody rigidbody = player.GetComponent<Rigidbody>();

        bool characterControllerWasEnabled = characterController != null && characterController.enabled;

        if (characterController != null)
            characterController.enabled = false;

        if (rigidbody != null)
        {
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }

        player.transform.SetPositionAndRotation(position, rotation);

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
            playerController.ResetLook(rotation);

        if (characterController != null)
            characterController.enabled = characterControllerWasEnabled;
    }
}
