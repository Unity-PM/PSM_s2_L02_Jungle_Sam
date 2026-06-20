# PROJECT_STATUS.md - Jungle Sam

## Aktualny status projektu

Projekt: Jungle Sam  
Typ: FPS Horde Shooter / Vertical Slice / MVP  
Silnik: Unity 6.3 / 6000.3.10f1  
Render Pipeline: URP  
Jezyk: C#  
Input: New Input System  
Nawigacja: Unity AI Navigation / NavMesh  

## Status po aktualizacji 2026-06-19

### Struktura projektu
Projekt zostal uporzadkowany:

```text
Assets/JungleSam/     - kod, prefaby, sceny testowe, SO, settings, UI
Assets/ThirdParty/    - Flooded Grounds, MonsterMutant7, Zombie, bronie, pickup models
```

Usunieto stare puste foldery root:

```text
Assets/Scripts
Assets/Prefabs
Assets/Scenes
Assets/Models
Assets/Images
Assets/Materials
Assets/Settings
```

### Sceny
Build Settings:

```text
Assets/ThirdParty/Flooded_Grounds/Scenes/Scene_A.unity
Assets/JungleSam/Scenes/Test/World.unity
```

Flooded Grounds pozostaje docelowa mapa bazowa.

### Bron
Wdrozone:
- pistolet,
- karabinek 7.62x39mm,
- `WeaponBase` raycast,
- magazynek + reserve ammo,
- full auto / semi auto przez `weaponData.isAutomatic`,
- ammo pickupy dla typow broni,
- `IDamageable` support w `WeaponBase`,
- fallback na stare `EnemyAI`.

Animator broni:
- Triggery: `Shoot`, `Reload`, `ReloadEnd`, `Inspect`
- Boole: `IsWalking`, `IsRunning`

### Zombie
Zombie nadal uzywa `EnemyAI`.
`WeaponBase` nadal potrafi zadac obrazenia zombie przez fallback na `EnemyAI`.

### MutantStalker
Dodany gameplayowy przeciwnik MutantStalker na bazie MonsterMutant7.

Pliki:

```text
Assets/JungleSam/Enemies/MutantStalker/Scripts/MutantStalkerAI.cs
Assets/JungleSam/Enemies/MutantStalker/Scripts/MutantStalkerAnimator.cs
Assets/JungleSam/Enemies/MutantStalker/Editor/MutantStalkerAnimatorControllerBuilder.cs
Assets/JungleSam/Enemies/MutantStalker/Animators/AC_MutantStalker_Gameplay.controller
```

Funkcje:
- NavMeshAgent chase,
- attack melee,
- hit reaction,
- rage,
- death,
- `IDamageable`,
- action locks dla attack/gethit/rage,
- blokada nowych animacji po smierci,
- locomotion guard w animator wrapperze.

Inspector values:

```text
Attack Animation Lock: 1.1
Hit Reaction Lock: 0.28
Hit Reaction Cooldown: 0.35
Rage Animation Lock: 1.2
```

### Aktualne ryzyka
- Po reorganizacji folderow Unity musi przeimportowac projekt; trzeba sprawdzic Console.
- Prefaby powinny zachowac referencje, bo przenoszono assety razem z `.meta`, ale trzeba to potwierdzic w Unity.
- Jesli MutantStalker nadal slizga sie po attack/gethit/rage, nastepny krok to sprawdzenie importu animacji, Avatar/Rig i ustawien klipow MonsterMutant7.

## Najblizsze testy
- Otworzyc `Scene_A.unity`.
- Sprawdzic Build Settings.
- Sprawdzic Player prefab, Zombie prefab, MutantStalker object/prefab.
- Strzelic w zombie i MutantStalker.
- Przetestowac MutantStalker: chase, attack, hit reaction, rage, death.
- Sprawdzic, czy `DealDamageToTarget` jest dodane jako Animation Event, jesli `Damage By Animation Event` jest wlaczone.
