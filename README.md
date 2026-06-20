# Jungle Sam

Unity FPS / Horde Shooter inspired by the fast arcade combat of Serious Sam and wave-based survival shooters. The current MVP focuses on responsive first-person movement, raycast weapons, zombie AI on NavMesh, ammo pickups, HUD feedback, and a configurable wave spawner.


## Tech Stack

* **Engine:** Unity 6000.3.10f1
* **Render Pipeline:** Universal Render Pipeline (URP)
* **Language:** C#
* **Input:** Unity New Input System package
* **Navigation:** Unity AI Navigation / NavMesh
* **UI:** TextMesh Pro



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

* `Assets/JungleSam/Scripts/Player/PlayerController.cs`
* `Assets/JungleSam/Scripts/Player/PlayerStats.cs`
* `Assets/JungleSam/Scripts/Weapon/WeaponBase.cs`
* `Assets/JungleSam/Scripts/Weapon/WeaponData.cs`
* `Assets/JungleSam/Scripts/Weapon/WeaponManager.cs`
* `Assets/JungleSam/Scripts/Enemy/EnemyAI.cs`
* `Assets/JungleSam/Scripts/Spawning/WaveSpawner.cs`
* `Assets/JungleSam/Scripts/Pickups/AmmoPack.cs`
* `Assets/JungleSam/Scripts/UI/AmmoUI.cs`
* `Assets/JungleSam/Scripts/Combat/IDamageable.cs`
* `Assets/JungleSam/Enemies/MutantStalker/Scripts/MutantStalkerAI.cs`
* `Assets/JungleSam/Enemies/MutantStalker/Scripts/MutantStalkerAnimator.cs`

---

## Project Layout Update - 2026-06-19

The project was reorganized into first-party and third-party areas:

```text
Assets/
  JungleSam/
    Scripts/
    Prefabs/
    Scenes/Test/
    ScriptableObjects/Weapons/
    Settings/
    UI/Sprites/
    Enemies/MutantStalker/
  ThirdParty/
    Flooded_Grounds/
    Models/
```

Current Build Settings scenes:

```text
Assets/ThirdParty/Flooded_Grounds/Scenes/Scene_A.unity
Assets/JungleSam/Scenes/Test/World.unity
```

`Flooded_Grounds` remains the target map base. Avoid using the old root paths such as `Assets/Scripts`, `Assets/Prefabs`, `Assets/Scenes`, `Assets/Models`, and `Assets/Flooded_Grounds`.

---

## MutantStalker Status - 2026-06-19

MutantStalker is a gameplay enemy built from the imported MonsterMutant7 asset. Do not edit the original MonsterMutant7 Animator Controller.

Work only in:

```text
Assets/JungleSam/Enemies/MutantStalker/
```

Important files:

```text
Assets/JungleSam/Enemies/MutantStalker/Scripts/MutantStalkerAI.cs
Assets/JungleSam/Enemies/MutantStalker/Scripts/MutantStalkerAnimator.cs
Assets/JungleSam/Enemies/MutantStalker/Editor/MutantStalkerAnimatorControllerBuilder.cs
Assets/JungleSam/Enemies/MutantStalker/Animators/AC_MutantStalker_Gameplay.controller
```

Animator builder menu:

```text
Tools > Jungle Sam > Enemies > Build Mutant Stalker Animator Controller
```

Inspector values for current action locks:

```text
Attack Animation Lock: 1.1
Hit Reaction Lock: 0.28
Hit Reaction Cooldown: 0.35
Rage Animation Lock: 1.2
```

Damage flow:
- `Assets/JungleSam/Scripts/Combat/IDamageable.cs` was added.
- `MutantStalkerAI` implements `IDamageable`.
- `WeaponBase` checks `IDamageable` first, then falls back to the old `EnemyAI` damage path.

Unity verification:
- Let Unity compile and reimport after folder moves.
- Check Console for missing scripts and missing references.
- Open `Assets/ThirdParty/Flooded_Grounds/Scenes/Scene_A.unity`.
- Test shooting old zombie `EnemyAI` and new `MutantStalkerAI`.
- Test MutantStalker chase, attack, hit reaction, rage and death.



