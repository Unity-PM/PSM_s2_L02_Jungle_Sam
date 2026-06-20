# Jungle Sam - Implementation Roadmap

## Założenia

Ten dokument opisuje plan technicznego wdrożenia vertical slice bez dodawania kodu na tym etapie. Nazwy systemów, skryptów, prefabów i GameObjectów są propozycjami do późniejszej implementacji.

Priorytetem jest zbudowanie czytelnego przepływu misji: start przy statku, kościół i cmentarz, kompleks Black Orchid ze szklarniami, finałowa lokacja UFO oraz abdukcja.

Na MVP śmierć gracza nie przeładowuje całej sceny przez `SceneManager.LoadScene`. Gracz respawnuje się w tej samej scenie przy ostatnim aktywnym checkpoincie, aktywny encounter jest resetowany, a żywi przeciwnicy z tego encountera są despawnowani.

## Systemy do dodania

- Mission flow controller do obsługi etapów misji, objective textów i przejść między arenami.
- Trigger zones dla wejść do lokacji, aktywacji fal, checkpointów i finałowej abdukcji.
- Encounter director do uruchamiania fal przeciwników zależnie od etapu.
- Spawn point groups dla zombie cywilów, żołnierzy Grom Division, personelu Black Orchid i MutantStalkera.
- Objective UI adapter do wyświetlania aktualnego celu bez mieszania logiki misji z UI.
- Pickup placement pass dla broni, amunicji i zasobów po każdym encounterze.
- UFO finale sequence dla beamu, blokady gracza, fade out i końca vertical slice.
- Environmental storytelling pass dla dokumentów, ciał, kontenerów Black Orchid, barykad Grom Division i śladów infekcji.
- Player health system dla HP gracza, śmierci i odzyskiwania pełnego HP po respawnie.
- Player death handler do obsługi fail state, blokady sterowania, komunikatu śmierci i wywołania respawnu.
- Checkpoint manager przechowujący ostatni aktywny checkpoint i punkt respawnu.
- Checkpoint volumes aktywowane po ukończeniu ważnych etapów misji, poza aktywną walką.
- Respawn points definiujące pozycję i rotację powrotu gracza.
- Death UI controller dla krótkiego ekranu śmierci, fade out/fade in i informacji o powrocie do checkpointu.
- Encounter reset on player death: despawn żywych przeciwników aktywnego encountera, reset liczników fali i przygotowanie encountera do ponownego startu.
- Ammo safety floor po respawnie: bronie i amunicja zostają zachowane, ale amunicja poniżej minimalnego progu jest uzupełniana, żeby uniknąć softlocka.

## Kolejność implementacji

1. Przygotować puste GameObjecty organizacyjne w scenie dla etapów misji.
2. Dodać trigger volumes dla pięciu etapów bez logiki walki.
3. Podpiąć objective texty i sprawdzić przechodzenie między etapami.
4. Dodać encounter spawn groups dla startu przy statku.
5. Rozszerzyć encountery o kościół i cmentarz.
6. Zbudować kompleks Black Orchid oraz szklarniowe encountery z miejscem dla MutantStalkera.
7. Przygotować finałową arenę UFO i ostatnią falę.
8. Dodać sekwencję abdukcji: beam, blokada kontroli, fade out, koniec vertical slice.
9. Dodać system śmierci gracza i respawnu w tej samej scenie.
10. Dodać checkpointy po głównych etapach: start, wyjście z nabrzeża, wyjście z kościoła, wyjście z Black Orchid i finał UFO.
11. Podpiąć reset aktywnego encountera po śmierci gracza.
12. Dodać minimalny próg amunicji po respawnie.
13. Wykonać pass pickupów, checkpointów i czytelności ścieżki.
14. Wykonać test całości od spawnu gracza do abdukcji.

## Proponowane nazwy GameObjectów w scenie

- `MissionRoot`
- `MissionStage_01_DockStart`
- `MissionStage_02_ChurchCemetery`
- `MissionStage_03_BlackOrchidGreenhouses`
- `MissionStage_04_UFOFinalArea`
- `MissionStage_05_Abduction`
- `Trigger_Stage01_StartWave`
- `Trigger_Stage02_ChurchGate`
- `Trigger_Stage03_BlackOrchidEntrance`
- `Trigger_Stage04_UFOSignal`
- `Trigger_Stage05_AbductionBeam`
- `Encounter_DockStart`
- `Encounter_ChurchCemetery`
- `Encounter_BlackOrchidCamp`
- `Encounter_UFOFinalHorde`
- `SpawnGroup_Zombie_Civilians`
- `SpawnGroup_Zombie_GromSoldiers`
- `SpawnGroup_Zombie_BlackOrchidStaff`
- `SpawnPoint_MutantStalker_Greenhouse`
- `ObjectiveMarker_DockExit`
- `ObjectiveMarker_Church`
- `ObjectiveMarker_BlackOrchidGate`
- `ObjectiveMarker_UFOFinal`
- `UFO_FinalObject`
- `UFO_BeamVolume`
- `UFO_AbductionCameraTarget`
- `Checkpoint_Start_Boat`
- `Checkpoint_DockExit`
- `Checkpoint_ChurchExit`
- `Checkpoint_BlackOrchidExit`
- `Checkpoint_UFOFinal`
- `RespawnPoint_Start_Boat`
- `RespawnPoint_DockExit`
- `RespawnPoint_ChurchExit`
- `RespawnPoint_BlackOrchidExit`
- `RespawnPoint_UFOFinal`
- `CheckpointManager`
- `DeathUI`

## Proponowane nazwy prefabów

- `PF_MissionTriggerBox`
- `PF_EncounterSpawnPoint`
- `PF_EncounterSpawnGroup`
- `PF_ObjectiveMarker`
- `PF_CheckpointVolume`
- `PF_RespawnPoint`
- `PF_DeathUI`
- `PF_Pickup_Ammo_Small`
- `PF_Pickup_Ammo_Large`
- `PF_Pickup_Health_Small`
- `PF_UFO_FinalObject`
- `PF_UFO_Beam`
- `PF_BlackOrchid_Crate`
- `PF_BlackOrchid_ResearchContainer`
- `PF_Grom_Barricade`
- `PF_Grom_AmmoCache`
- `PF_InfectionGrowth_Small`
- `PF_InfectionGrowth_Large`
- `PF_StoryProp_Document`
- `PF_StoryProp_BodyBag`

## Proponowane nazwy skryptów

Na późniejszy etap implementacji, bez tworzenia kodu w tym zadaniu:

- `MissionFlowController`
- `MissionStage`
- `MissionObjectiveDefinition`
- `MissionTriggerZone`
- `EncounterDirector`
- `EncounterDefinition`
- `SpawnPointGroup`
- `SpawnPoint`
- `ObjectiveUIBridge`
- `PlayerHealth`
- `PlayerDeathHandler`
- `CheckpointManager`
- `CheckpointVolume`
- `RespawnPoint`
- `DeathUIController`
- `FinaleUFOSequence`
- `PlayerControlLock`
- `ScreenFadeController`
- `StoryProp`

## Zasady respawnu i resetu encountera

- Checkpointy aktywują się po ukończeniu ważnych etapów, nie w środku aktywnej walki.
- Po śmierci gracz wraca do ostatniego aktywnego checkpointu w tej samej scenie.
- Po respawnie gracz odzyskuje pełne HP.
- Bronie gracza zostają zachowane.
- Amunicja zostaje zachowana, ale jeśli jest poniżej minimalnego progu, system uzupełnia ją do wartości anty-softlockowej.
- Pickupy na MVP nie muszą się resetować.
- Aktywny encounter zostaje zresetowany po śmierci gracza.
- Żywi przeciwnicy z aktualnego encountera zostają usunięci albo despawnowani.
- Finałowa sekwencja UFO po aktywacji beamu blokuje normalny fail state śmierci i kończy vertical slice.

## Lista checkpointów

- `Checkpoint_Start_Boat`: aktywny od startu poziomu.
- `Checkpoint_DockExit`: aktywowany po ukończeniu etapu nabrzeża.
- `Checkpoint_ChurchExit`: aktywowany po oczyszczeniu kościoła i cmentarza.
- `Checkpoint_BlackOrchidExit`: aktywowany po ukończeniu encountera Black Orchid i otwarciu drogi do finału.
- `Checkpoint_UFOFinal`: aktywowany przed finałową walką albo po zabezpieczeniu wejścia do areny UFO, ale przed beamem.

## Testy w Unity po każdym etapie

### Etap 1: Start przy statku

- Uruchomić scenę od domyślnego spawnu gracza.
- Sprawdzić, czy gracz widzi kierunek wyjścia z nabrzeża.
- Sprawdzić aktywację pierwszego objective textu.
- Sprawdzić trigger pierwszej fali.
- Sprawdzić, czy po walce gracz otrzymuje zasoby i może przejść dalej.
- Zabić gracza przed aktywacją `Checkpoint_DockExit` i potwierdzić powrót do `Checkpoint_Start_Boat`.
- Potwierdzić, że encounter startowy resetuje żywych przeciwników po śmierci.

### Etap 2: Kościół i cmentarz

- Wejść do triggera przy bramie lub kościele.
- Sprawdzić, czy spawn pointy nie tworzą przeciwników na oczach gracza.
- Przetestować walkę z kilku kierunków i czytelność osłon.
- Sprawdzić nagrodę po encounterze oraz odblokowanie ścieżki do kolejnej lokacji.
- Po ukończeniu kościoła potwierdzić aktywację `Checkpoint_ChurchExit`.
- Zabić gracza po kościele i sprawdzić respawn przy `Checkpoint_ChurchExit`.

### Etap 3: Kompleks Black Orchid / szklarnie

- Przejść przez bramę kompleksu i potwierdzić zmianę objective textu.
- Sprawdzić spawn zombie personelu oraz żołnierzy.
- Przetestować przestrzeń walki dla MutantStalkera.
- Upewnić się, że gracz ma możliwość ucieczki, przeładowania i zebrania amunicji.
- Sprawdzić, czy po encounterze otwiera się droga do finału.
- Zabić gracza podczas aktywnego encountera Black Orchid.
- Potwierdzić despawn żywych zombie i MutantStalkera z aktualnego encountera.
- Potwierdzić powrót do `Checkpoint_ChurchExit`, jeśli `Checkpoint_BlackOrchidExit` nie został jeszcze aktywowany.

### Etap 4: Finalna lokacja UFO

- Sprawdzić widoczność UFO lub jego światła przed rozpoczęciem ostatniej walki.
- Uruchomić finałową hordę i sprawdzić kierunki spawnów.
- Potwierdzić, że arena nie blokuje gracza geometrią ani pickupami.
- Sprawdzić checkpoint przed finałem.
- Zabić gracza w finale przed aktywacją beamu.
- Potwierdzić respawn przy `Checkpoint_UFOFinal` i reset finałowej hordy.

### Etap 5: Abdukcja

- Po zakończeniu finałowej walki wejść w trigger beamu.
- Sprawdzić blokadę ruchu i strzelania.
- Sprawdzić kierunek kamery, światło UFO, dźwięk i fade out.
- Potwierdzić, że sekwencja kończy vertical slice bez błędów konsoli.
- Powtórzyć test po restarcie z ostatniego checkpointu.
- Po rozpoczęciu beamu potwierdzić brak normalnej śmierci i brak standardowego respawnu.
- Potwierdzić, że sekwencja abdukcji ma priorytet nad fail state gracza.

## Testy systemu śmierci i checkpointów

- Śmierć przed pierwszym checkpointem progresywnym: gracz wraca do `Checkpoint_Start_Boat`, ma pełne HP, zachowuje bronie, a aktywny encounter startowy jest zresetowany.
- Śmierć po kościele: gracz wraca do `Checkpoint_ChurchExit`, nie do początku mapy.
- Śmierć podczas encountera Black Orchid: żywi przeciwnicy z aktualnego encountera są despawnowani, MutantStalker jest resetowany, a gracz wraca do ostatniego aktywnego checkpointu.
- Śmierć w finale przed beamem: gracz wraca do `Checkpoint_UFOFinal`, finałowa horda resetuje się, a amunicja jest podbijana do minimum tylko wtedy, gdy jest poniżej progu.
- Brak normalnej śmierci po rozpoczęciu sekwencji abdukcji: beam blokuje standardowy fail state i prowadzi do zakończenia vertical slice.

## Kryteria ukończenia vertical slice

- Gracz może przejść całą trasę od statku do finałowej lokacji UFO.
- Każdy etap ma jasny objective text, trigger i encounter.
- Każda walka ma zasoby potrzebne do kontynuowania gry.
- MutantStalker pojawia się jako wyraźny moment eskalacji.
- Śmierć gracza respawnuje go przy ostatnim aktywnym checkpoincie bez przeładowania sceny.
- Aktywny encounter resetuje się po śmierci i usuwa żywych przeciwników z aktualnej walki.
- Finałowy beam UFO blokuje gracza i kończy vertical slice abdukcją.
- Scena nie wymaga ręcznego debugowania w trakcie normalnego przejścia.
