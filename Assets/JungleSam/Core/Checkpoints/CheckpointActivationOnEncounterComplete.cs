using UnityEngine;

[DisallowMultipleComponent]
public class CheckpointActivationOnEncounterComplete : MonoBehaviour
{
    [SerializeField] private CheckpointVolume checkpoint;
    [SerializeField] private bool showNotification = true;
    [SerializeField] private string notificationText = "CHECKPOINT AKTYWNY";
    [SerializeField] private GameplayHUDController hud;

    [ContextMenu("Debug Activate Checkpoint")]
    public void ActivateCheckpoint()
    {
        if (checkpoint == null)
            checkpoint = GetComponentInChildren<CheckpointVolume>();

        if (checkpoint != null)
            checkpoint.ActivateCheckpoint();
        else
            Debug.LogWarning($"[{name}] Cannot activate checkpoint: CheckpointVolume is not assigned.");

        if (showNotification)
        {
            ResolveHUD();

            if (hud != null)
                hud.ShowNotification(notificationText);
            else
                Debug.Log($"HUD notification: {notificationText}");
        }
    }

    private void ResolveHUD()
    {
        if (hud == null)
            hud = GameplayHUDController.Instance;

        if (hud == null)
            hud = FindFirstObjectByType<GameplayHUDController>();
    }
}
