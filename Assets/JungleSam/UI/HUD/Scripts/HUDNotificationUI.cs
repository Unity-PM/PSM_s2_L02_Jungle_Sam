using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class HUDNotificationUI : MonoBehaviour
{
    [SerializeField] private GameObject notificationRoot;
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInDuration = 0.15f;
    [SerializeField] private float fadeOutDuration = 0.35f;

    private Coroutine _hideRoutine;

    private void Awake()
    {
        AutoWire();
        Hide();
    }

    private void Start()
    {
        AutoWire();
        Hide();
    }

    [ContextMenu("Auto Wire")]
    public void AutoWire()
    {
        notificationRoot ??= gameObject;
        notificationText ??= GetComponentInChildren<TMP_Text>(true);
        canvasGroup ??= GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Show(string text, float duration)
    {
        AutoWire();

        if (notificationRoot != null && !notificationRoot.activeSelf)
            notificationRoot.SetActive(true);

        if (notificationText != null)
            notificationText.text = string.IsNullOrWhiteSpace(text) ? "CEL ZAKTUALIZOWANY" : text;

        if (_hideRoutine != null)
            StopCoroutine(_hideRoutine);

        _hideRoutine = StartCoroutine(ShowRoutine(duration));
    }

    public void Hide()
    {
        AutoWire();

        if (_hideRoutine != null)
        {
            StopCoroutine(_hideRoutine);
            _hideRoutine = null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private IEnumerator ShowRoutine(float duration)
    {
        if (notificationRoot != null)
            notificationRoot.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        yield return Fade(0f, 1f, fadeInDuration);
        yield return new WaitForSeconds(Mathf.Max(0f, duration));
        yield return Fade(1f, 0f, fadeOutDuration);

        _hideRoutine = null;
        Hide();
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (canvasGroup == null)
            yield break;

        if (duration <= 0f)
        {
            canvasGroup.alpha = to;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}
