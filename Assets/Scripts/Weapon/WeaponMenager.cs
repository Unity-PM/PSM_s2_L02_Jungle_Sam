using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    public GameObject[] weaponSlots; // Tablica z obiektami broni (Pistolet, Karabin, Shotgun)
    private int _currentWeaponIndex = 0;

    void Start()
    {
        SelectWeapon(0); // Zaczynamy z pierwszą bronią
    }

    void Update()
    {
        // Obsługa klawiszy 1, 2, 3 z New Input System
        if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectWeapon(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectWeapon(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectWeapon(2);
    }

    void SelectWeapon(int index)
    {
        // Sprawdzamy, czy slot istnieje
        if (index >= weaponSlots.Length || weaponSlots[index] == null) return;

        // Sprawdzamy, czy broń jest odblokowana (przygotowanie pod sklep)
        WeaponBase weapon = weaponSlots[index].GetComponent<WeaponBase>();
        if (weapon != null && !weapon.isUnlocked)
        {
            Debug.Log("Ta broń jest jeszcze zablokowana!");
            return;
        }

        // Dezaktywujemy wszystkie bronie i aktywujemy wybraną
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            weaponSlots[i].SetActive(i == index);
        }

        _currentWeaponIndex = index;
        Debug.Log("Wybrano broń: " + weaponSlots[index].name);
    }
}