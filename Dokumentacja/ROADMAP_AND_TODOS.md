# 🚀 Roadmap & Future Development

**Status:** Aktywnie rozwijany  
**Ostatnia aktualizacja:** 2025

---

## 📋 Wykonane (✅ DONE)

### Core Weapon System v1.0
- ✅ WeaponBase — logika strzału i przeładowania
- ✅ WeaponData — konfiguracja broni (ScriptableObject)
- ✅ Semi-auto & auto-fire support
- ✅ Raycast hit detection
- ✅ Animator synchronizacja

### Animation System v2.0
- ✅ State machine (Idle → Walk → Run)
- ✅ Shoot/Reload action states
- ✅ Movement blending z triggerami
- ✅ Blokada strzału podczas przeładowania
- ✅ Blokada ruchu podczas akcji

### Stabilizacja & Bug fixes
- ✅ Pending reload queue (po ostatnim strzale)
- ✅ Trigger reset (avoid Shoot/Reload conflicts)
- ✅ State preservation (_lastIsWalking/Running)
- ✅ Blokada SetBool podczas akcji
- ✅ Audio caching
- ✅ Muzzle flash auto-cleanup

### Player & Enemy Systems
- ✅ PlayerController (ruch, patrzenie)
- ✅ PlayerStats (HP, armor, UI)
- ✅ EnemyAI (NavMesh-based)
- ✅ WaveSpawner (fale przeciwników)
- ✅ WeaponManager (zmiana broni 1-2-3)

---

## 🔨 W trakcie (🟡 IN PROGRESS)

### Optymalizacja animacji
- 🟡 Loop Pose ustawienia
- 🟡 Blend Tree zamiast booli
- 🟡 Animation Events (zamiast timera)
- **ETA:** do konca tygodnia

### UI Improvements
- 🟡 Ammo counter (currentAmmo / maxAmmo)
- 🟡 Reload bar (progress)
- **ETA:** następny sprint

---

## 📅 Planowane (❌ TODO)

### Faza 1: Polishing (Tydzień 1-2)

#### 1.1 Animator Optimization
```csharp
// TODO: Zamiast bool → Blend Tree
public float moveSpeed = 0f; // 0-1 range
// Walk = 0.5, Run = 1.0, Idle = 0.0

// W Animator:
// - Create 1D Blend Tree "MoveSpeed"
// - Idle (0.0) ↔ Walk (0.5) ↔ Run (1.0)
// - Transition Duration: 0.15s (smooth blend)
```

**Korzyści:**
- Brak "szarpnięcia" przy walk → run
- Płynne przejścia
- Naturalniejszy ruch

**Czasochłonność:** ~3h (animator setup + testing)

#### 1.2 Animation Events
```csharp
// TODO: Zamiast Time.time >= _reloadEndTime
// Używać callback z animacji

// W Animator:
// - Hands|Reload_t → dodaj Event na 1.0
// - Method: OnReloadAnimationFinished()

// W WeaponBase:
public void OnReloadAnimationFinished()
{
    // Gdy Animator trigger event fires
    FinishReload();
}
```

**Korzyści:**
- Precyzyjne timing (100% zgoda z klipu)
- Brak desyncu z timerem
- Łatwiej testować w Animator preview

**Czasochłonność:** ~2h

#### 1.3 Recoil & Camera Shake
```csharp
// TODO: Na koniec animacji Shoot, dodać recoil
// - Perturbacja kamery (0.1-0.2s)
// - Powrót do pozycji (smooth return)

// Pseudocode:
void Shoot()
{
    // ... existing code ...
    ApplyCameraShake(shakeDuration: 0.15f, magnitude: 0.3f);
}

private void ApplyCameraShake(float duration, float magnitude)
{
    StartCoroutine(ShakeCoroutine(duration, magnitude));
}

IEnumerator ShakeCoroutine(float duration, float mag)
{
    Vector3 originalPos = camera.localPosition;
    float elapsed = 0f;
    
    while (elapsed < duration)
    {
        float randomX = Random.Range(-mag, mag);
        float randomY = Random.Range(-mag, mag);
        camera.localPosition = originalPos + new Vector3(randomX, randomY, 0);
        
        elapsed += Time.deltaTime;
        yield return null;
    }
    
    camera.localPosition = originalPos;
}
```

**Czasochłonność:** ~4h (tweaking feel)

---

### Faza 2: Content Expansion (Tydzień 3-4)

#### 2.1 Weapon Variety
```csharp
// TODO: Dodać więcej broni z własnymi animacjami

Pistolet (DONE)
  - damage: 10
  - fireRate: 0.25
  - ammo: 30
  
Karabin (TODO)
  - damage: 20
  - fireRate: 0.12 (auto)
  - ammo: 45
  - reloadTime: 2.0
  
Shotgun (TODO)
  - damage: 35 (dmg multiplier per pellet)
  - fireRate: 1.0
  - ammo: 8
  - reloadTime: 3.0
  - special: MultiRaycast (5-8 rays w spread)

Sniper (TODO)
  - damage: 50
  - fireRate: 1.5
  - ammo: 5
  - special: Zoom view
```

**Każda broń potrzebuje:**
- WeaponData asset
- Animator clips (idle, walk, run, shoot, reload)
- Audio (shoot, reload, cock)
- Model 3D / prefab

**Czasochłonność:** ~20h (art assets + setup)

#### 2.2 Weapon Attachment System
```csharp
// TODO: Scope, Silencer, Extended Magazine
public class WeaponAttachment
{
    public string attachmentName;
    public float damageMultiplier = 1.0f;
    public float fireRateMultiplier = 1.0f;
    public float ammoMultiplier = 1.0f;
    public GameObject visualPrefab;
}

// W WeaponBase:
public WeaponAttachment[] attachments;

public float GetFinalDamage()
{
    float dmg = weaponData.damage;
    foreach (var att in attachments)
        dmg *= att.damageMultiplier;
    return dmg;
}
```

**Czasochłonność:** ~8h

---

### Faza 3: Advanced Combat (Tydzień 5-6)

#### 3.1 Melee Attack System
```csharp
// TODO: Punch/kick attack

public class MeleeAttack : MonoBehaviour
{
    public float meleeDamage = 5;
    public float meleeRange = 2;
    public float meleeCooldown = 0.5f;
    
    private float _nextMeleeTime = 0f;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && Time.time >= _nextMeleeTime)
        {
            Melee();
            _nextMeleeTime = Time.time + meleeCooldown;
        }
    }
    
    void Melee()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, meleeRange);
        foreach (var hit in hits)
        {
            EnemyAI enemy = hit.GetComponent<EnemyAI>();
            if (enemy != null)
                enemy.TakeDamage(meleeDamage);
        }
        
        // Trigger melee animation
        animator.SetTrigger("Melee");
    }
}
```

**Czasochłonność:** ~6h

#### 3.2 Grenade System
```csharp
// TODO: G-Key to throw grenades

public class GrenadeProjectile : MonoBehaviour
{
    public float damage = 30;
    public float explosionRadius = 5;
    public float explosionDelay = 3;
    
    public void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            EnemyAI enemy = hit.GetComponent<EnemyAI>();
            if (enemy != null)
                enemy.TakeDamage(damage);
        }
        
        Destroy(gameObject);
    }
}

// W WeaponBase:
void ThrowGrenade()
{
    GameObject grenade = Instantiate(grenadePrefab, camera.position, Quaternion.identity);
    Rigidbody rb = grenade.GetComponent<Rigidbody>();
    rb.velocity = camera.forward * 20; // throw force
    
    GrenadeProjectile proj = grenade.GetComponent<GrenadeProjectile>();
    Invoke(nameof(proj.Explode), 3f);
}
```

**Czasochłonność:** ~8h

---

### Faza 4: Advanced Features (Tydzień 7-8)

#### 4.1 Headshot Detection
```csharp
// TODO: Zwiększone obrażenia za headshot

if (Physics.Raycast(ray, out RaycastHit hit, range))
{
    float damageMultiplier = 1.0f;
    
    if (hit.collider.CompareTag("Head"))
        damageMultiplier = 2.0f; // 2x damage
    
    EnemyAI enemy = hit.transform.GetComponent<EnemyAI>();
    if (enemy != null)
        enemy.TakeDamage(weaponData.damage * damageMultiplier);
    
    // Particle effect
    if (hit.collider.CompareTag("Head"))
        Instantiate(headShotEffect, hit.point, Quaternion.identity);
}
```

**Czasochłonność:** ~4h

#### 4.2 Weapon Spread & Recoil Pattern
```csharp
// TODO: Bullet spread (niż dokładny raycast)

void Shoot()
{
    // Oblicz spread angle
    float spreadAngle = isMoving ? 0.1f : 0.05f;
    float randomX = Random.Range(-spreadAngle, spreadAngle);
    float randomY = Random.Range(-spreadAngle, spreadAngle);
    
    // Zamiast camera.forward → camera.forward + spread
    Vector3 spreadDir = camera.forward + 
                        camera.right * randomX + 
                        camera.up * randomY;
    
    Ray spreadRay = new Ray(camera.position, spreadDir);
    Physics.Raycast(spreadRay, ...);
}
```

**Czasochłonność:** ~3h

#### 4.3 Armor Degradation
```csharp
// TODO: Pancerz sich zunutze macht

public void TakeDamage(float amount)
{
    if (armor > 0)
    {
        float blocked = Mathf.Min(armor, amount * 0.8f); // block 80%
        armor -= blocked;
        amount -= blocked;
    }
    
    currentHealth -= amount;
    
    if (currentHealth <= 0)
        Die();
    
    UpdateUI();
}
```

**Czasochłonność:** ~2h

---

## 🎯 Prioritetyzacja

```
Priority Level:

HIGH (Robi się gra lepsze)
├─ 1. Blend Tree animation (fluidy)
├─ 2. Animation Events (precyzja)
├─ 3. Recoil + camera shake (feel)
├─ 4. Ammo counter UI (usability)
└─ 5. Reload bar UI (feedback)

MEDIUM (Nice to have)
├─ 1. Weapon variety (content)
├─ 2. Headshot detection (gameplay depth)
├─ 3. Attachment system (customization)
└─ 4. Weapon spread (realism)

LOW (Spit and Polish)
├─ 1. Melee attack (option)
├─ 2. Grenades (variety)
├─ 3. Armor system (balance)
└─ 4. Dual wielding (fancy)
```

---

## 📊 Estimated Timeline

```
Week 1-2: Core Optimization
  └─ ~9h (Blend Tree, Events, Recoil)
  
Week 3-4: Content & Features
  └─ ~28h (Weapons, Attachments, UI)
  
Week 5-6: Advanced Combat
  └─ ~14h (Melee, Grenades)
  
Week 7-8: Polish
  └─ ~13h (Headshots, Spread, Armor)

TOTAL: ~64h (~2 weeks @ 8h/day, or 4 weeks @ 4h/day)
```

---

## 📌 Known Technical Debt

- [ ] Raycast should use Physics.raycastAll for multiple hits
- [ ] Audio pooling (zamiast PlayOneShot per shot)
- [ ] Muzzle flash pooling (zamiast Instantiate)
- [ ] Cache enemy list (zamiast FindGameObjectWithTag co spawn)
- [ ] Async weapon loading (zamiast Resources.Load)
- [ ] Settings menu (volume, sensitivity, graphics)

---

## 🔐 Bugs & Fixes

### Pending Bugs
- [ ] Reload timeout nie da się uszkodził jeśli gracz umrze mid-reload
- [ ] Enemy raycast może trafić drugiego enemyego
- [ ] Muzzle flash rotation nie zawsze poprawne

### Fixed (Current Version)
- ✅ Shoot animation was cut off → dodane _shootAnimEndTime
- ✅ Reload pomijany → dodane _pendingReload queue
- ✅ State nie wraca po reloadzie → zapamiętanie stanu
- ✅ Trigger conflicts → ResetTrigger() antes cada trigger

---

## 📚 Resources for Future Development

### Learning
- https://docs.unity3d.com/Manual/AnimationStateMachines.html
- https://docs.unity3d.com/ScriptReference/Animator.SetTrigger.html
- NavMesh tutorial: https://www.youtube.com/...

### Assets
- Free weapon models: Sketchfab, TurboSquid
- Sound effects: Freesound.org, zapsplat.com
- Animation packs: Asset Store (search "weapon animations")

---

## 🎬 Notes for Next Dev

### Before starting work:
1. Przeczytaj `SYSTEM_BRONI_DOKUMENTACJA.md`
2. Run the game, test current state
3. Check Animator State Machine visually
4. Open Console, shoot & reload — sprawdź logi

### When modifying Animator:
1. Test transitions w Animator preview (play button)
2. Sprawdź Has Exit Time ustawienia
3. Test all conditions combinations
4. Play game, test 5+ razy z różnymi move states

### When adding new weapon:
1. Utwórz nowy WeaponData (ScriptableObject)
2. Skopiuj animation clips z innej broni (basis)
3. Edytuj animator state machine (add new weapon states)
4. Add to WeaponManager slots
5. Test w grze

---

**Document prepared for team handoff & future continuation.**

```
Version: 2.0
Last Modified: 2025
Status: Ready for Next Phase
```
