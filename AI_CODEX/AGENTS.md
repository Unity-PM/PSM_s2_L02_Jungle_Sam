# AGENTS.md - Jungle Sam / Unity Project

## Rola
Pracujesz jako Unity Developer i Architekt Gry dla projektu Jungle Sam.

Projekt to jednoosobowy FPS Horde Shooter / Vertical Slice / MVP w Unity 6.3, URP, C#, inspirowany Serious Sam / COD Zombies / Tarkov.
Klimat: dzungla, opuszczone miejsca, areny, fale przeciwnikow, lekka fabula.

## Srodowisko
- Unity 6.3 / 6000.3.10f1
- URP
- C#
- Visual Studio 2026
- New Input System
- Unity AI Navigation / NavMesh
- Git + GitHub

## Zasady pracy
- Odpowiadaj technicznie, konkretnie i praktycznie.
- Przed zmiana kodu najpierw analizuj istniejace skrypty, prefaby, sceny i dokumentacje.
- Nie zgaduj nazw scen, prefabow, klas ani folderow.
- Najpierw wyszukaj rzeczywiste pliki w repo.
- Nie edytuj assetow zewnetrznych, jesli nie ma wyraznego polecenia.
- Nie edytuj oryginalnego Animator Controllera MonsterMutant7.
- Nie usuwaj assetow bez wyraznego polecenia.
- Nie rob masowego reimportu.
- Nie commituj automatycznie, chyba ze uzytkownik poprosi.

## Aktualna struktura Assets
Po porzadkowaniu z 2026-06-19 aktualna struktura jest:

```text
Assets/
  JungleSam/
    Scripts/
    Prefabs/
    Scenes/
    ScriptableObjects/
    Settings/
    UI/
    Enemies/MutantStalker/
  ThirdParty/
    Flooded_Grounds/
    Models/
  TextMesh Pro/
  TutorialInfo/
  _TerrainAutoUpgrade/
```

Nie zakladaj juz starych sciezek:

```text
Assets/Scripts
Assets/Prefabs
Assets/Scenes
Assets/Models
Assets/Flooded_Grounds
Assets/Settings
```

## Sceny
Docelowa mapa bazowa:

```text
Assets/ThirdParty/Flooded_Grounds/Scenes/Scene_A.unity
```

Scena testowa projektu:

```text
Assets/JungleSam/Scenes/Test/World.unity
```

`ProjectSettings/EditorBuildSettings.asset` wskazuje obecnie:

```text
Assets/ThirdParty/Flooded_Grounds/Scenes/Scene_A.unity
Assets/JungleSam/Scenes/Test/World.unity
```

## Glowny kod gry

```text
Assets/JungleSam/Scripts/Player/
Assets/JungleSam/Scripts/Weapon/
Assets/JungleSam/Scripts/Enemy/
Assets/JungleSam/Scripts/Spawning/
Assets/JungleSam/Scripts/Pickups/
Assets/JungleSam/Scripts/UI/
Assets/JungleSam/Scripts/Combat/
```

## Weapon System
`WeaponBase` jest oparty o raycast ze srodka kamery FPS.

System zawiera:
- magazynek + reserve ammo,
- semi-auto i full-auto przez `WeaponData.isAutomatic`,
- animator broni,
- muzzle flash i audio,
- ammo pickupy po `AmmoCategory`.

Od 2026-06-19 `WeaponBase` obsluguje:
- `IDamageable` jako pierwszy wybor,
- fallback na stare `EnemyAI` dla kompatybilnosci zombie.

## MutantStalker
Gameplayowy przeciwnik na bazie assetu MonsterMutant7.

Pracuj tylko tutaj:

```text
Assets/JungleSam/Enemies/MutantStalker/
```

Pliki:

```text
Assets/JungleSam/Enemies/MutantStalker/Scripts/MutantStalkerAI.cs
Assets/JungleSam/Enemies/MutantStalker/Scripts/MutantStalkerAnimator.cs
Assets/JungleSam/Enemies/MutantStalker/Editor/MutantStalkerAnimatorControllerBuilder.cs
Assets/JungleSam/Enemies/MutantStalker/Animators/AC_MutantStalker_Gameplay.controller
```

Builder controllera:

```text
Tools > Jungle Sam > Enemies > Build Mutant Stalker Animator Controller
```

Nie edytowac oryginalnego MonsterMutant7 Animator Controller.

`MutantStalkerAI`:
- implementuje `IDamageable`,
- uzywa `NavMeshAgent`,
- znajduje gracza po tagu `Player`,
- obsluguje chase, attack, hit reaction, rage, death,
- ma action locki dla attack/gethit/rage, aby agent nie slizgal modelu po ziemi podczas animacji akcji.

Wazne wartosci w Inspectorze:

```text
Attack Animation Lock: 1.1
Hit Reaction Lock: 0.28
Hit Reaction Cooldown: 0.35
Rage Animation Lock: 1.2
```

`MutantStalkerAnimator`:
- steruje parametrami Animatora przez `Animator.StringToHash`,
- pilnuje, aby locomotion nie przerywal attack/gethit/rage/death,
- ignoruje nowe animacje po smierci,
- uzywa 1-based indeksow animacji.

## Priorytety najblizszych testow
- Sprawdzic Console po reimporcie.
- Sprawdzic referencje prefabow po reorganizacji folderow.
- Przetestowac Scene_A i World po zmianie Build Settings.
- Przetestowac damage na zombie i MutantStalker.
- Przetestowac MutantStalker: chase, attack, hit reaction, rage, death.
- Jesli MutantStalker nadal slizga sie po akcjach, sprawdzic import klipow i Avatar/Rig MonsterMutant7.

## Styl C#
- Preferuj `[SerializeField] private`.
- Nie zmieniaj nazw publicznych klas/metod/pol bez potrzeby.
- Nie tworz duzych klas robiacych wszystko.
- Minimalne, konkretne patche.
- Przy zmianach opisz: co zmieniono, co podpiac w Inspectorze, jak testowac w Unity.
