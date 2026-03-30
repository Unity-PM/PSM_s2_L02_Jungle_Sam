using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponBase : MonoBehaviour
{
    public WeaponData weaponData; // Przypisz dane broni w Inspektorze

    protected int _currentAmmo;
    protected float _nextFireTime = 0f;
    

    // Referencja do kamery (potrzebna do Raycastu - strzelania na środek ekranu)
    protected Camera _mainCam;

    void Start()
    {
        _mainCam = Camera.main;
        _currentAmmo = weaponData.maxAmmo;
    }

    void Update()
    {
        // Sprawdzamy, czy gracz naciska strzał (LPM)
        if (Mouse.current.leftButton.isPressed)
        {
            TryShoot();
        }
    }

    public void TryShoot()
    {
        if (Time.time >= _nextFireTime && _currentAmmo > 0)
        {
            Shoot();
            _nextFireTime = Time.time + weaponData.fireRate;
        }
    }

    protected virtual void Shoot()
    {
        _currentAmmo--;
        Debug.Log("Strzał z: " + weaponData.weaponName + " | Amunicja: " + _currentAmmo);

        // Prosty Raycast (strzał Hitscan - jak w pistolecie/karabinie)
        Ray ray = _mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // Wewnątrz metody Shoot() w WeaponBase.cs:
        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range))
        {
            // Sprawdź, czy trafiony obiekt ma skrypt EnemyAI
            EnemyAI enemy = hit.transform.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(weaponData.damage);
            }
        }

        // TODO: Miejsce na dźwięk i efekty wizualne
    }
}