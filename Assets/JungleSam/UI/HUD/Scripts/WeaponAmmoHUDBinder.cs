using UnityEngine;

[DisallowMultipleComponent]
public class WeaponAmmoHUDBinder : MonoBehaviour
{
    [SerializeField] private GameplayHUDController hud;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private WeaponBase weapon;
    [SerializeField] private string fallbackWeaponName = "AK-47";
    [SerializeField] private string fallbackAmmoType = "7.62x39mm";
    [SerializeField] private bool showFallbackWhenNoWeapon = false;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Update()
    {
        RefreshFromWeapon();
    }

    [ContextMenu("Refresh From Weapon")]
    public void RefreshFromWeapon()
    {
        ResolveReferences();

        if (hud == null)
            return;

        WeaponBase currentWeapon = weapon;

        if (playerController != null && playerController.currentWeapon != null)
            currentWeapon = playerController.currentWeapon;

        if (currentWeapon == null)
        {
            if (showFallbackWhenNoWeapon)
                hud.SetAmmo(fallbackWeaponName, 30, 120, fallbackAmmoType);

            return;
        }

        string weaponName = fallbackWeaponName;
        string ammoType = fallbackAmmoType;

        if (currentWeapon.weaponData != null)
        {
            weaponName = string.IsNullOrWhiteSpace(currentWeapon.weaponData.weaponName)
                ? fallbackWeaponName
                : currentWeapon.weaponData.weaponName;

            ammoType = GetAmmoTypeLabel(weaponName, currentWeapon.weaponData.ammoCategory);
        }

        hud.SetAmmo(
            weaponName,
            currentWeapon.GetCurrentAmmo(),
            currentWeapon.GetReserveAmmo(),
            ammoType
        );
    }

    private void ResolveReferences()
    {
        if (hud == null)
            hud = FindFirstObjectByType<GameplayHUDController>();

        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        if (weapon == null && playerController != null)
            weapon = playerController.currentWeapon;
    }

    private static string GetAmmoTypeLabel(string weaponName, AmmoCategory ammoCategory)
    {
        if (IsAkPatternWeapon(weaponName))
            return "7.62x39mm";

        return ammoCategory switch
        {
            AmmoCategory.Rifle => "5.56 NATO",
            AmmoCategory.PistolSmg => "9x19mm",
            _ => "AMMO"
        };
    }

    private static bool IsAkPatternWeapon(string weaponName)
    {
        if (string.IsNullOrWhiteSpace(weaponName))
            return false;

        string normalized = weaponName.Trim().ToUpperInvariant();
        return normalized == "AK"
            || normalized.StartsWith("AK-")
            || normalized.Contains("AK37")
            || normalized.Contains("AK-37")
            || normalized.Contains("AK47")
            || normalized.Contains("AK-47");
    }
}
