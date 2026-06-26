using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDFeedbackUI : MonoBehaviour
{
    [Header("Hit Marker")]
    [SerializeField] private bool showHitMarker = false;
    [SerializeField] private Graphic hitMarkerGraphic;
    [SerializeField] private Color hitMarkerColor = Color.white;
    [SerializeField] private float hitMarkerDuration = 0.12f;
    [SerializeField] private float hitMarkerScale = 1.25f;

    [Header("Damage Flash")]
    [SerializeField] private Graphic damageFlashGraphic;
    [SerializeField] private Color damageFlashColor = new Color(1f, 0f, 0f, 0.58f);
    [SerializeField] private float damageFlashDuration = 0.42f;

    private RectTransform _hitMarkerRect;
    private Vector3 _hitMarkerBaseScale = Vector3.one;
    private float _hitMarkerTimer;
    private float _damageFlashTimer;

    private void Awake()
    {
        EnsureGeneratedElements();

        if (hitMarkerGraphic != null)
        {
            _hitMarkerRect = hitMarkerGraphic.rectTransform;
            _hitMarkerBaseScale = _hitMarkerRect.localScale;
        }

        SetGraphicAlpha(hitMarkerGraphic, 0f, hitMarkerColor);
        SetGraphicAlpha(damageFlashGraphic, 0f, damageFlashColor);
    }

    private void EnsureGeneratedElements()
    {
        if (damageFlashGraphic == null)
            damageFlashGraphic = CreateDamageFlash();

        if (showHitMarker && hitMarkerGraphic == null)
            hitMarkerGraphic = CreateHitMarker();
    }

    private void OnEnable()
    {
        WeaponBase.EnemyHit += OnEnemyHit;
        PlayerStats.PlayerDamaged += OnPlayerDamaged;
    }

    private void OnDisable()
    {
        WeaponBase.EnemyHit -= OnEnemyHit;
        PlayerStats.PlayerDamaged -= OnPlayerDamaged;
    }

    private void Update()
    {
        UpdateHitMarker();
        UpdateDamageFlash();
    }

    private void OnEnemyHit(WeaponBase weapon, EnemyAI enemy)
    {
        if (!showHitMarker)
            return;

        _hitMarkerTimer = hitMarkerDuration;

        if (_hitMarkerRect != null)
            _hitMarkerRect.localScale = _hitMarkerBaseScale * hitMarkerScale;
    }

    private void OnPlayerDamaged(PlayerStats playerStats, float damageAmount)
    {
        EnsureGeneratedElements();
        _damageFlashTimer = damageFlashDuration;
        SetGraphicAlpha(damageFlashGraphic, damageFlashColor.a, damageFlashColor);
    }

    private void UpdateHitMarker()
    {
        if (hitMarkerGraphic == null)
            return;

        if (_hitMarkerTimer <= 0f)
        {
            SetGraphicAlpha(hitMarkerGraphic, 0f, hitMarkerColor);
            return;
        }

        _hitMarkerTimer -= Time.unscaledDeltaTime;
        float progress = Mathf.Clamp01(_hitMarkerTimer / hitMarkerDuration);
        SetGraphicAlpha(hitMarkerGraphic, progress, hitMarkerColor);

        if (_hitMarkerRect != null)
        {
            float scale = Mathf.Lerp(1f, hitMarkerScale, progress);
            _hitMarkerRect.localScale = _hitMarkerBaseScale * scale;
        }
    }

    private void UpdateDamageFlash()
    {
        if (damageFlashGraphic == null)
            return;

        if (_damageFlashTimer <= 0f)
        {
            SetGraphicAlpha(damageFlashGraphic, 0f, damageFlashColor);
            return;
        }

        _damageFlashTimer -= Time.unscaledDeltaTime;
        float progress = Mathf.Clamp01(_damageFlashTimer / damageFlashDuration);
        SetGraphicAlpha(damageFlashGraphic, progress * damageFlashColor.a, damageFlashColor);
    }

    private static void SetGraphicAlpha(Graphic graphic, float alpha, Color baseColor)
    {
        if (graphic == null)
            return;

        if (alpha > 0f && !graphic.gameObject.activeSelf)
            graphic.gameObject.SetActive(true);

        baseColor.a = alpha;
        graphic.color = baseColor;
    }

    private Graphic CreateHitMarker()
    {
        GameObject markerObject = new GameObject("Generated Hit Marker", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        markerObject.transform.SetParent(transform, false);
        markerObject.transform.SetAsLastSibling();

        RectTransform rectTransform = markerObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(42f, 42f);

        TextMeshProUGUI markerText = markerObject.GetComponent<TextMeshProUGUI>();
        markerText.text = "X";
        markerText.fontSize = 34f;
        markerText.fontStyle = FontStyles.Bold;
        markerText.alignment = TextAlignmentOptions.Center;
        markerText.raycastTarget = false;
        markerText.color = new Color(hitMarkerColor.r, hitMarkerColor.g, hitMarkerColor.b, 0f);

        return markerText;
    }

    private Graphic CreateDamageFlash()
    {
        GameObject flashObject = new GameObject("Generated Damage Flash", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        flashObject.transform.SetParent(transform, false);
        flashObject.transform.SetAsFirstSibling();

        RectTransform rectTransform = flashObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image flashImage = flashObject.GetComponent<Image>();
        flashImage.raycastTarget = false;
        flashImage.color = new Color(damageFlashColor.r, damageFlashColor.g, damageFlashColor.b, 0f);

        return flashImage;
    }
}
