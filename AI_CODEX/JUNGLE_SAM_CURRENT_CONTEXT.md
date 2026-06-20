# Jungle Sam - aktualny kontekst projektu

Ten plik jest skrótem aktualnego stanu prac nad Unity 6.3 URP FPS Horde Shooter **Jungle Sam**. Można go wkleić do nowego chatu, żeby szybko przekazać kontekst fabuły, systemów i obecnego vertical slice.

## Założenia projektu

- Silnik: Unity 6.3, URP.
- Gatunek: FPS horde shooter.
- Klimat: militarno-biologiczny horror akcji z elementem UFO.
- Ton: zalana strefa, dżungla, zniszczone obiekty wojskowe, badania biologiczne, tajna operacja.
- Główne frakcje:
  - **Black Orchid** - tajna organizacja prowadząca badania biologiczne.
  - **Grom Division** - wojskowa jednostka zabezpieczająca teren i prowadząca działania w strefie.

## Fabuła

Black Orchid prowadziła badania biologiczne w zalanej strefie. Grom Division zabezpieczała teren, ale eksperyment wymknął się spod kontroli. Infekcja rozprzestrzeniła się na personel badawczy, żołnierzy oraz ludzi znajdujących się w strefie.

Zombie to zainfekowani ludzie z obszaru katastrofy. MutantStalker jest silniejszym efektem mutacji, prawdopodobnie wynikiem eksperymentu bojowego. UFO jest powiązane ze źródłem infekcji i ma prowadzić do finału vertical slice.

Na końcu vertical slice gracz ma zostać zablokowany przez beam UFO i porwany. Tego finału jeszcze nie implementowaliśmy.

## Lokacje mapy

Mapa ma prowadzić przez:

- nabrzeże, statek, zalaną drogę,
- opuszczony kościół i cmentarz,
- domki, budynki i opuszczone szklarnie,
- obóz Black Orchid,
- finałową wyspę albo budynek z UFO nad lokacją.

Aktualnie pracujemy głównie nad pierwszym etapem przy statku/nabrzeżu.

## Aktualny vertical slice

Obecny fragment gry zaczyna się przy statku/nabrzeżu. Gracz znajduje radio Grom Division przy małej łódce. Podniesienie radia:

1. pokazuje/powinno pokazywać prompt interakcji,
2. aktualizuje cel misji,
3. uruchamia pierwszą arenę przy statku,
4. zamyka/utrzymuje blockery areny podczas walki,
5. po ukończeniu fal otwiera przejście,
6. aktywuje checkpoint po arenie.

Pierwsza arena ma fale przeciwników i działa z istniejącym `WaveSpawner`.

## Radio Grom Division

Radio jest pierwszym fabularnym pickupem.

Obiekt sceniczny powinien mieć:

- `StoryPickupInteractable`
- `ArenaStartOnStoryPickup`
- `ObjectiveOnStoryPickup`
- collider ustawiony jako `Is Trigger`

Podniesienie radia powinno:

- ukryć radio wizualnie,
- wyłączyć collider interakcji,
- odpalić eventy `onPickedUp`,
- uruchomić arenę,
- zaktualizować objective,
- pokazać notification HUD, jeśli HUD jest podpięty.

Ważne: radio nie powinno być niszczone przez `Destroy(gameObject)`, bo bez reloadu sceny musi dać się przywrócić po śmierci gracza przed ukończeniem areny.

## System śmierci i checkpointów

Nie resetujemy całej sceny przez `SceneManager.LoadScene`. Respawn odbywa się w tej samej scenie.

Główne klasy:

- `PlayerHealth`
- `PlayerDeathHandler`
- `PlayerControlLock`
- `CheckpointManager`
- `CheckpointVolume`
- `RespawnPoint`
- `DeathUIController`
- `EncounterResetService`

Po śmierci:

1. `PlayerHealth.OnDied` odpala `PlayerDeathHandler`.
2. `PlayerDeathHandler` blokuje kontrolę gracza przez `PlayerControlLock`.
3. Pokazuje Death UI, jeśli jest podpięte.
4. Woła `EncounterResetService.ResetActiveEncounter()`.
5. Po opóźnieniu woła `CheckpointManager.RespawnPlayer(playerRoot)`.
6. `CheckpointManager` teleportuje gracza do ostatniego checkpointu albo fallback `RespawnPoint`.
7. Resetuje velocity Rigidbody, jeśli istnieje.
8. Obsługuje teleport CharacterController przez chwilowe disable/enable.
9. Przywraca HP przez `PlayerHealth.RestoreFullHealth()`.
10. Przywraca armor przez `PlayerStats.SetArmor(respawnArmor)`, jeśli `restoreArmorOnRespawn` jest aktywne.
11. Odblokowuje kontrolę gracza.

## Checkpointy planowane

Docelowe checkpointy:

- `Checkpoint_Start_Boat`
- `Checkpoint_DockExit`
- `Checkpoint_ChurchExit`
- `Checkpoint_BlackOrchidExit`
- `Checkpoint_UFOFinal`

Aktualnie najważniejszy jest checkpoint po pierwszej arenie przy statku. Ma aktywować się dopiero po ukończeniu areny, nie po samym podniesieniu radia.

## Reset encountera po śmierci

To jest ważny aktualny temat.

Jeśli gracz:

1. podniesie radio,
2. arena wystartuje,
3. gracz zginie przed ukończeniem areny,

to po respawnie powinno być:

- radio znowu dostępne,
- objective cofnięty do celu sprzed areny,
- arena niezaliczona,
- fale wyczyszczone i gotowe do ponownego startu,
- blockery/bramy nie powinny być w stanie "arena ukończona".

Jeśli arena została ukończona i checkpoint po arenie aktywowany, wtedy reset nie powinien cofać radia ani objective.

Klasy biorące udział:

- `EncounterResetService`
- `ArenaEncounterController`
- `WaveSpawner`
- `StoryPickupInteractable`
- `ObjectiveOnStoryPickup`

`WaveSpawner` implementuje `IEncounterResettable` i potrafi zatrzymać fale oraz usunąć zarejestrowanych przeciwników. `StoryPickupInteractable` i `ObjectiveOnStoryPickup` również są resettable, żeby cofać stan fabularny po śmierci przed ukończeniem areny.

## Arena przy statku

Arena używa:

- `ArenaEncounterController`
- `WaveSpawner`
- `ArenaGateController`
- `EncounterResetService`

Założenia:

- arena nie powinna startować automatycznie po wejściu w trigger, jeśli start ma być fabularny przez radio,
- `ArenaStartOnStoryPickup` wywołuje `ArenaEncounterController.StartArena()`,
- blockery/bramy mają zamknąć przejście podczas walki,
- po ukończeniu fal `ArenaEncounterController.CompleteArena()` otwiera bramy,
- po ukończeniu areny aktywuje checkpoint.

Ważne pola:

- `startOnPlayerEnter` powinno być wyłączone dla areny startowanej radiem,
- `closeGatesOnStart` powinno być włączone,
- `openGatesOnComplete` powinno być włączone,
- `closeGatesOnDeathReset` powinno być włączone.

## HUD

HUD jest normalnym Canvasem, nie statycznym obrazkiem.

Główne elementy:

- `ObjectivePanel`
- `PlayerStatsPanel`
- `AmmoPanel`
- `InteractionPromptPanel`
- `NotificationPanel`
- `TopLeft_StatusPanel`
- `TopRight_BlackOrchidPanel`
- `CenterCrosshair`

Główne skrypty:

- `GameplayHUDController`
- `PlayerHealthHUDBinder`
- `WeaponAmmoHUDBinder`
- `InteractionPromptUI`
- `HUDNotificationUI`
- `GameplayHUDPrefabBuilder`

Ważne: `GameplayHUDPrefabBuilder` nie powinien automatycznie nadpisywać ręcznych zmian HUD-u przy Play Mode. Ma działać tylko ręcznie przez context menu, chyba że świadomie włączone jest `autoEnsureOnAwake`.

`GameplayHUDController` nie powinien automatycznie resetować tekstów objective przy starcie gry. Ręczne teksty w prefabie/scenie mają zostać zachowane. Domyślne wartości można wymusić tylko przez `ApplyDefaultState()` albo opcję `applyDefaultValuesOnStart`, jeśli ktoś ją świadomie włączy.

## HP i Armor

HP:

- `PlayerHealth` przechowuje `maxHealth`, `currentHealth`,
- `PlayerHealth.OnHealthChanged` aktualizuje HUD przez `PlayerHealthHUDBinder`,
- `GameplayHUDController.SetHealth(current, max)` aktualizuje tekst i pasek.

Armor:

- armor jest w `PlayerStats`,
- `PlayerStats.Armor` i `PlayerStats.MaxArmor` są używane przez HUD,
- `PlayerStats.PlayerStatsChanged` informuje HUD o zmianach,
- po respawnie `CheckpointManager` może przywrócić armor do `respawnArmor`.

Aktualnie HP i Armor spadają na HUD i działają.

## Ammo HUD

`WeaponAmmoHUDBinder` aktualizuje HUD na podstawie aktualnej broni.

Ważna reguła:

- dla broni typu AK / AK-37 / AK-47 HUD ma pokazywać amunicję `7.62x39mm`,
- inne rifle mogą pokazywać `5.56 NATO`,
- pistol/smg pokazuje `9x19mm`.

Fallback ammo nie powinien nadpisywać ręcznie ustawionych tekstów, jeśli nie znaleziono broni, chyba że włączone jest `showFallbackWhenNoWeapon`.

## Przepływ informacji

### Podniesienie radia

`StoryPickupInteractable.PickUp()`

-> `onPickedUp.Invoke()`

-> `ArenaStartOnStoryPickup.StartArenaFromStoryPickup()`

-> `ArenaEncounterController.StartArena()`

-> `WaveSpawner.StartSpawner()`

oraz:

-> `ObjectiveOnStoryPickup.UpdateObjective()`

-> `GameplayHUDController.SetObjective()`

-> `GameplayHUDController.ShowNotification()`

### Śmierć gracza

`PlayerHealth.TakeDamage()`

-> HP spada do 0

-> `PlayerHealth.OnDied`

-> `PlayerDeathHandler.HandlePlayerDied()`

-> `EncounterResetService.ResetActiveEncounter()`

-> reset areny / fal / radia / objective, jeśli arena nieukończona

-> `CheckpointManager.RespawnPlayer()`

-> teleport do checkpointu

-> restore HP i armor

### Ukończenie areny

`WaveSpawner` kończy ostatnią falę

-> `SpawnerFinished`

-> `ArenaEncounterController.CompleteArena()`

-> otwarcie bram/blockerów

-> aktywacja checkpointu po arenie

## Ważne zasady dalszej pracy

- Nie używać `SceneManager.LoadScene` do respawnu MVP.
- Nie niszczyć pickupów fabularnych na stałe, jeśli mają być resetowane po śmierci.
- Nie pozwalać builderom typu HUD/radio nadpisywać ręcznych zmian przy Play Mode.
- `GameplayHUDPrefabBuilder` i `GromDivisionRadioVisual` mają być narzędziami ręcznymi, nie aktywnymi systemami gameplayowymi.
- Nie ruszać bez potrzeby `EnemyAI`, `WeaponBase`, `MutantStalkerAI`, `WaveSpawner`.
- Zmiany robić modularnie, przez małe komponenty.
- Preferować `[SerializeField] private` zamiast publicznych pól.
- New Input System: nie używać `UnityEngine.Input.GetKeyDown`.

## Obecne rzeczy do dopilnowania

- Przetestować pełny flow: podniesienie radia -> arena -> śmierć przed ukończeniem -> respawn -> radio wraca -> objective cofnięty -> blockery nie są otwarte jak po sukcesie.
- Przetestować flow po ukończeniu areny: checkpoint aktywny -> śmierć -> radio nie wraca, objective nie cofa się do starego, blockery pozostają w stanie po ukończeniu.
- Upewnić się, że `Arena_DockStart` jest spójnie wpisane w `ArenaEncounterController`, `ArenaStartOnStoryPickup`, `StoryPickupInteractable`, `ObjectiveOnStoryPickup`.
- Dopiąć później minimum ammo refill po respawnie.
- Dopracować drugi etap misji: przejście do kościoła/cmentarza.
