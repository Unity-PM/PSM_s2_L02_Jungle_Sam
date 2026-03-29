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
    public bool isAutomatic = false;

    [Header("Efekty")]
    public GameObject muzzleFlashPrefab;
    public AudioClip shootSound;
}