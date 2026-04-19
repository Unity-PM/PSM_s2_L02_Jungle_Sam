# ⚡ Quick Reference - System Broni

## 🎯 Status: ✅ Funkcjonalny

---

## 🕹️ Sterowanie gracza

| Klawisz | Akcja |
|---------|-------|
| `W/A/S/D` | Ruch |
| `Left Shift` | Sprint |
| `Myszka` | Patrzenie |
| `LPM` | Strzał |
| `R` | Przeładowanie |
| `1/2/3` | Zmiana broni |

---

## ⚙️ Szybka konfiguracja w Unity

### 1. Animator Controller Setup

**Parametry (kliknij + w lewym panelu):**
- `IsWalking` (Bool)
- `IsRunning` (Bool)
- `Shoot` (Trigger)
- `Reload` (Trigger)
- `ReloadEnd` (Trigger)

**Stany (PPM → Create State):**
- `Entry` → `Hands|Origin` (entry state)
- `Hands|Origin` (idle)
- `Hands|Walk`
- `Hands|Run`
- `Hands|Shot`
- `Hands|Reload_t`

### 2. Tranzycje (Transition Settings)

#### Lokomocja (Origin → Walk → Run)
```
Origin → Walk
  Condition: IsWalking == true
  Has Exit Time: ON (0.9)
  Transition Duration: 0.1
  
Walk → Run
  Condition: IsRunning == true
  
Run → Walk
  Condition: IsRunning == false && IsWalking == true
  
Walk → Origin
  Condition: IsWalking == false && IsRunning == false
  
Run → Origin
  Condition: IsRunning == false && IsWalking == false
```

#### Akcje (Any State → Shot/Reload)
```
Any State → Hands|Shot
  Condition: Shoot == true
  Has Exit Time: OFF
  Transition Duration: 0.05
  Interruption Source: NONE
  
Hands|Shot → Hands|Run
  Condition: IsRunning == true
  Exit Time: 0.95
  Transition Duration: 0.1
  
Hands|Shot → Hands|Walk
  Condition: IsWalking == true && IsRunning == false
  
Hands|Shot → Origin
  Condition: IsWalking == false && IsRunning == false
  
[To samo dla Reload]
```

### 3. Animator Import Settings (dla klipów)

**Idle/Walk/Run (looping):**
```
Loop Time: ON
Loop Pose: ON
Cycle Offset: 0.0 (lub 0.05-0.1 do testowania)
```

**Shoot/Reload (single):**
```
Loop Time: OFF
```

### 4. WeaponData Ustawienia

**Dla pistoletu:**
```
Weapon Name: Pistolet
Damage: 10
Fire Rate: 0.25s
Shoot Animation Duration: 0.25s
Range: 100
Max Ammo: 30
Reload Time: 1.5s
Is Automatic: false (semi-auto)
```

**Dla karabinu (auto):**
```
Fire Rate: 0.12s
Shoot Animation Duration: 0.12s
Is Automatic: true
Max Ammo: 45
Reload Time: 2.0s
```

**Dla strzelby:**
```
Fire Rate: 1.0s
Shoot Animation Duration: 0.8s
Max Ammo: 8
Reload Time: 3.0s
Is Automatic: false
```

---

## 🔍 Debugowanie

### Logowanie
```csharp
// W WeaponBase się debuguje (print na console):
Debug.Log("Strzał...") // W Shoot()
Debug.Log("Przeładowanie...") // W Reload()
Debug.Log("Przeładowanie ukończone...") // W FinishReload()
```

### Sprawdzenie
1. **Console → kliknij Filter → szukaj "Strzał"**
   - Jeśli widzisz = raycast i ammo poprawnie
   
2. **Animator window → preview state machine**
   - Kliknij play i obserwuj przejścia między stanami
   
3. **Animator debugger** (window)
   - Parameters panel — obserwuj bools/triggers w real-time
   
4. **Weapon properties (Inspector)**
   - Sprawdź `_currentAmmo` i `_isReloading` live

---

## 🚨 Typowe problemy

| Problem | Przyczyna | Rozwiązanie |
|---------|-----------|------------|
| Animacje się "ucinal" | Transition Duration za mały | ↑ do 0.1 |
| Shot animation não jest widoczna | fireRate > shootAnimDuration | Wyrównaj wartości (0.25s) |
| Strzał w trakcie reloadu | _nextFireTime nie blokuje | ✅ Już naprawione w kodzie |
| Reload nie wraca do biegu | _lastIsRunning nie zapamiętany | ✅ Już naprawione w kodzie |
| Brak dźwięku | AudioSource brakuje lub clip pusty | Dodaj AudioSource + AudioClip |
| Muzzle flash nie widoczny | Prefab nie ma Renderer | Upewnij się że ma Sprite/Mesh |

---

## 📈 Optymalizacje do zrobienia (Priority)

### 1. Blend Tree (łatwe, dużo help)
```csharp
// Zamiast booli IsWalking/IsRunning
float moveSpeed = 0f;  // 0-1 range
if (isWalking && !isSprinting) moveSpeed = 0.5f;
if (isSprinting) moveSpeed = 1.0f;

animator.SetFloat("MoveSpeed", Mathf.Lerp(moveSpeed, targetSpeed, Time.deltaTime * 5f)); // damping
```

### 2. Animation Events
```csharp
// Na koniec klipu Reload, zamiast timera:
// Animator → Hands|Reload_t → dodaj Event na 1.0 (end)
// → GameObject → WeaponBase.OnReloadAnimationFinished()
```

### 3. Pooling Muzzle Flash
```csharp
// Zamiast Instantiate + Destroy 1s
// Użyj Object Pool dla muzzle flashów
private Queue<GameObject> _muzzleFlashPool;
```

---

## 📚 Referencje

- **Unity New Input System:** Input.GetKey() → Keyboard.current.key
- **NavMesh:** Enemies muszą być na baked NavMesh (Bake → Window)
- **Raycast:** Z kamery gracz → hitscan (bezniego latencji)
- **Animator Parameters:** Hashe są szybsze niż stringi
- **Triggers:** ResetTrigger() żeby nie zostały "stuck" na false

---

## 🎨 Artystyczne notatki

- Animacje rąk/pistoletu powinny być na **podobnych kluczowych klatkach** (sync)
- **Muzzle flash** najlepiej na frame 3-5 animacji strzału (realistycznie)
- **Reload** powinien mieć wyraźną fazę:
  - 0-30%: wyciągnięcie magazynka
  - 30-70%: wrzucenie i załadowanie nowego
  - 70-100%: zablokowanie i powrót do idle
- **Walk cycle** musi być na ~1.2 sekundy (natural cadence)
- **Run cycle** ~0.6 sekundy

---

## ✅ Checklist przed deployem

- [ ] Wszystkie bronie mają WeaponData ScriptableObject
- [ ] Animator Controller ma wszystkie parametry
- [ ] Transitionu są poprawne (czy testowałeś każde?)
- [ ] Loop Time: Walk/Run = ON, Shot/Reload = OFF
- [ ] Audio clips przypisane
- [ ] Muzzle flash prefab (opcjonalnie) lub disabled
- [ ] Enemy AI ma nawigację (NavMesh)
- [ ] Player ma Tag "Player"
- [ ] WaveSpawner ma spawn points
- [ ] UI canvasu ma TextMeshPro dla HP/Ammo
- [ ] Game builds bez błędów

---

**Last update:** 2025  
**Prepared for:** Continuation & Team reference
