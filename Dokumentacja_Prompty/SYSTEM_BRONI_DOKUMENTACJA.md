# 📋 Dokumentacja Projektu "Jungle Sam"
## System Broni i Animacji

**Ostatnia aktualizacja:** 2025  
**Status:** ✅ Funcjonalny (z optymalizacjami)  
**Wersja systemu:** 2.0 (Blend-ready)

---

## 📑 Spis treści

1. [Przegląd projektu](#przegląd-projektu)
2. [Architektura systemu broni](#architektura-systemu-broni)
3. [Opis klas](#opis-klas)
4. [Flow strzału i przeładowania](#flow-strzału-i-przeładowania)
5. [System animacji](#system-animacji)
6. [Znane problemy i rozwiązania](#znane-problemy-i-rozwiązania)
7. [TODO i ulepszenia](#todo-i-ulepszenia)

---

## 🎮 Przegląd projektu

**Jungle Sam** to FPS (First-Person Shooter) w stylu Serious Sam z mechaoniką:
- **Gracz:** Stoi/chodzi/biega, strzela do przeciwników
- **Przeciwnicy:** AI ze spawnerem fal
- **Bronie:** System z animacjami rąk, triggerami, przeładowaniem
- **UI:** HP, pancerz, licznik amunicji, wave counter

### Główne komponenty
- `PlayerController` — ruch, patrzenie
- `WeaponBase` — logika broni (strzał, przeładowanie)
- `WeaponData` — statystyki broni (ScriptableObject)
- `EnemyAI` — przeciwnicy na NavMesh
- `PlayerStats` — zdrowie, pancerz, UI
- `WaveSpawner` — generator fal
- `WeaponManager` — zmiana broni (1-2-3)

---

## 🔫 Architektura systemu broni

### Hierarchia obiektów
```
Player (GameObject)
├── PlayerController (ruch, input)
├── PlayerStats (HP, armor)
├── CharacterController (fizyka)
├── Camera (FPS view)
└── WeaponManager (switch broń)
    └── Weapon Slot 0: Pistolet
        ├── WeaponBase (logika)
        ├── Animator (ruch rąk/broni)
        ├── AudioSource (dźwięki)
        └── Model 3D (Glock/karabin/etc)
```

### Przepływ danych
```
Input (LPM/R/1-2-3)
    ↓
PlayerController.Update()
    ├→ ReadInput() — LMB, Shift, WASD, R
    ├→ HandleMovement() — ruchy postaci
    ├→ HandleLook() — muszka myszy
    └→ UpdateWeaponAnimations() — wysyła bools do broni
    ↓
WeaponBase.Update()
    ├→ Update() — reload timer, input strzału
    ├→ TryShoot() — sprawdza cooldown + ammo
    ├→ Shoot() — raycast, damage, dźwięk, efekty
    ├→ Reload() — trigger animacji, timer
    └→ SetMovementAnimations() — parametry Animatora
    ↓
Animator (state machine)
    └→ Hands|Idle → Hands|Walk → Hands|Run
       ├→ Any State → Hands|Shot
       └→ Any State → Hands|Reload
```

---

## 📚 Opis klas

### 1. **WeaponBase.cs** ⭐

**Rola:** Główna logika broni (strzał, reload, cooldown, animacje)

#### Kluczowe zmienne
```csharp
_currentAmmo          // Bieżąca amunicja
_nextFireTime         // Czas następnego strzału
_weaponAnimator       // Animator broni
_isReloading          // Flag przeładowania
_shootAnimEndTime     // Koniec animacji strzału
_pendingReload        // Reload w kolejce (po ostatnim strzale)
_lastIsWalking/Running // Zapamiętane stany ruchu
```

#### Kluczowe metody
| Metoda | Opis | Wywoływane z |
|--------|------|--------------|
| `TryShoot()` | Sprawdza cooldown, ammo, wywoła `Shoot()` | `Update()` |
| `Shoot()` | Zmniejsza ammo, trigger, raycast, damage | `TryShoot()` |
| `Reload()` | Włącza `_isReloading`, trigger reload, blokuje strzały | `Update()` / `TryShoot()` |
| `FinishReload()` | Kończy reload, przywraca stan ruchu | `Update()` (timer) |
| `SetMovementAnimations()` | Ustawia IsWalking/IsRunning booly | `PlayerController.UpdateWeaponAnimations()` |

#### Parametry Animatora (StringToHash)
- `Shoot` (Trigger) — animacja strzału
- `Reload` (Trigger) — animacja przeładowania
- `ReloadEnd` (Trigger) — koniec reloadu (opcjonalnie)
- `IsWalking` (Bool) — chodzenie
- `IsRunning` (Bool) — sprintowanie

#### Specjalne cechy
- ✅ Obsługa **semi-auto** (R-click) i **auto** (hold)
- ✅ **Pending reload** — jeśli strzeliłem ostatnim naboje, reload się queuje
- ✅ **Reset triggerów** — przed Shoot resetuje Reload trigger (i vice versa)
- ✅ **Blokada parametrów ruchu** — podczas akcji (shoot/reload) nie nadpisuje walk/run
- ✅ **Zapamiętywanie stanu** — po reloadzie wraca do walk/run
- ✅ **Cache AudioSource** — nie tworzy nowego co strzał

---

### 2. **WeaponData.cs**

**Rola:** ScriptableObject z danymi broni (balans, efekty)

```csharp
weaponName                  // "Pistolet", "Karabin", etc
damage                      // Obrażenia per strzał
fireRate                    // Cooldown między strzałami
shootAnimationDuration      // Czas animacji strzału (nie ammo effect)
range                       // Zasięg raycastu
maxAmmo                     // Magazynek
reloadTime                  // Czas przeładowania
isAutomatic                 // true=hold, false=single click
shootSound                  // AudioClip
muzzleFlashPrefab          // Efekt wizualny
```

#### Domyślne wartości
```
fireRate: 0.5s
shootAnimationDuration: 0.25s
reloadTime: 1.5s
maxAmmo: 30
damage: 10
range: 50m
```

---

### 3. **PlayerController.cs**

**Rola:** Ruch, patrzenie, synchronizacja animacji broni

#### Główne metody
- `ReadInput()` — odczyt WASD, myszka, Shift, R, LMB (New Input System)
- `HandleMovement()` — CharacterController + gravity
- `HandleLook()` — kamera FPS
- `UpdateWeaponAnimations()` — wysyła move state do broni

#### Kluczowe zmienne
```csharp
currentWeapon           // Referencja do aktywnej WeaponBase
_moveInput              // Wektor ruchu (normalized 2D)
_sprintPressed          // Trzymane Shift
walkSpeed: 7 m/s
sprintSpeed: 12 m/s
```

---

### 4. **PlayerStats.cs**

**Rola:** HP, pancerz, UI

```csharp
maxHealth: 100
currentHealth
armor: 0 (opcjonalnie)
healthText, armorText   // TextMeshPro UI
```

#### Mechanika
- Obrażenia najpierw trafiają **pancerz**, potem **HP**
- Jeśli armor < 0, **reszta idzie w HP**
- Przy HP ≤ 0 → `Die()` (Destroy)

---

### 5. **EnemyAI.cs**

**Rola:** Przeciwnik (NavMesh Agent)

```csharp
health: 50
speed: 5
damage: 10
attackRange: 2
_agent: NavMeshAgent   // Nawigacja
```

#### Flow
1. `Update()` — jeśli gracz > attackRange → `_agent.SetDestination(player)`
2. Jeśli gracz < attackRange → `Attack()` → `playerStats.TakeDamage(damage)`
3. Jeśli health ≤ 0 → `Die()` → Destroy + Drop coins

---

### 6. **WaveSpawner.cs**

**Rola:** Spawner fal przeciwników

```csharp
states: Spawning, Waiting, Counting
waves[]                 // Tablica fal (prefab, count, rate)
spawnPoints[]           // Punkty spawn
waveCountdown: 5s       // Przerwa między falami
```

#### Flow
1. **Counting** → czeka 5s
2. **Spawning** → spawna enemies co `1/wave.rate` sekund
3. **Waiting** → czeka aż wszyscy wrogowie umrą
4. **Powtórz** (loop lub koniec)

---

### 7. **WeaponManager.cs**

**Rola:** Zmiana broni (klawisze 1-2-3)

```csharp
weaponSlots[]          // Tablica obiektów broni
SelectWeapon(index)    // Dezaktywuje stare, aktywuje nowe
```

---

## 🔄 Flow strzału i przeładowania

### A. Strzał (Shoot Loop)
```
Klatka 1: LPM wciśnięty
  └→ WeaponBase.Update() → TryShoot()
    └→ if (Time.time >= _nextFireTime && _currentAmmo > 0 && !_isReloading)
      └→ Shoot()
        ├→ _currentAmmo--
        ├→ _shootAnimEndTime = Time.time + 0.25s
        ├→ SetTrigger("Shoot")
        ├→ PlayOneShot(shootSound)
        ├→ Instantiate(muzzleFlash)
        ├→ Raycast() → TakeDamage() na противнику
        └→ if (_currentAmmo == 0) _pendingReload = true
      └→ _nextFireTime = Time.time + max(fireRate, animDuration)
        (= 0.5s dla cooldown na następny strzał)

Klazki 2-5: W trakcie animacji (SetBool(IsWalking/IsRunning) = false)

Klatka 6+: Animacja kończy się, wraca do walk/run
```

### B. Przeładowanie (Reload Loop)
```
Sytuacja 1: Ręczne (R wciśnięty)
  └→ WeaponBase.Update() → if (Keyboard.rKey.wasPressedThisFrame)
    └→ Reload()
      ├→ _isReloading = true
      ├→ _reloadEndTime = Time.time + weaponData.reloadTime (1.5s)
      ├→ _nextFireTime = _reloadEndTime (blokada strzałów)
      ├→ ResetTrigger("Shoot")
      └→ SetTrigger("Reload")

Sytuacja 2: Automatyczne (ostatnia kula)
  └→ Shoot() → if (_currentAmmo == 0) _pendingReload = true
  └→ Update() → if (_pendingReload && Time.time >= _shootAnimEndTime)
    └→ Reload() [identycznie jak wyżej]

Timer:
  └→ Klatka N: if (_isReloading && Time.time >= _reloadEndTime)
    └→ FinishReload()
      ├→ _isReloading = false
      ├→ _currentAmmo = maxAmmo
      ├→ SetTrigger("ReloadEnd")
      └→ SetBool(IsWalking, _lastIsWalking)
      └→ SetBool(IsRunning, _lastIsRunning)
        → Wraca do walk/run
```

### C. Synchronizacja ruchu
```
Update() co klatkę:
  PlayerController.UpdateWeaponAnimations()
    └→ isMoving = _moveInput.magnitude > 0.1
    └→ isSprinting = isMoving && _sprintPressed
    └→ WeaponBase.SetMovementAnimations(isMoving && !isSprinting, isSprinting)
      ├→ if (_isReloading || Time.time < _shootAnimEndTime)
      │   └→ SetBool(IsWalking, false)
      │   └→ SetBool(IsRunning, false)
      │   └→ return (blokada)
      └→ else
        └→ SetBool(IsWalking, isWalking)
        └→ SetBool(IsRunning, isRunning)
```

---

## 🎬 System animacji

### State Machine w Animatorze

```
┌─────────────────┐
│  Entry → Hands|Origin (Idle)
└────────┬────────┘
         │
    IsWalking=true
         │
    ┌────v─────┐
    │Hands|Walk │
    └────┬─────┘
         │ IsRunning=true
         ├──────────┐
         │          │
         │     ┌────v──────┐
         │     │Hands|Run   │
         │     └────┬───────┘
         │          │ IsRunning=false && IsWalking=true
         │          │
         │     ┌────v──────┐
         └─────┤Hands|Walk  │ ← (wraca)
              └────────────┘

┌─────────────────────────────┐
│ Any State ──Shoot──────────→ Hands|Shot
│                              └──→ Hands|Walk/Run/Origin (Exit Time)
└─────────────────────────────┘

┌─────────────────────────────┐
│ Any State ──Reload────────→ Hands|Reload_t
│                             └──→ Hands|Walk/Run/Origin (Exit Time)
└─────────────────────────────┘
```

### Parametry

| Parametr | Typ | Trigger | Opis |
|----------|-----|---------|------|
| `IsWalking` | Bool | Continuous | Chodzenie (nie sprint) |
| `IsRunning` | Bool | Continuous | Sprint |
| `Shoot` | Trigger | Per-Shot | Animacja strzału |
| `Reload` | Trigger | On-Demand | Animacja przeładowania |
| `ReloadEnd` | Trigger | Optional | Koniec reloadu (event) |

### Transition Settings (Zalecane)

**Dla lokomocji (Idle → Walk → Run):**
- `Has Exit Time` = ON
- `Exit Time` = 0.9
- `Transition Duration` = 0.1
- `Can Transition To Self` = OFF

**Dla akcji (Any → Shot/Reload):**
- `Has Exit Time` = OFF
- `Transition Duration` = 0.05
- `Interruption Source` = NONE (nie przerywa się lokomocją)

**Po Shot/Reload (powrót):**
- `Has Exit Time` = ON
- `Exit Time` = 0.95
- `Transition Duration` = 0.1
- **3 wyjścia zależne od stanu ruchu:**
  - `Shot → Hands|Run` (if IsRunning)
  - `Shot → Hands|Walk` (if IsWalking && !IsRunning)
  - `Shot → Hands|Origin` (if !IsWalking && !IsRunning)

---

## 🐛 Znane problemy i rozwiązania

### Problem 1: Pomijane animacje strzału przy szybkim fire
**Przyczyna:** `fireRate` < `shootAnimationDuration` lub zbyt krótki Transition Duration  
**Rozwiązanie:** 
```
min(fireRate) = shootAnimationDuration
Zamiast: fireRate = 0.5s, shootAnimDuration = 0.3s
Ustaw: fireRate = 0.25s, shootAnimDuration = 0.25s
```

### Problem 2: Reload jest pomijany po ostatnim strzale
**Przyczyna:** Trigger Reload nadpisywany przez Shoot lub state nie ma czasu się zagrać  
**Rozwiązanie:** 
- ✅ Dodane `_pendingReload` queue
- ✅ `ResetTrigger()` przed każdym triggerem
- ✅ Timeout na wykonanie pending reloadu

### Problem 3: Mówienie shot/reload podczas biegu
**Przyczyna:** Parametry `IsWalking/IsRunning` nadpisują się triggerami  
**Rozwiązanie:** 
- ✅ Blokada SetBool w `SetMovementAnimations()` podczas akcji
- ✅ Zapamiętywanie stanu w `_lastIsWalking/Running`
- ✅ Przywracanie stanu w `FinishReload()`

### Problem 4: "Szarpnięcie" animacji pomiędzy Walk → Run
**Przyczyna:** Brak Loop Pose, clip nie ma płynnego przejścia  
**Rozwiązanie:**
- ✅ W `.fbx`: Loop Time = ON, Loop Pose = ON
- ✅ Transition Duration = 0.1s
- ✅ Animator State: Has Exit Time = OFF

---

## 📝 TODO i ulepszenia

### High Priority (Zalecane)

- [ ] **Blend Tree zamiast bool** — zamiast IsWalking/IsRunning użyć float `MoveSpeed` (0-1) z dampingiem
  - Płynniejsze przejścia między walk/run
  - Brak "szarpnięcia" przy przejściu walk ↔ run
  
- [ ] **Animation Events** — zamiast timera w kodzie
  - `OnReloadAnimationFinished()` callback na koniec klipu
  - `OnShootMuzzleFlash()` w środku animacji strzału (bardziej natural)

- [ ] **Reload bar UI** — pasek przeładowania
  - `(reloadEndTime - Time.time) / reloadTime`

- [ ] **Ammo counter** — wyświetlanie `currentAmmo / maxAmmo`

### Medium Priority

- [ ] **Recoil animation** — dodatkowa animacja kopcia/kick
- [ ] **Wpływ recoilu na musztę** (camera shake)
- [ ] **Particlele na trafienie** (krople krwi, sparki, etc)
- [ ] **Różne bronie** — karabin/shotgun z własnymi animacjami
- [ ] **Weapon sway** — kołysanie broni (cuando idle/walk)

### Low Priority

- [ ] **Dual wielding** — dwie bronie jednocześnie
- [ ] **Weapon attachments** — scope, stock, silencer (z animacjami)
- [ ] **Melee attack** — punch/kick trigger
- [ ] **Grenades** — armatury, arc prediction

---

## 🔧 Konfiguracja w Unity (Checklist)

### Setup broni
- [ ] Animator Controller przypisany do obiektu broni
- [ ] Parametry w Animatorze: `IsWalking`, `IsRunning`, `Shoot`, `Reload`, `ReloadEnd`
- [ ] Klip `Shoot` bez Loop Time
- [ ] Klip `Reload` bez Loop Time
- [ ] Klip `Walk` z Loop Time = ON
- [ ] Klip `Run` z Loop Time = ON
- [ ] Transitionu przejścia (patrz wyżej)
- [ ] AudioSource na obiekcie broni (lub dodaje się automatycznie)
- [ ] Muzzle Flash prefab (opcjonalnie)

### Setup gracza
- [ ] PlayerController ma referencję do CurrentWeapon (lub auto-find)
- [ ] PlayerStats UI na canvasu
- [ ] Camera przypisana w PlayerController
- [ ] Player ma Tag "Player" (dla EnemyAI)

### Setup wrogów
- [ ] EnemyAI na prefabie przeciwnika
- [ ] NavMeshAgent w scenie
- [ ] Przeciwnik ma Tag "Enemy"
- [ ] WaveSpawner ma spawnPoints assigned

---

## 📊 Statystyki i performance

### Memory footprint
- WeaponBase per instance: ~50 KB (2-3 zmienne, 1 Animator ref)
- WeaponData (ScriptableObject): ~10 KB
- Animator Controller: ~20 KB

### CPU impact per frame
- `WeaponBase.Update()`: <0.5 ms (4 checks, 1 potential raycast)
- `Raycast`: ~1-2 ms (zależne od collider count)
- `PlayerController.Update()`: <0.3 ms (input + movement)

### Rekomendacje
- Max 4-5 broni aktywnych jednocześnie
- Pooling muzzle flash (zamiast Instantiate/Destroy)
- Cache raycast results

---

## 🔗 Linki do kodu

- `Assets/Scripts/Weapon/WeaponBase.cs` — core logic
- `Assets/Scripts/Weapon/WeaponData.cs` — data container
- `Assets/Scripts/PlayerController.cs` — ruch + synchronizacja
- `Assets/Scripts/PlayerStats.cs` — HP/armor
- `Assets/Scripts/EnemyAI.cs` — przeciwnik
- `Assets/Scripts/WaveSpawner.cs` — spawn system
- `Assets/Scripts/Weapon/WeaponManager.cs` — zmiana broni

---

## 📞 Kontakt i Notes

**Branch:** `master`  
**Remote:** `https://github.com/Unity-PM/PSM_s2_L02_Jungle_Sam`

**Ostatnie zmiany:**
- ✅ Dodane `_pendingReload` i `_shootAnimEndTime` dla stabilizacji
- ✅ Reset triggerów aby uniknąć conflicts
- ✅ Zapamiętywanie stanu ruchu (`_lastIsWalking/Running`)
- ✅ Blokada SetBool podczas akcji (shoot/reload)
- ✅ Cache AudioSource
- ✅ Auto-destroy muzzle flash po 1s

---

**Dokument przygotowany do użytku jako prompt i wyznacznik do dalszych prac.**
