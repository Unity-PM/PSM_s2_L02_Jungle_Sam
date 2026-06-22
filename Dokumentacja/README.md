# Jungle Sam - dokumentacja projektu

To jest glowny indeks dokumentacji. Poprzednie rozbite pliki z opisami broni, ammo, architektury, statusu i roadmapy zostaly scalone, zeby dokumentacja byla krotsza i latwiejsza do utrzymania.

## Aktualne pliki

| Plik | Zawartosc |
| --- | --- |
| `README.md` | Ten indeks, aktualny stan projektu i zasady utrzymania dokumentacji. |
| `TECHNICAL_REFERENCE.md` | Systemy techniczne: bron, ammo, shot feedback, HUD, health, checkpointy, encounter reset, MutantStalker. |
| `MISSION_AND_STORY.md` | Fabula, frakcje, przebieg misji, checkpointy etapow i flow vertical slice. |

## Status projektu

- Projekt: `Jungle Sam`
- Typ: FPS horde shooter / vertical slice / MVP
- Unity: `6000.3.10f1`
- Render pipeline: URP
- Input: New Input System
- Nawigacja: Unity AI Navigation / NavMesh
- Glowna mapa: `Assets/ThirdParty/Flooded_Grounds/Scenes/Scene_A.unity`
- Scena testowa: `Assets/JungleSam/Scenes/Test/World.unity`

## Aktualna struktura

```text
Assets/
  JungleSam/
    Core/
    Docs/                  # nieuzywane po scaleniu; nowe docs sa w /Dokumentacja
    Enemies/
    Prefabs/
    Scenes/
    ScriptableObjects/
    Scripts/
    Settings/
    UI/
  ThirdParty/
    Flooded_Grounds/
    Models/
```

Nie zakladac starych sciezek typu:

```text
Assets/Scripts
Assets/Prefabs
Assets/Scenes
Assets/Models
Assets/Settings
```

## Co jest wdrozone

- `WeaponBase` raycast ze srodka kamery FPS.
- Pistolet i AK z ammo, reloadem, animacjami, dzwiekiem i muzzle flash.
- Magazynek + reserve ammo.
- Semi-auto i full-auto przez `WeaponData.isAutomatic`.
- Pickupy amunicji po `AmmoCategory`.
- `IDamageable` jako preferowany interfejs obrazen.
- Fallback damage do starego `EnemyAI`.
- HUD gameplayowy: HP, armor, ammo, objective, prompt interakcji, notification.
- System smierci i respawnu bez reloadu sceny.
- Checkpointy i reset aktywnego encountera po smierci.
- Story pickupy i objective resettable dla flow radio -> arena.
- MutantStalker jako silniejszy przeciwnik z `IDamageable`, `NavMeshAgent`, action lockami i kontrolerem animatora.

## Zasady utrzymania dokumentacji

- Nowe informacje techniczne dopisywac do `TECHNICAL_REFERENCE.md`.
- Fabule, flow misji, checkpointy etapow i rozmieszczenie lokacji dopisywac do `MISSION_AND_STORY.md`.
- Nie tworzyc osobnego pliku dla kazdej drobnej zmiany.
- Tymczasowe notatki dla AI trzymac w `AI_CODEX/AGENTS.md`, ale nie duplikowac tam calej dokumentacji.
- Jesli dokument przestaje byc aktualny, scalic go z jednym z dwoch plikow kanonicznych albo usunac.

## Szybkie testy po zmianach

1. Otworzyc `Scene_A.unity` albo `World.unity`.
2. Sprawdzic Console po rekompilacji.
3. Przetestowac pistolet:
   - jeden klik = jeden strzal,
   - dzwiek zgrany z animacja,
   - muzzle flash przy lufie,
   - brak feedbacku przy pustym magazynku.
4. Przetestowac AK:
   - full auto uzywa `PlayOneShot`,
   - dzwieki sie nie ucinaja,
   - flash pojawia sie przy kazdym faktycznym strzale.
5. Przetestowac damage na zombie i MutantStalkerze.
6. Przetestowac flow: radio -> arena -> smierc przed ukonczeniem -> respawn -> radio i arena wracaja.
7. Przetestowac flow po ukonczeniu areny: checkpoint aktywny, radio nie wraca, bramy pozostaja w stanie po sukcesie.

