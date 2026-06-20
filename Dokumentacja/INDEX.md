# 📖 Jungle Sam - Dokumentacja Projektu (INDEX)

**Projekt:** Jungle Sam (FPS w stylu Serious Sam)  
**Engine:** Unity  
**Target:** .NET Standard 2.1  
**Status:** ✅ Funkcjonalny (v2.0)  
**Data:** 2025

---

## 🗂️ Dokumentacja (Plik → Cel)

### 1. **SYSTEM_BRONI_DOKUMENTACJA.md** ⭐ START HERE
**Cel:** Pełna analiza architektoniki i działania systemu  
**Dla kogo:** Programiści chcący zrozumieć cały system  
**Zawiera:**
- Przegląd projektu
- Opis wszystkich klas (WeaponBase, WeaponData, PlayerController, EnemyAI, etc.)
- Flow strzału i przeładowania (diagramy tekstowe)
- System animacji (State Machine)
- Znane problemy i rozwiązania
- Parametry Animatora

**Jak czytać:** 20-30 minut, top-to-bottom

---

### 2. **QUICK_REFERENCE.md** ⚡ FOR QUICK LOOKUP
**Cel:** Szybkie notatki i Setup guides  
**Dla kogo:** Artyści (Animacja), Projektanci (Balans), Programiści (Setup)  
**Zawiera:**
- Sterowanie gracza (keyboard mapping)
- Animator Controller Setup (step-by-step)
- WeaponData konfiguracja (dla każdej broni)
- Typowe problemy & rozwiązania
- Checklist przed deployem

**Jak czytać:** 10-15 minut, Ctrl+F po słowie kluczowym

---

### 3. **ARCHITECTURE_DIAGRAMS.md** 🎨 VISUAL LEARNERS
**Cel:** Diagramy i flowcharty (ASCII art)  
**Dla kogo:** Visual learners, System designers  
**Zawiera:**
- Hierarchia sceny (GameObject tree)
- Data flow diagrams (Input → Output)
- Shooting system flowchart
- Reload system flowchart
- Animator State Machine diagram (detailed)
- Runtime data structures
- Dependency graph

**Jak czytać:** 15-20 minut, przeglądaj diagramy

---

### 4. **ROADMAP_AND_TODOS.md** 🚀 FUTURE WORK
**Cel:** Co już zrobiono, co trzeba zrobić, timeline  
**Dla kogo:** Project Manager, Team Lead, Next Developer  
**Zawiera:**
- ✅ Completed features
- 🟡 In progress
- ❌ TODO (fazy 1-4)
- Pseudocode dla każdego feature'a
- Estimated hours per task
- Prioritization
- Known bugs & technical debt

**Jak czytać:** 20-30 minut, focus na sekcji TODO

---

## 🎯 Quick Navigation

### "Chcę zrozumieć jak działa system broni"
→ `SYSTEM_BRONI_DOKUMENTACJA.md` (sekcja: Architektura Systemu Broni)

### "Chcę skonfigurować Animator dla nowej broni"
→ `QUICK_REFERENCE.md` (sekcja: Animator Controller Setup)

### "Chcę zobaczyć przepływy danych"
→ `ARCHITECTURE_DIAGRAMS.md` (sekcja: Data Flow Diagram)

### "Chcę wiedzieć co się dzieje w Update()"
→ `ARCHITECTURE_DIAGRAMS.md` (sekcja: Animator State Machine → Detailed)

### "Chcę dodać nowy feature (np. grenades)"
→ `ROADMAP_AND_TODOS.md` (sekcja: Faza 3: Advanced Combat)

### "Chcę debugować problem"
→ `QUICK_REFERENCE.md` (sekcja: Typowe problemy)

### "Chcę poznać flow strzału"
→ `SYSTEM_BRONI_DOKUMENTACJA.md` (sekcja: Flow Strzału i Przeładowania)

---

## 📋 Główne Systemy

```
┌─ WEAPON SYSTEM ────────────────────────────────┐
│ ├─ WeaponBase.cs (logika)                      │
│ ├─ WeaponData.cs (konfiguracja)                │
│ └─ Animator (animacje broni)                   │
├─ PLAYER SYSTEM ───────────────────────────────┤
│ ├─ PlayerController.cs (ruch + look)           │
│ ├─ PlayerStats.cs (HP, armor, UI)              │
│ └─ WeaponManager.cs (zmiana broni)             │
├─ ENEMY SYSTEM ────────────────────────────────┤
│ ├─ EnemyAI.cs (inteligencja, ataki)            │
│ └─ NavMesh (nawigacja)                         │
├─ WAVE SYSTEM ─────────────────────────────────┤
│ └─ WaveSpawner.cs (generator fal)              │
└─ UI SYSTEM ────────────────────────────────────┘
    └─ Health, Armor, Wave counter
```

---

## 🔑 Key Classes Summary

| Klasa | Plik | Rola | Kluczowe metody |
|-------|------|------|-----------------|
| `WeaponBase` | `Assets/Scripts/Weapon/WeaponBase.cs` | Logika broni | `Shoot()`, `Reload()`, `TryShoot()`, `SetMovementAnimations()` |
| `WeaponData` | `Assets/Scripts/Weapon/WeaponData.cs` | Config (SO) | N/A (data only) |
| `PlayerController` | `Assets/Scripts/PlayerController.cs` | Ruch, patrzenie | `ReadInput()`, `HandleMovement()`, `UpdateWeaponAnimations()` |
| `PlayerStats` | `Assets/Scripts/PlayerStats.cs` | HP, armor | `TakeDamage()`, `UpdateUI()` |
| `EnemyAI` | `Assets/Scripts/EnemyAI.cs` | Przeciwnik AI | `TakeDamage()`, `Attack()`, `Die()` |
| `WaveSpawner` | `Assets/Scripts/WaveSpawner.cs` | Spawn system | `SpawnWave()`, `WaveCompleted()` |
| `WeaponManager` | `Assets/Scripts/Weapon/WeaponManager.cs` | Switch broń | `SelectWeapon()` |

---

## 🎬 Animation State Machine

```
Entry → Hands|Origin (Idle)
  ↓
Hands|Walk ↔ Hands|Run
  ↓
Any State → Hands|Shot (trigger: Shoot)
Any State → Hands|Reload (trigger: Reload)
```

**Parametry:** `IsWalking`, `IsRunning`, `Shoot`, `Reload`

---

## 📊 Important Numbers

| Parametr | Wartość | Zmiennie |
|----------|---------|----------|
| Fire Rate (Pistolet) | 0.25s | Per WeaponData |
| Shoot Animation | 0.25s | Per WeaponData |
| Reload Time (Pistolet) | 1.5s | Per WeaponData |
| Walk Speed | 7 m/s | PlayerController |
| Sprint Speed | 12 m/s | PlayerController |
| Health | 100 | PlayerStats |
| Enemy Health | 50 | EnemyAI |
| Wave Countdown | 5s | WaveSpawner |

---

## ✅ Checklist - Przed pracą nad projektem

- [ ] Przeczytaj `SYSTEM_BRONI_DOKUMENTACJA.md` (top-to-bottom)
- [ ] Otwórz Unity projekt
- [ ] Uruchom grę i przetestuj (W/A/S/D + myszka + LPM + R)
- [ ] Otwórz Animator window i obserwuj State Machine przy grze
- [ ] Przeczytaj `QUICK_REFERENCE.md` dla szybkiego setup
- [ ] Jeśli masz problemy → `QUICK_REFERENCE.md` sekcja: Typowe problemy
- [ ] Jeśli dodajesz feature → `ROADMAP_AND_TODOS.md` dla pseudokodu

---

## 🐛 Debugging Quick Start

```powershell
# Uruchom grę w Unity
Play Button (Ctrl+P)

# Otwórz Console
Ctrl+Shift+C

# Filtruj logi
Window → Console → Filter (wpisz "Strzał" / "Przeładowanie")

# Obserwuj Animator
Window → Animator (podglądaj state transitions)

# Live inspect
Zaznacz broń w hierarchii → Inspector → WeaponBase
  - obserwuj _currentAmmo
  - obserwuj _isReloading
  - obserwuj _nextFireTime
```

---

## 🔗 Linki do kodów (w projekcie)

Wszystkie klasy znajdują się w:
```
Assets/Scripts/
├── Weapon/
│   ├── WeaponBase.cs
│   ├── WeaponData.cs
│   └── WeaponManager.cs
├── PlayerController.cs
├── PlayerStats.cs
├── EnemyAI.cs
└── WaveSpawner.cs
```

---

## 📞 Szybki Contact / Notes

**Branch:** `master`  
**Remote:** `https://github.com/Unity-PM/PSM_s2_L02_Jungle_Sam`

**Jeśli masz pytanie:**
1. Szukaj w dokumentacji (Ctrl+F)
2. Sprawdź `QUICK_REFERENCE.md` → Typowe problemy
3. Debuguj w Unity (Console + Animator)
4. Czytaj komentarze w kodzie

---

## 📊 File Summary

```
Sprawozdania/
├── INDEX.md (TEN PLIK)
│   └─ Overview & navigation
│
├── SYSTEM_BRONI_DOKUMENTACJA.md (MAIN)
│   └─ Pełna analiza architektoniki
│
├── QUICK_REFERENCE.md (QUICK LOOKUP)
│   └─ Setup guides & quick notes
│
├── ARCHITECTURE_DIAGRAMS.md (VISUAL)
│   └─ Diagramy i flowcharty
│
└── ROADMAP_AND_TODOS.md (FUTURE)
    └─ Plany rozwoju & TODO list
```

**Całkowicie:** ~30 stron dokumentacji  
**Czas czytania:** 1-2 godziny (cały stack)

---

## 🎓 Rekomendowana kolejność czytania

```
Dla całkiem nowego programisty:
1. Ten index (5 min)
2. QUICK_REFERENCE.md (10 min)
3. SYSTEM_BRONI_DOKUMENTACJA.md (30 min)
4. ARCHITECTURE_DIAGRAMS.md (20 min)
5. Uruchom grę i testuj (30 min)
→ TOTAL: ~1.5h (teraz możesz code!)

Dla artysty (animator):
1. QUICK_REFERENCE.md (10 min)
2. ARCHITECTURE_DIAGRAMS.md → Animator section (10 min)
3. Otwórz Animator w Unity (15 min)
→ TOTAL: ~35 min (gotowy do animacji!)

Dla project managera:
1. SYSTEM_BRONI_DOKUMENTACJA.md → Przegląd (10 min)
2. ROADMAP_AND_TODOS.md (20 min)
→ TOTAL: 30 min
```

---

## 🚀 Getting Started (Developer)

```bash
# Clone repo
git clone https://github.com/Unity-PM/PSM_s2_L02_Jungle_Sam.git
cd "Jungle Sam"

# Open in Unity 2022+ (LTS recommended)
# File → Open Project → select folder

# Test the game
Play (Ctrl+P)
  - W/A/S/D: move
  - Mouse: look
  - LPM: shoot
  - R: reload
  - 1/2/3: weapon switch
  - Shift: sprint

# Read documentation
Sprawozdania/SYSTEM_BRONI_DOKUMENTACJA.md

# Start coding
Assets/Scripts/Weapon/WeaponBase.cs
```

---

**Dokument przygotowany do szybkiego onboardingu i przyszłego rozwoju projektu.**

```
Version: 2.0 (Complete)
Status: Ready for Team Handoff
Last Modified: 2025
```

# Update 2026-06-19 - Current Context

Read first for the newest state:

```text
Dokumentacja/UPDATE_2026_06_19.md
AI_CODEX/AGENTS.md
AI_CODEX/PROJECT_STATUS.md
AI_CODEX/CODEX_TASKS.md
```

Current paths changed after cleanup:

```text
Assets/JungleSam/Scripts/
Assets/JungleSam/Prefabs/
Assets/JungleSam/Scenes/Test/
Assets/JungleSam/Enemies/MutantStalker/
Assets/ThirdParty/Flooded_Grounds/
Assets/ThirdParty/Models/
```

Old root paths like `Assets/Scripts`, `Assets/Prefabs`, `Assets/Scenes`, `Assets/Models`, and `Assets/Flooded_Grounds` are obsolete.

Current target map:

```text
Assets/ThirdParty/Flooded_Grounds/Scenes/Scene_A.unity
```

MutantStalker gameplay files:

```text
Assets/JungleSam/Enemies/MutantStalker/Scripts/MutantStalkerAI.cs
Assets/JungleSam/Enemies/MutantStalker/Scripts/MutantStalkerAnimator.cs
Assets/JungleSam/Enemies/MutantStalker/Editor/MutantStalkerAnimatorControllerBuilder.cs
Assets/JungleSam/Enemies/MutantStalker/Animators/AC_MutantStalker_Gameplay.controller
```

Do not edit the original MonsterMutant7 Animator Controller.

---
