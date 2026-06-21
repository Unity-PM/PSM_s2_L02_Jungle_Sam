using UnityEngine;

[DisallowMultipleComponent]
public class CheckpointActivationOnEncounterComplete : MonoBehaviour
{
    [SerializeField] private CheckpointVolume checkpoint;
    [SerializeField] private bool showNotification = true;
    [SerializeField] private string notificationText = "CHECKPOINT AKTYWNY";

    private GameplayHUDController _hud;

    [ContextMenu("Debug Activate Checkpoint")]
    public void ActivateCheckpoint()
    {
        if (checkpoint == null)
            checkpoint = GetComponentInChildren<CheckpointVolume>();

        if (checkpoint != null)
            checkpoint.ActivateCheckpoint();
        else
            Debug.LogWarning($"[{name}] Cannot activate checkpoint: CheckpointVolume is not assigned.", this);

        if (!showNotification)
            return;

        ResolveHUD();

        if (_hud != null)
            _hud.ShowNotification(notificationText);
        else
            Debug.Log($"HUD notification: {notificationText}");
    }

    private void ResolveHUD()
    {
        if (_hud == null)
            _hud = GameplayHUDController.Instance;

        if (_hud == null)
            _hud = FindFirstObjectByType<GameplayHUDController>();
    }
}
