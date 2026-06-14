# Jungle Sam

Unity FPS / Horde Shooter inspired by the fast arcade combat of Serious Sam and wave-based survival shooters. The current MVP focuses on responsive first-person movement, raycast weapons, zombie AI on NavMesh, ammo pickups, HUD feedback, and a configurable wave spawner.

---

## Tech Stack

* **Engine:** Unity 6000.3.10f1
* **Render Pipeline:** Universal Render Pipeline (URP)
* **Language:** C#
* **Input:** Unity New Input System package
* **Navigation:** Unity AI Navigation / NavMesh
* **UI:** TextMesh Pro

---

## Implemented Systems

### Player

* FPS movement built on `CharacterController`.
* Walk, sprint, jump, gravity, and mouse look.
* Weapon movement animation forwarding through `WeaponBase.SetMovementAnimations`.

### Weapons and Ammo

* `WeaponBase` handles raycast shooting, reload timing, reserve ammo, shoot/reload/inspect animator triggers, muzzle flash spawning, and shoot audio.
* `WeaponData` ScriptableObjects store weapon stats, ammo category, fire mode, effects, and prefab references.
* Semi-auto and full-auto weapons are supported through `WeaponData.isAutomatic`.
* `WeaponManager` switches weapon slots with keys 1, 2, and 3.
* `AmmoPack` adds ammo by `AmmoCategory` and supports pickup respawn.

### Enemies

* `EnemyAI` uses `NavMeshAgent` for zombie movement.
* AI logic is throttled with `aiTickRate` instead of running full decision logic every frame.
* Zombies support detection range, chase, attack cooldown, damage, hit animation, death animation, collider disabling, and coin rewards.
* Death is exposed through both an instance event (`Died`) and the existing static action (`OnEnemyDied`) for compatibility.

### Waves

* `WaveSpawner` supports multiple waves, spawn points, max alive enemies per wave, optional looping, optional manual start, and NavMesh spawn sampling.
* Spawned enemies are tracked through the instance `EnemyAI.Died` event.
* Wave status can be displayed through a `TextMeshProUGUI` field.

### UI

* `AmmoUI` displays current magazine ammo and reserve ammo for the selected weapon.
* `PlayerStats` updates health, armor, and coin text fields.
* `ClockUI` displays Polish local time and caches the time-zone lookup.

---

## Important Project Notes

* Do not manually edit Unity `.meta` files.
* Keep scene and prefab changes scoped and intentional.
* Main gameplay scripts currently rely on Inspector references, object tags, and prefab setup.
* The player object must have the `Player` tag for `EnemyAI`.
* Wave spawners require configured waves, enemy prefabs, spawn points, and a baked NavMesh.
* Weapon prefabs need `WeaponBase`, assigned `WeaponData`, animator parameters, and optional `AudioSource`.

---

## Main Script Locations

* `Assets/Scripts/Player/PlayerController.cs`
* `Assets/Scripts/Player/PlayerStats.cs`
* `Assets/Scripts/Weapon/WeaponBase.cs`
* `Assets/Scripts/Weapon/WeaponData.cs`
* `Assets/Scripts/Weapon/WeaponManager.cs`
* `Assets/Scripts/Enemy/EnemyAI.cs`
* `Assets/Scripts/Spawning/WaveSpawner.cs`
* `Assets/Scripts/Pickups/AmmoPack.cs`
* `Assets/Scripts/UI/AmmoUI.cs`
* `Assets/Scripts/UI/ClockUI.cs`
