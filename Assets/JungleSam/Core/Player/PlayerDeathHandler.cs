using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 1.5f;
    [SerializeField] private bool waitForRespawnButton = false;
    [SerializeField] private GameObject playerRoot;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private CheckpointManager checkpointManager;
    [SerializeField] private DeathUIController deathUIController;
    [SerializeField] private PlayerControlLock playerControlLock;
    [SerializeField] private EncounterResetService encounterResetService;

    private Coroutine _deathRoutine;
    private bool _respawnRequested;

    private void Reset()
    {
        ResolveReferences();
    }

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnDied -= HandlePlayerDied;
    }

    private void HandlePlayerDied()
    {
        if (_deathRoutine != null)
            return;

        _deathRoutine = StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        _respawnRequested = false;

        if (playerControlLock != null)
            playerControlLock.SetLocked(true);

        if (deathUIController != null)
        {
            deathUIController.SetRespawnCallback(RequestRespawn);
            deathUIController.Show();
        }

        if (encounterResetService != null)
            encounterResetService.ResetActiveEncounter();

        if (waitForRespawnButton)
        {
            while (!_respawnRequested)
                yield return null;
        }
        else if (respawnDelay > 0f)
        {
            float elapsed = 0f;

            while (elapsed < respawnDelay && !_respawnRequested)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        if (checkpointManager != null && playerRoot != null)
            checkpointManager.RespawnPlayer(playerRoot);
        else if (playerHealth != null)
            playerHealth.RestoreFullHealth();

        // TODO: Hook minimum ammo refill here after the ammo safety floor system exists.

        if (deathUIController != null)
            deathUIController.Hide();

        if (playerControlLock != null)
            playerControlLock.SetLocked(false);

        _deathRoutine = null;
    }

    private void RequestRespawn()
    {
        _respawnRequested = true;
    }

    private void ResolveReferences()
    {
        if (playerRoot == null)
            playerRoot = gameObject;

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        if (playerControlLock == null)
            playerControlLock = GetComponent<PlayerControlLock>();

        if (checkpointManager == null)
            checkpointManager = FindFirstObjectByType<CheckpointManager>();

        if (deathUIController == null)
            deathUIController = FindFirstObjectByType<DeathUIController>();

        if (encounterResetService == null)
            encounterResetService = FindFirstObjectByType<EncounterResetService>();
    }
}
