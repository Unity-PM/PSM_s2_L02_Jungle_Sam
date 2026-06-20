using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI ammoText;

    

    private PlayerController _playerController;

    void Start()
    {
        // Znalezienie PlayerController
        _playerController = Object.FindAnyObjectByType<PlayerController>();
        if (_playerController == null)
        {
            Debug.LogError("AmmoUI: PlayerController not found!");
            return;
        }

        /* Ustawienie pozycji (lewy dolny róg + offset)
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0, 0); // Lewy dolny róg
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.anchoredPosition = new Vector2(offsetX, offsetY);
            rectTransform.pivot = new Vector2(0, 0);
        }
        */

        UpdateAmmoDisplay();
    }

    void Update()
    {
        UpdateAmmoDisplay();
    }

    private void UpdateAmmoDisplay()
    {
        if (_playerController == null || _playerController.currentWeapon == null || ammoText == null)
            return;

        int currentAmmo = _playerController.currentWeapon.GetCurrentAmmo();
        int maxAmmo = _playerController.currentWeapon.GetMaxAmmo();
        int reserveAmmo = _playerController.currentWeapon.GetReserveAmmo();

        // Format:  11 w magazynku, 30 w schowku
        ammoText.text = $"{currentAmmo} / {reserveAmmo}";
    }
}
