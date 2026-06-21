using UnityEngine;

[DisallowMultipleComponent]
public class ObjectiveOnEncounterComplete : MonoBehaviour
{
    [Header("Objective")]
    [SerializeField] private string objectiveText = "Sprawdź zabudowania wskazane w raporcie";
    [SerializeField] private string secondaryObjectiveText = "Przejdź do domu obok kościoła";
    [SerializeField] private bool showNotification = true;
    [SerializeField] private string notificationText = "CEL ZAKTUALIZOWANY";

    private GameplayHUDController _hud;

    [ContextMenu("Debug Apply Objective")]
    public void ApplyObjective()
    {
        ResolveHUD();

        if (_hud == null)
        {
            Debug.Log($"[{name}] Encounter complete objective: {objectiveText} / {secondaryObjectiveText}");
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
}
