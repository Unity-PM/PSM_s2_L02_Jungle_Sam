# Mission Stage 02 - Church / Cemetery

Dokument opisuje drugi etap misji w vertical slice Jungle Sam: dojście do kościoła, przeszukanie terenu, podniesienie dokumentu Grom Division, encounter na cmentarzu i skierowanie gracza do domu obok kościoła.

## Cel Etapu

Po ukończeniu pierwszej areny przy statku gracz ma zostać poprowadzony w stronę kościoła i cmentarza. Na miejscu znajduje dokument Grom Division. Dokument ujawnia, że Black Orchid przeniosła część materiału biologicznego do zabudowań obok kościoła. Po zamknięciu popupu dokumentu startuje encounter przy kościele/cmentarzu.

## Flow

1. Po ukończeniu pierwszej areny:
   - Main objective: `Dotrzyj do kościoła i cmentarza`
   - Secondary objective: `Podążaj drogą przez zalane nabrzeże`

2. Po wejściu do obszaru kościoła/cmentarza:
   - obiekt: `Trigger_ChurchAreaEntered`
   - komponent: `ObjectiveTriggerZone`
   - Main objective: `Przeszukaj teren kościoła`
   - Secondary objective: `Znajdź ślady oddziału Grom Division`

3. W kościele gracz znajduje:
   - obiekt: `INT_Church_MilitaryDocument`
   - komponent: `StoryPickupInteractable`
   - prompt: `[E] PODNIEŚ DOKUMENT`

4. Po podniesieniu dokumentu:
   - dokument znika wizualnie,
   - collider interakcji zostaje wyłączony,
   - pojawia się story popup,
   - encounter jeszcze nie startuje.

5. Po kliknięciu `KONTYNUUJ` w popupie:
   - popup znika,
   - input gracza wraca,
   - odpala się `StoryPickupInteractable.onPickedUp`,
   - objective zmienia się na walkę,
   - startuje `Encounter_ChurchCemetery`.

6. Podczas encountera:
   - Main objective: `Przetrwaj atak przy kościele`
   - Secondary objective: `Utrzymaj pozycję na terenie cmentarza`

7. Po ukończeniu encountera:
   - aktywuje się `Checkpoint_ChurchExit`,
   - pokazuje się notification `CHECKPOINT AKTYWNY`,
   - objective prowadzi do domu obok kościoła:
     - Main objective: `Sprawdź zabudowania wskazane w raporcie`
     - Secondary objective: `Przejdź do domu obok kościoła`

## Proponowana Hierarchia Sceny

```text
MissionStage_02_ChurchCemetery
├── Triggers
│   └── Trigger_ChurchAreaEntered
├── Interactables
│   └── INT_Church_MilitaryDocument
├── Encounter
│   └── Encounter_ChurchCemetery
├── SpawnPoints
│   ├── SP_Church_Zombie_01
│   ├── SP_Church_Zombie_02
│   ├── SP_Cemetery_Zombie_01
│   ├── SP_Cemetery_Zombie_02
│   ├── SP_Cemetery_Mutant_01
│   └── SP_Cemetery_Mutant_02
├── Checkpoints
│   ├── Checkpoint_ChurchExit
│   └── RespawnPoint_ChurchExit
└── NextObjective
    └── ObjectiveMarker_HouseNearChurch
```

## Trigger_ChurchAreaEntered

Dodaj obiekt z colliderem `Is Trigger`.

Komponent:

- `ObjectiveTriggerZone`

Ustawienia:

- `Trigger Id`: `Trigger_ChurchAreaEntered`
- `Required Player Tag`: `Player`
- `Activate Once`: true
- `Objective Text`: `Przeszukaj teren kościoła`
- `Secondary Objective Text`: `Znajdź ślady oddziału Grom Division`
- `Show Notification`: true
- `Notification Text`: `CEL ZAKTUALIZOWANY`

## INT_Church_MilitaryDocument

Użyj istniejącego `StoryPickupInteractable`. Nie twórz drugiego systemu interakcji.

Komponenty:

- `Collider` z `Is Trigger`
- `StoryPickupInteractable`
- `ObjectiveOnStoryPickup`
- `ArenaStartOnStoryPickup`

`StoryPickupInteractable`:

- `Pickup Id`: `StoryPickup_Church_MilitaryDocument`
- `Display Name`: `Dokument Grom Division`
- `Interaction Prompt`: `E - Podnieś dokument`
- `Interaction Key`: `E`
- `Interaction Text`: `Podnieś dokument`
- `Show Story Popup Before Events`: true
- `Popup Title`: `DOKUMENT GROM DIVISION`
- `Popup Subtitle`: `Raport terenowy // Sektor kościoła`
- `Popup Button Text`: `KONTYNUUJ`
- `Register With Encounter Reset Service`: true
- `Reset On Encounter Reset`: true
- `Linked Arena Id`: `Encounter_ChurchCemetery`

Popup body:

```text
Zabezpieczono fragment raportu Grom Division.

Black Orchid przeniosła część materiału biologicznego do zabudowań obok kościoła.
Oddział zabezpieczający zgłaszał wzrost agresji zainfekowanych w rejonie cmentarza.

Ostatni wpis:
«Nie otwierać domu bez wsparcia. Sygnał nad strefą wpływa na mutanty.»
```

`ObjectiveOnStoryPickup`:

- `Objective Text`: `Przetrwaj atak przy kościele`
- `Secondary Objective Text`: `Utrzymaj pozycję na terenie cmentarza`
- `Show Notification`: true
- `Notification Text`: `CEL ZAKTUALIZOWANY`
- `Reset Objective On Encounter Reset`: true
- `Reset Objective Text`: `Przeszukaj teren kościoła`
- `Reset Secondary Objective Text`: `Znajdź ślady oddziału Grom Division`
- `Linked Arena Id`: `Encounter_ChurchCemetery`

`ArenaStartOnStoryPickup`:

- `Arena Id`: `Encounter_ChurchCemetery`
- `Start Method Name`: `StartArena`
- `Start Only Once`: true
- można podpiąć `Arena Encounter Controller` bezpośrednio, albo zostawić auto-find po `Arena Id`.

Eventy `StoryPickupInteractable.onPickedUp`:

1. `ObjectiveOnStoryPickup.UpdateObjective()`
2. `ArenaStartOnStoryPickup.StartArenaFromStoryPickup()`

Ważne: jeśli `Show Story Popup Before Events` jest true, te eventy odpalą się dopiero po kliknięciu `KONTYNUUJ`.

## Story Popup

Jeśli w scenie nie ma popupu, utwórz UI panel i dodaj:

- `StoryItemPopupUI`
- `CanvasGroup`
- `Button`
- TMP teksty:
  - `TitleText`
  - `SubtitleText`
  - `BodyText`
  - `ButtonText`

`StoryItemPopupUI` może znaleźć pola automatycznie po nazwach dzieci, ale bezpieczniej podpiąć je ręcznie w Inspectorze.

Opcjonalnie podepnij:

- `PlayerControlLock`

Jeśli `Lock Player While Open` jest true, popup zablokuje input gracza do czasu kliknięcia `KONTYNUUJ`.

## Encounter_ChurchCemetery

Komponenty:

- `ArenaEncounterController`
- `WaveSpawner`

Ustawienia `ArenaEncounterController`:

- `Arena Id`: `Encounter_ChurchCemetery`
- `Start On Player Enter`: false
- `Complete Once`: true
- `Activate Checkpoint On Complete`: false, jeśli używasz adaptera `CheckpointActivationOnEncounterComplete`
- `Close Gates On Start`: false
- `Open Gates On Complete`: false
- `Close Gates On Death Reset`: false albo true bez znaczenia, jeśli nie ma bram
- `Arena Gates`: puste
- `Register With Encounter Reset Service`: true

Nie dodawaj blockerów dla tego encountera.

`WaveSpawner`:

- `Start On Play`: false
- fale i przeciwników ustaw ręcznie w Inspectorze,
- spawn pointy ustaw ręcznie:
  - `SP_Church_Zombie_01`
  - `SP_Church_Zombie_02`
  - `SP_Cemetery_Zombie_01`
  - `SP_Cemetery_Zombie_02`
  - `SP_Cemetery_Mutant_01`
  - `SP_Cemetery_Mutant_02`

Proponowane grupy organizacyjne:

- `SpawnGroup_Church_Zombies`
- `SpawnGroup_Cemetery_Zombies`
- `SpawnGroup_Church_Mutants`

## Eventy Po Ukończeniu Encountera

Na `Encounter_ChurchCemetery` w `ArenaEncounterController.onArenaCompleted` ustaw:

1. `CheckpointActivationOnEncounterComplete.ActivateCheckpoint()`
2. `ObjectiveOnEncounterComplete.ApplyObjective()`

`CheckpointActivationOnEncounterComplete`:

- `Checkpoint`: `Checkpoint_ChurchExit`
- `Show Notification`: true
- `Notification Text`: `CHECKPOINT AKTYWNY`

`ObjectiveOnEncounterComplete`:

- `Objective Text`: `Sprawdź zabudowania wskazane w raporcie`
- `Secondary Objective Text`: `Przejdź do domu obok kościoła`
- `Show Notification`: true
- `Notification Text`: `CEL ZAKTUALIZOWANY`

## Checkpoint_ChurchExit

Obiekty:

- `Checkpoint_ChurchExit`
- `RespawnPoint_ChurchExit`

`Checkpoint_ChurchExit`:

- collider może być triggerem, ale dla tego flow checkpoint aktywuje adapter po ukończeniu encountera,
- komponent: `CheckpointVolume`,
- `Checkpoint Id`: `Checkpoint_ChurchExit`,
- `Respawn Point`: `RespawnPoint_ChurchExit`,
- `Activate Once`: true.

`RespawnPoint_ChurchExit`:

- komponent: `RespawnPoint`,
- ustaw pozycję i rotację, gdzie gracz ma wrócić po śmierci po ukończeniu encountera.

## Reset Po Śmierci Przed Ukończeniem Encountera

Jeśli gracz podniesie dokument, zamknie popup, rozpocznie encounter i zginie przed ukończeniem:

- `EncounterResetService` resetuje aktywne encountery,
- `WaveSpawner` czyści żywych przeciwników i wraca do idle,
- `StoryPickupInteractable` przywraca dokument,
- collider dokumentu wraca,
- visualRoot dokumentu wraca,
- `ObjectiveOnStoryPickup` cofa objective do przeszukania kościoła,
- `Checkpoint_ChurchExit` nie aktywuje się.

Jeśli encounter został ukończony:

- dokument nie wraca,
- objective zostaje na celu prowadzącym do domu,
- `Checkpoint_ChurchExit` pozostaje aktywny,
- śmierć respawnuje gracza przy `RespawnPoint_ChurchExit`.

## Testy

1. Po pierwszej arenie HUD pokazuje `Dotrzyj do kościoła i cmentarza`.
2. Wejście w `Trigger_ChurchAreaEntered` zmienia cel na `Przeszukaj teren kościoła`.
3. Podejście do dokumentu pokazuje `[E] PODNIEŚ DOKUMENT`.
4. Podniesienie dokumentu pokazuje story popup.
5. Dopóki popup jest otwarty, encounter nie startuje.
6. Po kliknięciu `KONTYNUUJ` popup znika, objective zmienia się na walkę i startuje `Encounter_ChurchCemetery`.
7. Śmierć przed ukończeniem encountera przywraca dokument i objective sprzed walki.
8. Po ukończeniu encountera aktywuje się `Checkpoint_ChurchExit`.
9. Po ukończeniu encountera objective zmienia się na `Sprawdź zabudowania wskazane w raporcie`.
10. Śmierć po ukończeniu encountera respawnuje gracza przy `RespawnPoint_ChurchExit` i nie cofa dokumentu.
11. Console bez `NullReferenceException`.
