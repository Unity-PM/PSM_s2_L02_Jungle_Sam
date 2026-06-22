# Jungle Sam - technical reference

Ten plik jest aktualnym technicznym zrodlem prawdy dla glownego kodu gameplayowego. Zastapil starsze, rozbite dokumenty o broni, ammo, architekturze, quick reference, statusie i shot feedbacku.

## Glowne systemy i sciezki

```text
Assets/JungleSam/Scripts/Player/
Assets/JungleSam/Scripts/Weapon/
Assets/JungleSam/Scripts/Enemy/
Assets/JungleSam/Scripts/Enemies/
Assets/JungleSam/Scripts/Spawning/
Assets/JungleSam/Scripts/Pickups/
Assets/JungleSam/Scripts/UI/
Assets/JungleSam/Scripts/Combat/
Assets/JungleSam/Core/
Assets/JungleSam/UI/HUD/
Assets/JungleSam/Enemies/MutantStalker/
```

## Weapon system

`WeaponBase` jest raycastowy. Strzal idzie ze srodka kamery FPS:

```text
WeaponBase.TryShoot()
 -> sprawdza cooldown, ammo i reload
 -> WeaponBase.Shoot()
    -> odejmuje naboj
    -> odpala trigger animatora Shoot
    -> odpala shot feedback z opcjonalnym opoznieniem
    -> wykonuje raycast
    -> zadaje damage przez IDamageable albo fallback EnemyAI
```

Nie zmieniac bez potrzeby:

- inputu w `WeaponBase.Update()`,
- reloadu,
- logiki ammo,
- raycast damage,
- triggerow animatora.

### WeaponData

`WeaponData` zawiera balans i feedback broni:

```csharp
public string weaponName;
public GameObject weaponPrefab;
public AmmoCategory ammoCategory;
public float damage;
public float fireRate;
public float range;
public int maxAmmo;
public int maxReserveAmmo;
public float reloadTime;
public float shootAnimationDuration;
public bool isAutomatic;
public GameObject muzzleFlashPrefab;
public AudioClip shootSound;
public AudioClip[] shootSounds;
public Vector2 shootPitchRange;
public Vector2 shootVolumeRange;
public float shotFeedbackDelay;
```

Zasady:

- `shootSound` zostaje fallbackiem dla pojedynczego klipu.
- `shootSounds` sluzy do losowania wariantow, szczegolnie dla full auto.
- `shotFeedbackDelay` opoznia dzwiek i flash wzgledem logicznego strzalu.
- Pistolet ma `shotFeedbackDelay = 0.08`.
- AK ma `shotFeedbackDelay = 0`.

### Audio strzalu

Folder dzwiekow:

```text
Assets/JungleSam/Audio/jungle_sam_user_weapon_sfx_processed/
```

Pistolet:

- `pistol_9mm_user_trimmed_game.wav`
- przypiety jako `PistolData.shootSound`

AK:

- `rifle_user_single_01_short.wav`
- `rifle_user_single_02_short.wav`
- `rifle_user_single_03_short.wav`
- `rifle_user_single_04_short.wav`
- `rifle_user_single_05_short.wav`
- `rifle_user_single_06_short.wav`
- `rifle_user_single_07_short.wav`
- `rifle_user_single_08_short.wav`

AK uzywa tablicy `shootSounds`, a `WeaponBase` odpala dzwiek przez `AudioSource.PlayOneShot(clip, volume)`, zeby full auto nie ucinalo poprzedniego strzalu.

`WeaponBase` zabezpiecza `AudioSource` runtime:

```csharp
weaponAudioSource.enabled = true;
weaponAudioSource.playOnAwake = false;
weaponAudioSource.loop = false;
weaponAudioSource.spatialBlend = 0f;
weaponAudioSource.dopplerLevel = 0f;
```

## Muzzle flash

Muzzle flash jest przypiety przez `WeaponData.muzzleFlashPrefab`.

Aktualny prefab:

```text
Assets/JungleSam/Prefabs/Weapons/Effects/FX_MuzzleFlash_Video.prefab
```

Bazowy asset:

```text
Assets/JungleSam/Prefabs/Weapons/MuzzleFlashSide-012-ProRes/FX Elements - Muzzle Flash - Side - 012/MuzzleFlash-side-012.mov
```

Poniewaz asset jest plikiem `.mov`, a nie gotowym prefabem Unity, dodany zostal wrapper:

```text
Assets/JungleSam/Scripts/Weapon/MuzzleFlashVideoEffect.cs
```

Wrapper runtime tworzy:

- quad,
- `RenderTexture`,
- material addytywny,
- `VideoPlayer`,
- krotki `Point Light`.

Aktualne parametry prefabu:

```yaml
size: {x: 1.15, y: 0.55}
localOffset: {x: -0.48, y: -0.02, z: 0.03}
destroyAfter: 0.42
faceMainCamera: 1
randomRollRange: {x: -3, y: 3}
flipHorizontal: 0
tint: {r: 2.4, g: 1.75, b: 1.05, a: 1}
createProceduralFlash: 0
createSparks: 0
```

Proceduralny fallback i sztuczne iskry sa wylaczone, bo nie pasowaly stylistycznie do pobranego efektu.

Jesli smuga idzie w zla strone, przelaczyc:

```text
FX_MuzzleFlash_Video -> flipHorizontal = true
```

Jesli flash jest zbyt maly lub przesuniety:

- `size` zmienia wielkosc,
- `localOffset` zmienia polozenie wzgledem lufy,
- `tint` wzmacnia jasnosc,
- `destroyAfter` kontroluje czas zycia efektu.

## Punkty lufy

`WeaponBase` ma:

```csharp
[SerializeField] private Transform muzzlePoint;
```

Jesli pole nie jest podpiete, kod szuka w childach:

```text
MuzzlePoint
Barrel_end_end
Barrel_end
BarrelEnd
wpn_bullet_end
wpn_bullet
Barrel
```

W `Player 1 1.prefab`:

- pistolet: `muzzlePoint = Barrel_end`,
- AK: `muzzlePoint = wpn_bullet_end`.

## Ammo

`WeaponBase` trzyma:

- `_currentAmmo` - magazynek,
- `_reserveAmmo` - zapas.

Reload przenosi ammo z reserve do magazynka:

```text
ammoNeeded = maxAmmo - currentAmmo
ammoToLoad = min(ammoNeeded, reserveAmmo)
currentAmmo += ammoToLoad
reserveAmmo -= ammoToLoad
```

`AmmoCategory`:

```csharp
public enum AmmoCategory
{
    PistolSmg,
    Rifle
}
```

`WeaponManager.AddAmmoToCategory(ammoCategory, amount)` dodaje ammo wszystkim broniom z danej kategorii.

## HUD

HUD jest normalnym Canvasem, nie statycznym obrazkiem.

Glowne elementy:

- `ObjectivePanel`
- `PlayerStatsPanel`
- `AmmoPanel`
- `InteractionPromptPanel`
- `NotificationPanel`
- `TopLeft_StatusPanel`
- `TopRight_BlackOrchidPanel`
- `CenterCrosshair`

Glowne skrypty:

- `GameplayHUDController`
- `PlayerHealthHUDBinder`
- `WeaponAmmoHUDBinder`
- `InteractionPromptUI`
- `HUDNotificationUI`
- `GameplayHUDPrefabBuilder`

Wazne:

- `GameplayHUDPrefabBuilder` powinien byc narzedziem recznym, nie systemem nadpisujacym HUD przy Play Mode.
- `GameplayHUDController` nie powinien resetowac recznie ustawionych tekstow objective przy starcie, chyba ze swiadomie wlaczone jest stosowne pole default.
- Dla AK / AK-37 / AK-47 ammo HUD pokazuje `7.62x39mm`.
- Pistol/smg pokazuje `9x19mm`.

## Health, armor, death i respawn

Glowne klasy:

- `PlayerHealth`
- `PlayerStats`
- `PlayerDeathHandler`
- `PlayerControlLock`
- `CheckpointManager`
- `CheckpointVolume`
- `RespawnPoint`
- `DeathUIController`
- `EncounterResetService`

Flow smierci:

```text
PlayerHealth.TakeDamage()
 -> HP <= 0
 -> PlayerHealth.OnDied
 -> PlayerDeathHandler.HandlePlayerDied()
 -> blokada kontroli
 -> Death UI
 -> EncounterResetService.ResetActiveEncounter()
 -> CheckpointManager.RespawnPlayer(playerRoot)
 -> teleport do checkpointu
 -> RestoreFullHealth()
 -> opcjonalne przywrocenie armor
 -> odblokowanie kontroli
```

Nie uzywac `SceneManager.LoadScene` jako domyslnego respawnu MVP.

## Encounter reset

Jesli gracz ginie w aktywnej arenie przed jej ukonczeniem:

- aktywny encounter resetuje fale,
- zywi przeciwnicy sa usuwani/despawnowani,
- story pickup moze wrocic,
- objective moze cofnac sie do stanu sprzed areny,
- bramy/blockery nie zostaja w stanie po sukcesie.

Jesli arena byla ukonczona i checkpoint aktywowany:

- radio nie wraca,
- objective nie cofa sie,
- bramy zostaja w stanie po ukonczeniu.

Glowne klasy:

- `EncounterResetService`
- `ArenaEncounterController`
- `WaveSpawner`
- `StoryPickupInteractable`
- `ObjectiveOnStoryPickup`
- `ArenaGateController`

## MutantStalker

Pierwszoplanowy silniejszy przeciwnik na bazie MonsterMutant7.

Pracowac tylko w:

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

Builder:

```text
Tools > Jungle Sam > Enemies > Build Mutant Stalker Animator Controller
```

Nie edytowac oryginalnego MonsterMutant7 Animator Controller.

Wazne wartosci:

```text
Attack Animation Lock: 1.1
Hit Reaction Lock: 0.28
Hit Reaction Cooldown: 0.35
Rage Animation Lock: 1.2
```

## Testy techniczne

Po zmianach odpalic:

```powershell
dotnet build .\Assembly-CSharp.csproj
git diff --check
```

W Unity sprawdzic:

- Console bez compile errors i missing scripts,
- pistolet: dzwiek, flash, ammo, reload,
- AK full auto: dzwieki nie ucinaja sie,
- brak dzwieku/flasha przy pustym magazynku,
- damage na zombie,
- damage na MutantStalkerze,
- smierc i respawn,
- reset aktywnego encountera,
- HUD: HP, armor, ammo, objective, prompt, notification.

