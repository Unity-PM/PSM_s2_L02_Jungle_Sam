using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "ProjectJungle/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Informacje ogólne")]
    public string weaponName;
    public GameObject weaponPrefab; // Model 3D broni

    [Header("Statystyki")]
    public float damage = 10f;
    public float fireRate = 0.5f; // Czas przerwy między strzałami
    public float range = 50f;
    public int maxAmmo = 30;
    public float reloadTime = 1.5f; // Czas trwania animacji przeładowania
    public float shootAnimationDuration = 0.3f; // Czas trwania animacji strzału
    public bool isAutomatic = false;

    [Header("Efekty")]
    public GameObject muzzleFlashPrefab;
    public AudioClip shootSound;
}