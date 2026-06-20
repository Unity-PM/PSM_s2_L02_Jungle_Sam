using UnityEngine;

[DisallowMultipleComponent]
public class ObjectiveOnEncounterComplete : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private GameplayHUDController hud;
    [SerializeField] private string objectiveText = "Sprawdź zabudowania wskazane w raporcie";
    [SerializeField] private string secondaryObjectiveText = "Przejdź do domu obok kościoła";
    [SerializeField] private bool showNotification = true;
    [SerializeField] private string notificationText = "CEL ZAKTUALIZOWANY";

    [ContextMenu("Debug Apply Objective")]
    public void ApplyObjective()
    {
        ResolveHUD();

        if (hud != null)
        {
            hud.SetObjective(objectiveText, secondaryObjectiveText);

            if (showNotification)
                hud.ShowNotification(notificationText);
        }
        else
        {
            Debug.Log($"[{name}] Encounter complete objective: {objectiveText} / {secondaryObjectiveText}");
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
