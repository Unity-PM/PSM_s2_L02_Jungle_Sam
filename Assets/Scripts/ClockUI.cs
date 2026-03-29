using UnityEngine;
using TMPro;
using System;

public class ClockUI : MonoBehaviour
{
    private TextMeshProUGUI _textMesh;

    void Start() => _textMesh = GetComponent<TextMeshProUGUI>();

    void Update()
    {
        // Pobieranie czasu w Polsce (CET/CEST)
        var polandZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        DateTime polandTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, polandZone);
        _textMesh.text = polandTime.ToString("HH:mm");
    }
}