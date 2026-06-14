# 🏗️ Architektura Systemu - Diagram i Przepływy

---

## 📊 Schemat hierarchii sceny

```
Scene: Main Game
│
├── Player (Capsule + CharacterController)
│   ├── Camera (Main Camera)
│   │   └── [FPS View - 90° rotation]
│   │
│   ├── WeaponManager (Component)
│   │   └── weaponSlots[] = {Slot0, Slot1, Slot2}
│   │
│   ├── PlayerController (Component)
│   │   ├── currentWeapon = Slot0 (aktywna broń)
│   │   └── Synchronizacja ruchu ↔ bronia
│   │
│   └── PlayerStats (Component)
│       ├── maxHealth: 100
│       ├── armor: 0
│       └── UI refs (HP/Armor text)
│
├── Weapon Slots (Child GameObjects)
│   ├── Pistolet (Slot 0) [ACTIVE]
│   │   ├── Model 3D (Glock mesh)
│   │   ├── Animator (WeaponAnimator Controller)
│   │   ├── AudioSource (shoot sounds)
│   │   ├── WeaponBase (Component)
│   │   └── WeaponData (reference asset)
│   │
│   ├── Karabin (Slot 1) [INACTIVE]
│   └── Shotgun (Slot 2) [INACTIVE]
│
├── Spawner (GameObject)
│   └── WaveSpawner (Component)
│       ├── waves[] = {...}
│       ├── spawnPoints[] = {Point1, Point2, Point3}
│       └── waveText (UI reference)
│
├── NavMesh (Baked)
│   └── Walkable surface dla EnemyAI
│
├── Enemies (Spawned dynamicznie)
│   ├── Enemy 1
│   │   ├── NavMeshAgent
│   │   ├── EnemyAI (Component)
│   │   └── Collider (dla raycast)
│   │
│   └── Enemy 2, 3, ... (z falami)
│
└── Canvas (UI)
    ├── Health Text (HP: 100)
    ├── Armor Text (Armor: 0)
    ├── Wave Text (Wave: 1)
    └── Crosshair (?)
```

---

## 🔄 Data Flow Diagram

### A. Input & Movement

```
┌─────────────────────┐
│   User Input        │
│ (Keyboard + Mouse)  │
└──────────┬──────────┘
           │
           ▼
┌──────────────────────────────┐
│ PlayerController.ReadInput() │
├──────────────────────────────┤
│ _moveInput (WASD)            │
│ _lookInput (Mouse delta)     │
│ _sprintPressed (Shift)       │
│ _jumpPressed (Space)         │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────────┐
│ PlayerController.HandleMovement()│
├──────────────────────────────────┤
│ CharacterController.Move()       │
│ Apply gravity                    │
│ Update velocity                  │
└──────────┬──────────────────────┘
           │
           ▼
┌──────────────────────────────────┐
│ PlayerController.HandleLook()    │
├──────────────────────────────────┤
│ Update camera rotation           │
│ Clamp X rotation (-90 to 90)     │
└──────────┬──────────────────────┘
           │
           ▼
┌─────────────────────────────────────────┐
│ PlayerController.UpdateWeaponAnimations()
├─────────────────────────────────────────┤
│ _moveInput.magnitude > 0.1 ?             │
│   → isMoving = true                      │
│ isMoving && _sprintPressed ?             │
│   → isSprinting = true                   │
│                                          │
│ WeaponBase.SetMovementAnimations(...)   │
│   → SetBool(IsWalking, isMoving)         │
│   → SetBool(IsRunning, isSprinting)      │
└──────────────────────────────────────────┘
```

### B. Shooting System

```
┌─────────────────────────────┐
│ LMB pressed?                │
│ (LPM wciśnięty)             │
└──────────┬──────────────────┘
           │
           ▼
┌────────────────────────────────────┐
│ WeaponBase.Update()                │
├────────────────────────────────────┤
│ Check:                             │
│ - if (_isReloading) { return; }   │
│ - if (!Mouse.current) { return; }  │
│                                    │
│ shootInput = isAutomatic ?         │
│   ? leftButton.isPressed           │ (trzymaj = burst)
│   : leftButton.wasPressedThisFrame │ (kliknij = single)
│                                    │
│ if (shootInput) TryShoot()         │
└──────────┬───────────────────────┘
           │
           ▼
┌────────────────────────────────────┐
│ WeaponBase.TryShoot()              │
├────────────────────────────────────┤
│ if (Time.time < _nextFireTime)     │
│   { return; } ← cooldown blokuje   │
│                                    │
│ if (_currentAmmo <= 0)             │
│   { Reload(); return; }            │
│                                    │
│ if (_isReloading)                  │
│   { return; } ← reload blokuje     │
│                                    │
│ ✓ Shoot()                          │
│ _nextFireTime = Time.time + CDN    │
└──────────┬───────────────────────┘
           │
           ▼
┌────────────────────────────────────────┐
│ WeaponBase.Shoot()                     │
├────────────────────────────────────────┤
│ 1. _currentAmmo--                      │
│                                        │
│ 2. _shootAnimEndTime = now + 0.25s    │
│                                        │
│ 3. Animator.SetTrigger("Shoot")        │
│    ├─ State machine: Any → Shot        │
│    └─ Animace se zahraje (0.25s)       │
│                                        │
│ 4. PlayOneShot(shootSound)             │
│                                        │
│ 5. Instantiate(muzzleFlash)            │
│    └─ Destroy po 1s                    │
│                                        │
│ 6. Ray ray = camera.ViewportToRay()    │
│    Physics.Raycast(ray, out hit)       │
│    ├─ if (hit.tag == "Enemy")          │
│    │  └─ enemy.TakeDamage(damage)      │
│    └─ Obrażenia trafiają wrogom        │
│                                        │
│ 7. if (_currentAmmo == 0)              │
│    └─ _pendingReload = true            │
│       (reload będzie po animacji)      │
└────────────────────────────────────────┘
           │
           ▼
┌────────────────────────────────────┐
│ Animator State Machine (parallel)  │
├────────────────────────────────────┤
│ SetBool(IsWalking, false)          │
│ SetBool(IsRunning, false)          │ ← blokada ruchu
│                                    │
│ Animacja Shot (0.25s):             │
│   0-100ms: Ręka przychodzi         │
│   100-150ms: Flash (ostrzelec)     │
│   150-250ms: Cofnięcie               │
│                                    │
│ Po Exit Time (0.95):               │
│   → return to Walk/Run/Idle        │
│   → SetBool(IsWalking, restored)   │
│   → SetBool(IsRunning, restored)   │
└────────────────────────────────────┘
```

### C. Reload System

```
┌──────────────────────┐
│ R wciśnięty?         │
│ (_currentAmmo < max) │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────────────────┐
│ WeaponBase.Update()              │
├──────────────────────────────────┤
│ if (Keyboard.rKey.wasPressedThisFrame)
│   && !_isReloading               │
│   && _currentAmmo < maxAmmo       │
│   Reload()                        │
└──────────┬──────────────────────┘
           │
    ┌──────┴──────┐
    │             │
    ▼ (R-click)   ▼ (auto, ammo=0)
┌──────────────┐ ┌───────────────────┐
│ Manual Reload│ │ Pending Reload    │
└──────────┬──┘ │ (queued auto)      │
           │    │                    │
           │    └───────────┬────────┘
           │                │
           └────────┬───────┘
                    │
                    ▼
        ┌──────────────────────────────────┐
        │ WeaponBase.Reload()              │
        ├──────────────────────────────────┤
        │ if (_isReloading) { return; }   │
        │                                  │
        │ _isReloading = true              │
        │ _reloadEndTime = now + 1.5s      │
        │ _nextFireTime = _reloadEndTime   │ ← blokada strzałów
        │                                  │
        │ ResetTrigger("Shoot")            │ ← clear shoot trigger
        │ SetTrigger("Reload")             │ ← play reload anim
        │                                  │
        │ _pendingReload = false           │ (jeśli auto)
        └───────────┬──────────────────────┘
                    │
        1.5 sekundy czekania...
                    │
                    ▼
        ┌──────────────────────────────────┐
        │ Update() - check timer           │
        ├──────────────────────────────────┤
        │ if (_isReloading &&              │
        │     Time.time >= _reloadEndTime) │
        │   FinishReload()                 │
        └───────────┬──────────────────────┘
                    │
                    ▼
        ┌──────────────────────────────────┐
        │ WeaponBase.FinishReload()        │
        ├──────────────────────────────────┤
        │ _isReloading = false             │
        │ _currentAmmo = maxAmmo           │
        │                                  │
        │ SetTrigger("ReloadEnd")          │
        │                                  │
        │ SetBool(IsWalking, _lastIsWalking)
        │ SetBool(IsRunning, _lastIsRunning)│
        │   ← restore movement state       │
        └──────────────────────────────────┘
                    │
                    ▼
        ┌──────────────────────────────────┐
        │ Animator returns to previous     │
        │ state (Walk/Run/Idle)            │
        │ based on restored bools          │
        └──────────────────────────────────┘
```

---

## 🎬 Animator State Machine (Detailed)

```
                    ┌─────────────────────────────┐
                    │ Entry Point                 │
                    └──────────────┬──────────────┘
                                   │
                                   ▼
                    ┌──────────────────────────────┐
                    │ Hands|Origin (Idle)          │
                    │ (brak parametrów)            │
                    └──────────┬──────────────────┘
                               │
                ┌──────────────┴──────────────┐
                │                             │
          IsWalking=true              IsWalking=false
                │                             │
                ▼                             │
    ┌──────────────────────┐                 │
    │ Hands|Walk           │◄────────────────┘
    │ (Loop Time: ON)       │
    └──┬────────────────────┘
       │
       │ IsRunning=true
       ▼
    ┌──────────────────────┐
    │ Hands|Run            │
    │ (Loop Time: ON)       │
    └──┬────────────────────┘
       │
       │ IsRunning=false && IsWalking=true
       │
       ├──────────────────────────────────────┐
       │                                      │
       ▼                                      │
    ┌────────────────────────────────────────┐│
    │ ← Hands|Walk (wraca)                   ││
    └────────────────────────────────────────┘│
                                              │
                    ┌─────────────────────────┘
                    │
                    ▼
    ┌────────────────────────────────────────┐
    │ IsWalking=false &&                     │
    │ IsRunning=false → Origin               │
    └────────────────────────────────────────┘


┌────────────────────────────────────────────────────────┐
│                    ACTION STATES                        │
├────────────────────────────────────────────────────────┤
│                                                        │
│  Any State ──Trigger(Shoot)──→ Hands|Shot             │
│                              │                        │
│                              │ (Animation ends)      │
│                              │ Exit Time: 0.95        │
│                              │                        │
│        ┌─────────────────────┼─────────────────────┐  │
│        │                     │                     │  │
│   IsRunning=T           IsWalking=T        Both=F   │  │
│        │                     │                     │  │
│        ▼                     ▼                     ▼  │
│   Hands|Run          Hands|Walk              Origin   │
│                                                        │
│  ─────────────────────────────────────────────────────│
│                                                        │
│  Any State ──Trigger(Reload)──→ Hands|Reload_t      │
│                              │                        │
│                              │ (Timer: 1.5s)        │
│                              │                        │
│        ┌─────────────────────┼─────────────────────┐  │
│        │                     │                     │  │
│   IsRunning=T           IsWalking=T        Both=F   │  │
│        │                     │                     │  │
│        ▼                     ▼                     ▼  │
│   Hands|Run          Hands|Walk              Origin   │
│                                                        │
│  Trigger(ReloadEnd) fires when FinishReload() called  │
│                                                        │
└────────────────────────────────────────────────────────┘
```

---

## 💾 Data Structure (Runtime)

```
┌──────────────────────────────────────┐
│ WeaponBase Instance                  │
├──────────────────────────────────────┤
│ weaponData: WeaponData (ref)          │
│   ├─ weaponName: "Pistolet"           │
│   ├─ damage: 10                       │
│   ├─ fireRate: 0.25                   │
│   ├─ shootAnimationDuration: 0.25     │
│   ├─ range: 100                       │
│   ├─ maxAmmo: 30                      │
│   ├─ reloadTime: 1.5                  │
│   ├─ isAutomatic: false               │
│   ├─ shootSound: AudioClip            │
│   └─ muzzleFlashPrefab: GameObject    │
│                                       │
│ State:                                │
│   ├─ _currentAmmo: 25 (int)           │
│   ├─ _nextFireTime: 1245.3 (float)    │
│   ├─ _isReloading: false (bool)       │
│   ├─ _reloadEndTime: 0.0 (float)      │
│   ├─ _shootAnimEndTime: 1245.2 (float)│
│   ├─ _pendingReload: false (bool)     │
│   ├─ _lastIsWalking: true (bool)      │
│   ├─ _lastIsRunning: false (bool)     │
│   ├─ _weaponAnimator: Animator (ref)  │
│   ├─ _audioSource: AudioSource (ref)  │
│   └─ _mainCam: Camera (ref)           │
│                                       │
│ Static Hash Values:                   │
│   ├─ ShootHash: 123456 (int)          │
│   ├─ ReloadHash: 123457 (int)         │
│   ├─ ReloadEndHash: 123458 (int)      │
│   ├─ IsWalkingHash: 123459 (int)      │
│   └─ IsRunningHash: 123460 (int)      │
└──────────────────────────────────────┘
```

---

## 🎲 State Machine Transition Conditions

| Source | Destination | Condition | Has Exit Time | Priority |
|--------|-------------|-----------|---------------|----------|
| Entry | Origin | (auto) | N/A | 0 |
| Origin | Walk | IsWalking == true | ON (0.9) | 1 |
| Walk | Run | IsRunning == true | ON (0.9) | 2 |
| Run | Walk | IsRunning == false && IsWalking == true | ON (0.9) | 3 |
| Walk | Origin | IsWalking == false && IsRunning == false | ON (0.9) | 4 |
| Run | Origin | IsRunning == false && IsWalking == false | ON (0.9) | 5 |
| **Any State** | **Shot** | **Shoot == true** | **OFF** | **10** |
| Shot | Run | IsRunning == true | ON (0.95) | 11 |
| Shot | Walk | IsWalking == true && IsRunning == false | ON (0.95) | 12 |
| Shot | Origin | IsWalking == false && IsRunning == false | ON (0.95) | 13 |
| **Any State** | **Reload** | **Reload == true** | **OFF** | **20** |
| Reload | Run | IsRunning == true | ON (0.95) | 21 |
| Reload | Walk | IsWalking == true && IsRunning == false | ON (0.95) | 22 |
| Reload | Origin | IsWalking == false && IsRunning == false | ON (0.95) | 23 |

---

## 🔗 Dependencies & Connections

```
WeaponManager
  ├─ spawns WeaponBase instances
  └─ enables/disables by index (1-2-3 keys)

PlayerController
  ├─ reads Input (WASD, Mouse, Shift, R, LMB)
  ├─ calls WeaponBase.SetMovementAnimations()
  └─ maintains currentWeapon reference

WeaponBase
  ├─ accesses WeaponData (stats)
  ├─ controls Animator (local)
  ├─ plays AudioSource
  ├─ raycasts for enemies
  ├─ calls EnemyAI.TakeDamage()
  └─ instantiates muzzle flash

EnemyAI
  ├─ uses NavMeshAgent (navigation)
  ├─ finds player by tag
  ├─ calls PlayerStats.TakeDamage()
  └─ calls WaveSpawner.AddCoins() (if dies)

WaveSpawner
  ├─ spawns enemies at spawnPoints
  ├─ uses waves configuration
  └─ updates waveText UI

PlayerStats
  ├─ tracks health & armor
  └─ updates UI (healthText, armorText)
```

---

**Diagram przygotowany do analizy i programowania na bazie już istniejącej struktury.**
