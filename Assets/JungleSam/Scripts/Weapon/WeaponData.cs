using UnityEngine;

public enum AmmoCategory
{
    PistolSmg,
    Rifle
}

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "ProjectJungle/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Informacje ogólne")]
    public string weaponName;
    public GameObject weaponPrefab; // Model 3D broni

    [Header("Typ amunicji")]
    public AmmoCategory ammoCategory = AmmoCategory.PistolSmg;

    [Header("Statystyki")]
    public float damage = 10f;
    public float fireRate = 0.5f; // Czas przerwy między strzałami
    public float range = 50f;
    public int maxAmmo = 30;
    public int maxReserveAmmo = 90; // Amunicja w schowku
    public float reloadTime = 1.5f; // Czas trwania animacji przeładowania
    public float shootAnimationDuration = 0.25f;
    public bool isAutomatic = false;

    [Header("Efekty")]
    public GameObject muzzleFlashPrefab;
    public Vector2 muzzleFlashSize = Vector2.zero;
    public bool scaleMuzzleFlashByCameraDistance = false;
    public float muzzleFlashReferenceDistance = 1f;
    public bool overrideMuzzleFlashTint = false;
    public Color muzzleFlashTint = new Color(1f, 0.78f, 0.45f, 0.85f);
    public bool overrideMuzzleFlashLight = false;
    public bool muzzleFlashLightEnabled = true;
    public float muzzleFlashLightLifeTime = 0.015f;
    public float muzzleFlashLightRange = 0.35f;
    public float muzzleFlashLightIntensity = 0.75f;
    public AudioClip shootSound;
    public AudioClip[] shootSounds;
    public Vector2 shootPitchRange = new Vector2(0.96f, 1.04f);
    public Vector2 shootVolumeRange = new Vector2(0.85f, 1f);
    public float shotFeedbackDelay = 0f;
    public float shotSoundDelay = 0f;
    public bool interruptPreviousShotSound = false;
}
