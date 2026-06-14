using UnityEngine;

public static class HUDFeedbackBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureHUDFeedback()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogWarning("HUDFeedbackBootstrap: No Canvas found in scene. HUD feedback was not created.");
            return;
        }

        if (canvas.GetComponentInChildren<HUDFeedbackUI>(true) != null)
            return;

        canvas.gameObject.AddComponent<HUDFeedbackUI>();
    }
}
