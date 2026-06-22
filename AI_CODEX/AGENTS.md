# AGENTS.md - Jungle Sam

Krotki kontekst dla agentow pracujacych w repo. Pelna dokumentacja jest w folderze `Dokumentacja`.

## Czytaj najpierw

1. `Dokumentacja/README.md`
2. `Dokumentacja/TECHNICAL_REFERENCE.md`
3. `Dokumentacja/MISSION_AND_STORY.md`

Nie odtwarzaj starych rozbitych plikow dokumentacji. Jesli trzeba cos dopisac, dopisz to do jednego z trzech plikow powyzej.

## Projekt

- Unity `6000.3.10f1`
- URP
- C#
- New Input System
- Unity AI Navigation / NavMesh
- FPS horde shooter / vertical slice / MVP
- Klimat: dzungla, zalana strefa, Black Orchid, Grom Division, infekcja, UFO.

## Glowne sceny

```text
Assets/ThirdParty/Flooded_Grounds/Scenes/Scene_A.unity
Assets/JungleSam/Scenes/Test/World.unity
```

## Glowne foldery

```text
Assets/JungleSam/
Assets/ThirdParty/
Dokumentacja/
```

Nie zakladaj starych sciezek:

```text
Assets/Scripts
Assets/Prefabs
Assets/Scenes
Assets/Models
Assets/Settings
```

## Zasady pracy

- Najpierw sprawdz rzeczywiste pliki przez `rg --files` / `rg`.
- Nie zgaduj nazw scen, prefabow, klas ani folderow.
- Nie edytuj assetow third-party bez wyraznego polecenia.
- Nie edytuj oryginalnego MonsterMutant7 Animator Controller.
- Nie rob masowego reimportu.
- Nie commituj automatycznie.
- Preferuj male, konkretne patche.
- Preferuj `[SerializeField] private`.
- Nie zmieniaj publicznych nazw klas/metod/pol bez potrzeby.
- Po zmianach podaj: co zmieniono, co podpiac w Inspectorze, jak testowac.

## Systemy szczegolnie wrazliwe

- `WeaponBase` - raycast, ammo, reload, input, damage, shot feedback.
- `WeaponData` - dane broni, dzwiek, muzzle flash, pitch/volume, delay feedbacku.
- `Player 1 1.prefab` - player, bronie, muzzle points, AudioSource.
- `GameplayHUDController` i builder HUD - nie nadpisywac recznych zmian przy Play Mode.
- `StoryPickupInteractable`, `ObjectiveOnStoryPickup`, `ArenaEncounterController`, `EncounterResetService` - flow radio/arena/reset po smierci.
- `MutantStalkerAI` i `MutantStalkerAnimator` - action locki i animator wrapper.

## MutantStalker

Pracuj tylko tutaj:

```text
Assets/JungleSam/Enemies/MutantStalker/
```

Builder controllera:

```text
Tools > Jungle Sam > Enemies > Build Mutant Stalker Animator Controller
```

## Test minimum po zmianach

```powershell
dotnet build .\Assembly-CSharp.csproj
git diff --check
```

W Unity:

- Console bez compile errors i missing scripts.
- Pistolet: pojedynczy strzal, dzwiek, muzzle flash, reload.
- AK: full auto, `PlayOneShot`, flash na kazdy faktyczny strzal.
- Brak dzwieku/flasha przy pustym magazynku.
- Damage na zombie i MutantStalkerze.
- Radio -> arena -> smierc przed ukonczeniem -> reset.
- Ukonczenie areny -> checkpoint -> smierc -> brak cofania areny.
