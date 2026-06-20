using UnityEngine;
using TMPro;
using System;

public class ClockUI : MonoBehaviour
{
    private static readonly string[] PolandTimeZoneIds =
    {
        "Central European Standard Time",
        "Europe/Warsaw"
    };

    private TextMeshProUGUI _textMesh;
    private TimeZoneInfo _polandZone;

    void Start()
    {
        _textMesh = GetComponent<TextMeshProUGUI>();
        _polandZone = GetPolandTimeZone();
    }

    void Update()
    {
        if (_textMesh == null)
            return;

        DateTime polandTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _polandZone);
        _textMesh.text = polandTime.ToString("HH:mm");
    }

    private static TimeZoneInfo GetPolandTimeZone()
    {
        foreach (string timeZoneId in PolandTimeZoneIds)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        Debug.LogWarning("ClockUI: Poland time zone not found. Falling back to local system time.");
        return TimeZoneInfo.Local;
    }
}
